using ChisaApi.Domain.Common;

namespace ChisaApi.Domain.Expenses.Entities;

public sealed class Expense : IAuditable
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string Category { get; set; } = "";
    public string? Note { get; set; }
    public DateTimeOffset SpentAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
