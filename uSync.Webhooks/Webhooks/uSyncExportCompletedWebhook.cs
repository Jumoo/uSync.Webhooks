using Microsoft.Extensions.Options;

using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Webhooks;
using Umbraco.Cms.Infrastructure.HostedServices;

using uSync.BackOffice;

namespace uSync.Webhooks.Webhooks;
public class uSyncExportCompletedWebhook : IWebhookEvent,
    INotificationAsyncHandler<uSyncExportCompletedNotification>
{
    private readonly IWebhookFiringService _webhookFiringService;
    private readonly IWebHookService _webhookService;
    private readonly IServerRoleAccessor _serverRoleAccessor;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;

    private WebhookSettings _webhookSettings;

    public string EventName { get; set; }

    public uSyncExportCompletedWebhook(
        IWebhookFiringService webhookFiringService,
        IWebHookService webhookService,
        IServerRoleAccessor serverRoleAccessor,
        IOptionsMonitor<WebhookSettings> optionsMonitor,
        IBackgroundTaskQueue backgroundTaskQueue)
    {
        EventName = "uSync Export Completed";
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

    public async Task HandleAsync(uSyncExportCompletedNotification notification, CancellationToken cancellationToken)
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

        if (notification.Actions.Any() is false)
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
                await _webhookFiringService.FireAsync(webhook, EventName, notification.Actions, cancellationToken);
            });
        }
    }
}
