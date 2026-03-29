using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Dtos;
using WebApplication1.Entities;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize]
public sealed class ChatController : ControllerBase
{
    private const int ReplyPreviewMax = 160;
    private readonly AppDbContext _db;
    private readonly ILiveNotifier _live;

    public ChatController(AppDbContext db, ILiveNotifier live)
    {
        _db = db;
        _live = live;
    }

    [HttpGet("threads")]
    public async Task<ActionResult<IReadOnlyList<ChatThreadDto>>> Threads(CancellationToken cancellationToken)
    {
        if (User.IsInRole(nameof(UserRole.Admin)))
        {
            return Forbid();
        }

        if (User.IsInRole(nameof(UserRole.Customer)))
        {
            var customerId = GetUserId();
            var summaries = await _db.ChatMessages.AsNoTracking()
                .Where(m => m.CustomerUserId == customerId)
                .GroupBy(m => m.BusinessId)
                .Select(g => new { BusinessId = g.Key, LastAt = g.Max(m => m.SentAt) })
                .ToListAsync(cancellationToken);

            var businessIds = summaries.Select(s => s.BusinessId).ToList();
            var businesses = await _db.Businesses.AsNoTracking()
                .Where(b => businessIds.Contains(b.Id))
                .ToDictionaryAsync(b => b.Id, cancellationToken);

            var customerEmail = await _db.Users.AsNoTracking()
                .Where(u => u.Id == customerId)
                .Select(u => u.Email)
                .FirstAsync(cancellationToken);

            var result = new List<ChatThreadDto>();
            foreach (var s in summaries.OrderByDescending(x => x.LastAt))
            {
                businesses.TryGetValue(s.BusinessId, out var b);
                var preview = await _db.ChatMessages.AsNoTracking()
                    .Where(m => m.BusinessId == s.BusinessId && m.CustomerUserId == customerId)
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => m.Body)
                    .FirstOrDefaultAsync(cancellationToken);
                result.Add(new ChatThreadDto(s.BusinessId, b?.Name ?? "?", customerId, customerEmail, s.LastAt, preview));
            }

            return Ok(result);
        }

        if (User.IsInRole(nameof(UserRole.Business)))
        {
            if (!TryGetBusinessId(out var businessId))
            {
                return Forbid();
            }

            var summaries = await _db.ChatMessages.AsNoTracking()
                .Where(m => m.BusinessId == businessId)
                .GroupBy(m => m.CustomerUserId)
                .Select(g => new { CustomerUserId = g.Key, LastAt = g.Max(m => m.SentAt) })
                .ToListAsync(cancellationToken);

            var customerIds = summaries.Select(s => s.CustomerUserId).ToList();
            var customers = await _db.Users.AsNoTracking()
                .Where(u => customerIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, cancellationToken);

            var businessName = await _db.Businesses.AsNoTracking()
                .Where(b => b.Id == businessId)
                .Select(b => b.Name)
                .FirstAsync(cancellationToken);

            var result = new List<ChatThreadDto>();
            foreach (var s in summaries.OrderByDescending(x => x.LastAt))
            {
                customers.TryGetValue(s.CustomerUserId, out var c);
                var preview = await _db.ChatMessages.AsNoTracking()
                    .Where(m => m.BusinessId == businessId && m.CustomerUserId == s.CustomerUserId)
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => m.Body)
                    .FirstOrDefaultAsync(cancellationToken);
                result.Add(new ChatThreadDto(businessId, businessName, s.CustomerUserId, c?.Email ?? "?", s.LastAt, preview));
            }

