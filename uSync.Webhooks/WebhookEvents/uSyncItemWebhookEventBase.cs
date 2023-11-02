using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Webhooks;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Extensions;

using uSync.BackOffice;
using uSync.Webhooks.Models;

namespace uSync.Webhooks.WebhookEvents;

public abstract class uSyncItemWebhookEventBase<TNotification, TObject> : IWebhookEvent, INotificationAsyncHandler<TNotification>
    where TNotification : uSyncItemNotification<TObject>
{
    public string EventName { get; set; }

    protected uSyncWebhookEvent EventType { get; set; }

    protected readonly IWebhookFiringService _webhookFiringService;
    protected readonly IWebHookService _webHookService;
    protected readonly IServerRoleAccessor _serverRoleAccessor;
    protected WebhookSettings _webhookSettings;

    protected readonly IBackgroundTaskQueue _backgroundTaskQueue;

    public uSyncItemWebhookEventBase(
        IServerRoleAccessor serverRoleAccessor,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IWebhookFiringService webhookFiringService,
        IWebHookService webHookService,
        IBackgroundTaskQueue backgroundTaskQueue)

    {
        _serverRoleAccessor = serverRoleAccessor;
        _webhookSettings = webhookSettings.CurrentValue;

        _webhookFiringService = webhookFiringService;
        _webHookService = webHookService;
        _backgroundTaskQueue = backgroundTaskQueue;

        var meta = GetType().GetCustomAttribute<uSyncWebhookAttribute>(false);
        if (meta == null)
            throw new InvalidOperationException("uSyncWebhook Attribute missing");

        EventName = meta.EventName;
        EventType = meta.EventType;
    }

    public async Task HandleAsync(TNotification notification, CancellationToken cancellationToken)
    {
        if (_webhookSettings.Enabled is false)
        {
            return;
        }

        if (_serverRoleAccessor.CurrentServerRole is not ServerRole.Single
            && _serverRoleAccessor.CurrentServerRole is not ServerRole.SchedulingPublisher)
        {
            return;
        }

        if (FireWebhookForNotification(notification) is false)
        {
            return;
        }

        // fire the xml across the wire to something else. 
        var webhooks = await _webHookService.GetByEventNameAsync(EventName);

        foreach (var webhook in webhooks)
        {
            if (webhook.Enabled is false) continue;

            _backgroundTaskQueue.QueueBackgroundWorkItem(
                async cancellationToken => 
                {
                    var data = new uSyncWebhookData
                    {
                        EventName = EventName,
                        Data = notification.Item
                    };

                    await _webhookFiringService.FireAsync(webhook, EventName, data, cancellationToken);              
                });
        }
    }

    protected virtual bool FireWebhookForNotification(TNotification notification)
        => true;
}
