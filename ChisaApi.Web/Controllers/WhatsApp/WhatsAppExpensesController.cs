using ChisaApi.Application.Integrations.Settings;
using ChisaApi.Application.Integrations.WhatsApp;
using ChisaApi.Web.Models.WhatsApp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ChisaApi.Web.Controllers.WhatsApp;

[AllowAnonymous]
[ApiController]
[Route("api/whatsapp")]
public sealed class WhatsAppExpensesController : ControllerBase
{
    private readonly WhatsAppExpenseAppService _whatsappExpenses;
    private readonly WhatsAppIntegrationOptions _integration;

    public WhatsAppExpensesController(
        WhatsAppExpenseAppService whatsappExpenses,
        IOptions<WhatsAppIntegrationOptions> integration)
    {
        _whatsappExpenses = whatsappExpenses;
        _integration = integration.Value;
    }

    /// <summary>
    /// Ponto único para o n8n: cada mensagem do WhatsApp. Responde JSON com texto para reenviar ao usuário.
    /// Formato de gasto: "150 mercado" (valor + nome da categoria).
    /// </summary>
    [HttpPost("expenses")]
    [ProducesResponseType(typeof(WhatsAppInboundFlowResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> HandleInboundMessage(
        [FromBody] WhatsAppInboundRequest body,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_integration.WebhookSecret))
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new { message = "Integração WhatsApp não configurada (WhatsAppIntegration:WebhookSecret)." });
        }

        string? headerSecret = Request.Headers["X-Webhook-Secret"].FirstOrDefault();
        if (!string.Equals(headerSecret, _integration.WebhookSecret, StringComparison.Ordinal))
            return Unauthorized(new { message = "Segredo do webhook inválido." });

        string? text = body.Message?.Text;
        string? waId = body.Contact?.WaId;
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(waId))
            return BadRequest(new { message = "Campos message.text e contact.waId são obrigatórios." });

        WhatsAppInboundFlowResponse flow = await _whatsappExpenses
            .ProcessMessageAsync(waId, text, cancellationToken)
            .ConfigureAwait(false);

        return Ok(flow);
    }
}