            return Ok(result);
        }

        return Forbid();
    }

    [HttpGet("messages")]
    public async Task<ActionResult<IReadOnlyList<ChatMessageDto>>> Messages([FromQuery] Guid businessId, [FromQuery] Guid? customerUserId, CancellationToken cancellationToken)
    {
        if (User.IsInRole(nameof(UserRole.Admin)))
        {
            return Forbid();
        }

        Guid effectiveCustomerId;
        if (User.IsInRole(nameof(UserRole.Customer)))
        {
            effectiveCustomerId = GetUserId();
            if (customerUserId.HasValue && customerUserId.Value != effectiveCustomerId)
            {
                return Forbid();
            }
        }
        else if (User.IsInRole(nameof(UserRole.Business)))
        {
            if (!TryGetBusinessId(out var myBusinessId) || myBusinessId != businessId)
            {
                return Forbid();
            }

            if (!customerUserId.HasValue)
            {
                return BadRequest(new { message = "customerUserId gerekli." });
            }

            effectiveCustomerId = customerUserId.Value;
        }
        else
        {
            return Forbid();
        }

        var msgs = await _db.ChatMessages.AsNoTracking()
            .Where(m => m.BusinessId == businessId && m.CustomerUserId == effectiveCustomerId)
            .OrderBy(m => m.SentAt)
            .ToListAsync(cancellationToken);

        var replyIds = msgs.Where(m => m.ReplyToMessageId.HasValue).Select(m => m.ReplyToMessageId!.Value).Distinct().ToList();
        var replies = replyIds.Count == 0
            ? new List<ChatMessage>()
            : await _db.ChatMessages.AsNoTracking()
                .Where(m => replyIds.Contains(m.Id))
                .ToListAsync(cancellationToken);

        var senderIds = msgs.Select(m => m.SenderUserId)
            .Concat(replies.Select(m => m.SenderUserId))
            .Distinct()
            .ToList();
        var senders = await _db.Users.AsNoTracking()
            .Where(u => senderIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Email, cancellationToken);

        var replyMap = replies.ToDictionary(
            r => r.Id,
            r => (senders.GetValueOrDefault(r.SenderUserId, "?"), r.Body));

        var list = msgs.Select(m => ToDto(m, senders, replyMap)).ToList();
        return Ok(list);
    }

    [HttpPost("messages")]
    public async Task<ActionResult<ChatMessageDto>> Send([FromBody] SendChatMessageRequest request, CancellationToken cancellationToken)
    {
        var senderId = GetUserId();
        var body = request.Body.Trim();
        Guid customerId;
        Guid businessId = request.BusinessId;

        if (User.IsInRole(nameof(UserRole.Customer)))
        {
            customerId = senderId;
        }
        else if (User.IsInRole(nameof(UserRole.Business)))
        {
            if (!TryGetBusinessId(out var myBusinessId) || myBusinessId != businessId)
            {
                return Forbid();
            }

            if (!request.CustomerUserId.HasValue)
            {
                return BadRequest(new { message = "customerUserId gerekli." });
            }

            customerId = request.CustomerUserId.Value;
        }
        else
        {
            return Forbid();
        }

        var businessExists = await _db.Businesses.AnyAsync(b => b.Id == businessId && b.IsActive, cancellationToken);
        if (!businessExists)
        {
            return NotFound(new { message = "İşletme bulunamadı." });
        }

        var customerExists = await _db.Users.AnyAsync(u => u.Id == customerId && u.Role == UserRole.Customer && u.IsActive, cancellationToken);
        if (!customerExists)
        {
            return BadRequest(new { message = "Müşteri bulunamadı." });
        }

        if (request.ReplyToMessageId.HasValue)
        {
            var parentOk = await _db.ChatMessages.AsNoTracking().AnyAsync(
                m => m.Id == request.ReplyToMessageId && m.BusinessId == businessId && m.CustomerUserId == customerId,
                cancellationToken);
            if (!parentOk)
            {
                return BadRequest(new { message = "Yanıt verilen mesaj bu konuşmada değil." });
            }
        }

        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            CustomerUserId = customerId,
            SenderUserId = senderId,
            ReplyToMessageId = request.ReplyToMessageId,
            Body = body,
            SentAt = DateTimeOffset.UtcNow,
        };
        _db.ChatMessages.Add(message);
        await _db.SaveChangesAsync(cancellationToken);

        var senderEmail = await _db.Users.AsNoTracking().Where(u => u.Id == senderId).Select(u => u.Email).FirstAsync(cancellationToken);

        string? replyEmail = null;
        string? replyPreview = null;
        if (message.ReplyToMessageId.HasValue)
        {
            var parent = await _db.ChatMessages.AsNoTracking()
                .Where(m => m.Id == message.ReplyToMessageId)
                .Select(m => new { m.SenderUserId, m.Body })
                .FirstAsync(cancellationToken);
            replyEmail = await _db.Users.AsNoTracking().Where(u => u.Id == parent.SenderUserId).Select(u => u.Email).FirstAsync(cancellationToken);
            replyPreview = TruncatePreview(parent.Body);
        }

        var dto = new ChatMessageDto(
            message.Id,
            message.BusinessId,
            message.CustomerUserId,
            message.SenderUserId,
            senderEmail,
            message.Body,
            message.SentAt,
            message.ReplyToMessageId,
            replyEmail,
            replyPreview);

        await _live.ChatMessageAsync(businessId, customerId, dto, cancellationToken);
        return Ok(dto);
    }

    private static string? TruncatePreview(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        return text.Length <= ReplyPreviewMax ? text : text[..ReplyPreviewMax] + "…";
    }

    private static ChatMessageDto ToDto(
        ChatMessage m,
        IReadOnlyDictionary<Guid, string> senders,
        IReadOnlyDictionary<Guid, (string Email, string Body)> replyMap)
    {
        string? rEmail = null;
        string? rPreview = null;
        if (m.ReplyToMessageId.HasValue && replyMap.TryGetValue(m.ReplyToMessageId.Value, out var rb))
        {
            rEmail = rb.Email;
            rPreview = TruncatePreview(rb.Body);
        }

        return new ChatMessageDto(
            m.Id,
            m.BusinessId,
            m.CustomerUserId,
            m.SenderUserId,
            senders.GetValueOrDefault(m.SenderUserId, "?"),
            m.Body,
            m.SentAt,
            m.ReplyToMessageId,
            rEmail,
            rPreview);
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var id))
        {
            throw new InvalidOperationException();
        }

        return id;
    }

    private bool TryGetBusinessId(out Guid businessId)
    {
        var raw = User.FindFirst("businessId")?.Value;
        return Guid.TryParse(raw, out businessId);
    }
}
