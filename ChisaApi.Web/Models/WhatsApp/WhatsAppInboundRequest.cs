namespace ChisaApi.Web.Models.WhatsApp;

public sealed class WhatsAppInboundRequest
{
    public string? Source { get; set; }
    public string? Instance { get; set; }
    public string? Event { get; set; }
    public WhatsAppInboundMessage? Message { get; set; }
    public WhatsAppInboundContact? Contact { get; set; }
    public WhatsAppInboundMeta? Meta { get; set; }
}

public sealed class WhatsAppInboundMessage
{
    public string? Id { get; set; }
    public string? Text { get; set; }
    public string? Type { get; set; }
    public long? Timestamp { get; set; }
}

public sealed class WhatsAppInboundContact
{
    public string? Jid { get; set; }
    public string? WaId { get; set; }
    public string? PushName { get; set; }
}

public sealed class WhatsAppInboundMeta
{
    public bool? FromMe { get; set; }
}
