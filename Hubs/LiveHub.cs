using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
namespace WebApplication1.Hubs;

[Authorize]
public sealed class LiveHub : Hub
{
    public const string AdminGroup = "role-admin";

    /// <summary>Tüm müşteri bağlantıları — katalog stok güncellemeleri.</summary>
    public const string CustomerCatalogGroup = "role-customer-catalog";

    public static string BusinessGroup(Guid businessId) => $"business-{businessId}";
    public static string CustomerGroup(Guid userId) => $"customer-{userId}";
    public static string ConversationGroup(Guid businessId, Guid customerUserId) =>
        $"conv-{businessId:N}-{customerUserId:N}";

    public async Task JoinAdminPanel()
    {
        if (!IsAdmin())
        {
            throw new HubException("Yalnızca admin bu kanala katılabilir.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, AdminGroup);
    }

    public async Task JoinBusinessPanel()
    {
        var businessId = GetBusinessId();
        if (!businessId.HasValue)
        {
            throw new HubException("İşletme kullanıcısı değilsiniz.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, BusinessGroup(businessId.Value));
    }

    public async Task JoinCustomerPanel()
    {
        var userId = GetUserId();
        await Groups.AddToGroupAsync(Context.ConnectionId, CustomerGroup(userId));
        await Groups.AddToGroupAsync(Context.ConnectionId, CustomerCatalogGroup);
    }

    /// <summary>İşletme–müşteri sohbet odası (her iki taraf + admin genel dinlemede).</summary>
    public async Task JoinConversation(Guid businessId, Guid customerUserId)
    {
        var userId = GetUserId();
        if (IsAdmin())
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, ConversationGroup(businessId, customerUserId));
            return;
        }

        if (IsBusiness() && GetBusinessId() == businessId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, ConversationGroup(businessId, customerUserId));
            return;
        }

        if (IsCustomer() && userId == customerUserId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, ConversationGroup(businessId, customerUserId));
            return;
        }

        throw new HubException("Bu sohbete erişim izniniz yok.");
    }

    /// <summary>Karşı tarafa yazıyor durumu (müşteri ↔ işletme).</summary>
    public async Task SetChatTyping(Guid businessId, Guid customerUserId, bool isTyping)
    {
        var userId = GetUserId();
        if (IsAdmin())
        {
            return;
        }

        if (IsBusiness() && GetBusinessId() == businessId)
        {
            await Clients.Group(CustomerGroup(customerUserId)).SendAsync("chatTyping", new
            {
                businessId,
                customerUserId,
                userId,
                isTyping,
            });
            return;
        }

        if (IsCustomer() && userId == customerUserId)
        {
            await Clients.Group(BusinessGroup(businessId)).SendAsync("chatTyping", new
            {
                businessId,
                customerUserId,
                userId,
                isTyping,
            });
            return;
        }

        throw new HubException("Bu sohbet için yazıyor bildirimi gönderemezsiniz.");
    }

    private Guid GetUserId()
    {
        var sub = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? Context.User?.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var id))
        {
            throw new HubException("Kullanıcı tanımlanamadı.");
        }

        return id;
    }

    private Guid? GetBusinessId()
    {
        var raw = Context.User?.FindFirst("businessId")?.Value;
        return Guid.TryParse(raw, out var id) ? id : null;
    }

    private bool IsAdmin() => Context.User?.IsInRole("Admin") == true;
    private bool IsBusiness() => Context.User?.IsInRole("Business") == true;
    private bool IsCustomer() => Context.User?.IsInRole("Customer") == true;
}
