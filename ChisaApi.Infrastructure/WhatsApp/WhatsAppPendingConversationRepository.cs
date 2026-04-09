using ChisaApi.Domain.WhatsApp;
using ChisaApi.Domain.WhatsApp.Entities;
using ChisaApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChisaApi.Infrastructure.WhatsApp;

public sealed class WhatsAppPendingConversationRepository : IWhatsAppPendingConversationRepository
{
    private readonly AppDbContext _context;

    public WhatsAppPendingConversationRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<WhatsAppPendingConversation?> GetLatestActiveByPhoneAsync(string phoneE164, CancellationToken cancellationToken = default) =>
        _context.WhatsAppPendingConversations.AsNoTracking()
            .Where(x => x.PhoneE164 == phoneE164 && x.ExpiresAt > DateTimeOffset.UtcNow)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<WhatsAppPendingConversation?> GetLatestActiveTrackedByPhoneAsync(string phoneE164, CancellationToken cancellationToken = default) =>
        _context.WhatsAppPendingConversations
            .Where(x => x.PhoneE164 == phoneE164 && x.ExpiresAt > DateTimeOffset.UtcNow)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AddAsync(WhatsAppPendingConversation entity, CancellationToken cancellationToken = default)
    {
        await _context.WhatsAppPendingConversations.AddAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        WhatsAppPendingConversation? row = await _context.WhatsAppPendingConversations
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);
        if (row is not null)
            _context.WhatsAppPendingConversations.Remove(row);
    }

    public Task DeleteAllForPhoneAsync(string phoneE164, CancellationToken cancellationToken = default) =>
        _context.WhatsAppPendingConversations
            .Where(x => x.PhoneE164 == phoneE164)
            .ExecuteDeleteAsync(cancellationToken);
}