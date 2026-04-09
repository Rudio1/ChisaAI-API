namespace ChisaApi.Domain.WhatsApp.Entities;

public sealed class WhatsAppPendingConversation
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string PhoneE164 { get; set; } = "";
    public WhatsAppPendingStep Step { get; set; }
    public decimal Amount { get; set; }
    public string CategoryNameUnderReview { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}
