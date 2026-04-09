using ChisaApi.Application.Expenses.DataTransfers.Responses;

namespace ChisaApi.Application.Integrations.WhatsApp;

public sealed class WhatsAppInboundFlowResponse
{
    public required string Outcome { get; init; }
    public string? WhatsAppReplyText { get; init; }
    public ExpenseDto? Expense { get; init; }
    public string? ErrorCode { get; init; }
}
