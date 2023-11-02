using uSync.Webhooks.Models;

namespace uSync.Webhooks.WebhookEvents;
public class uSyncWebhookAttribute : Attribute
{
    public uSyncWebhookAttribute(string eventName, uSyncWebhookEvent eventType)
    {
        EventName = eventName;
        EventType = eventType;  
    }

    public string EventName { get; set; }   

    public uSyncWebhookEvent EventType { get; set; }
}
