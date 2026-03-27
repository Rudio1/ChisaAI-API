using ChisaApi.Domain.Common;

namespace ChisaApi.Domain.Expenses.Entities;

public sealed class ExpenseCategory : IAuditable
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
