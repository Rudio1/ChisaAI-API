using ChisaApi.Domain.WhatsApp.Entities;

namespace ChisaApi.Domain.WhatsApp;

public interface IWhatsAppPendingConversationRepository
{
    Task<WhatsAppPendingConversation?> GetLatestActiveByPhoneAsync(string phoneE164, CancellationToken cancellationToken = default);
    Task<WhatsAppPendingConversation?> GetLatestActiveTrackedByPhoneAsync(string phoneE164, CancellationToken cancellationToken = default);
    Task AddAsync(WhatsAppPendingConversation entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAllForPhoneAsync(string phoneE164, CancellationToken cancellationToken = default);
}
