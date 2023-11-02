using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Webhooks;
using Umbraco.Cms.Infrastructure.HostedServices;

using uSync.BackOffice;
using uSync.Core;
using uSync.Webhooks.Models;

namespace uSync.Webhooks.WebhookEvents; 

public class uSyncImportCompletedWebhookEvent : IWebhookEvent,
    INotificationAsyncHandler<uSyncImportCompletedNotification>
{
    private readonly IWebhookFiringService _webhookFiringService;
    private readonly IWebHookService _webhookService;
    private readonly IServerRoleAccessor _serverRoleAccessor;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;

    private WebhookSettings _webhookSettings;

    public string EventName { get; set; }

    public uSyncImportCompletedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebHookService webhookService,
        IServerRoleAccessor serverRoleAccessor,
        IOptionsMonitor<WebhookSettings> optionsMonitor,
        IBackgroundTaskQueue backgroundTaskQueue)
    {
        EventName = "uSync Import Completed";
        _webhookFiringService = webhookFiringService;
        _webhookService = webhookService;
        _serverRoleAccessor = serverRoleAccessor;

        _webhookSettings = optionsMonitor.CurrentValue;
        optionsMonitor.OnChange(settings =>
        {
            _webhookSettings = settings;
        });
        _backgroundTaskQueue = backgroundTaskQueue;
    }

    public async Task HandleAsync(uSyncImportCompletedNotification notification, CancellationToken cancellationToken)
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

        var changeActions = notification.Actions
            .Where(x => x.Change > ChangeType.NoChange)
            .ToList();

        if (changeActions.Count == 0)
        {
            return;
        }

        var webhooks = await _webhookService.GetByEventNameAsync(EventName);

        foreach (var webhook in webhooks)
        {
            if (webhook.Enabled is false) continue;

            _backgroundTaskQueue.QueueBackgroundWorkItem(
                async cancellationToken =>
                {
                    var data = new uSyncWebhookData
                    {
                        EventName = EventName,
                        Data = changeActions
                    };

                    await _webhookFiringService.FireAsync(webhook, EventName, data, cancellationToken);
                });
        }
    }
}
