namespace ChisaApi.Application.Expenses.DataTransfers.Requests;

public sealed class ListExpensesQueryParameters
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public Guid? CategoryId { get; set; }
}
