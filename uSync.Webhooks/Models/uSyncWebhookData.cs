using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace uSync.Webhooks.Models;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]

public class uSyncWebhookData
{
    public string EventName { get; set; } = string.Empty;

    public uSyncWebhookEvent EventType { get; set; }

    public object? Data { get; set; }
}

