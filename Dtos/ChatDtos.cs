using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Dtos;

public sealed record SendChatMessageRequest(
    [Required] Guid BusinessId,
    Guid? CustomerUserId,
    [Required, MaxLength(4000)] string Body,
    Guid? ReplyToMessageId);

public sealed record ChatMessageDto(
    Guid Id,
    Guid BusinessId,
    Guid CustomerUserId,
    Guid SenderUserId,
    string SenderEmail,
    string Body,
    DateTimeOffset SentAt,
    Guid? ReplyToMessageId,
    string? ReplyToSenderEmail,
    string? ReplyToBodyPreview);

public sealed record ChatThreadDto(
    Guid BusinessId,
    string BusinessName,
    Guid CustomerUserId,
    string CustomerEmail,
    DateTimeOffset LastMessageAt,
    string? Preview);
