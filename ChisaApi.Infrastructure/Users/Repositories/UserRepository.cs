using Microsoft.EntityFrameworkCore;
using ChisaApi.Infrastructure.Data;
using ChisaApi.Domain.Users;
using ChisaApi.Domain.Users.Entities;

namespace ChisaApi.Infrastructure.Users.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<User?> GetByPhoneAsync(string phoneNumberE164, CancellationToken cancellationToken = default) =>
        _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.PhoneNumberE164 == phoneNumberE164, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken).ConfigureAwait(false);
    }
}
