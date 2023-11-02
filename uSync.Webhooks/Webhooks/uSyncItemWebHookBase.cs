using Microsoft.Extensions.Options;

using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Webhooks;

using uSync.BackOffice;

namespace UmbTest.Webhooks;

public abstract class uSyncItemWebhookBase<TNotification, TObject> : IWebhookEvent, INotificationAsyncHandler<TNotification>
    where TNotification : uSyncItemNotification<TObject>
{
    public string EventName { get; set; }

    protected readonly IWebhookFiringService _webhookFiringService;
    protected readonly IWebHookService _webHookService;
    protected readonly IServerRoleAccessor _serverRoleAccessor;
    protected WebhookSettings _webhookSettings;

    public uSyncItemWebhookBase(
        string eventName,
        IServerRoleAccessor serverRoleAccessor,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IWebhookFiringService webhookFiringService,
        IWebHookService webHookService)

    {
        _serverRoleAccessor = serverRoleAccessor;
        _webhookSettings = webhookSettings.CurrentValue;

        EventName = eventName;
        _webhookFiringService = webhookFiringService;
        _webHookService = webHookService;
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


            await _webhookFiringService.FireAsync(webhook, EventName, notification.Item, cancellationToken);
        }
    }

    protected virtual bool FireWebhookForNotification(TNotification notification)
        => true;
}
