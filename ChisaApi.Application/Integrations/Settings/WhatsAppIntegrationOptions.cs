namespace ChisaApi.Application.Integrations.Settings;

public sealed class WhatsAppIntegrationOptions
{
    public const string SectionName = "WhatsAppIntegration";

    public string WebhookSecret { get; set; } = "";
}
