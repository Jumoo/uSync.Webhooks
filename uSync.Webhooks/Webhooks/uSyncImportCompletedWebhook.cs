using Microsoft.Extensions.Options;

using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Webhooks;

using uSync.BackOffice;
using uSync.Core;

namespace uSync.Webhooks.Webhooks;
public class uSyncImportCompletedWebhook : IWebhookEvent,
    INotificationAsyncHandler<uSyncImportCompletedNotification>
{
    private readonly IWebhookFiringService _webhookFiringService;
    private readonly IWebHookService _webhookService;
    private readonly IServerRoleAccessor _serverRoleAccessor;

    private WebhookSettings _webhookSettings;

    public string EventName { get; set; }

    public uSyncImportCompletedWebhook(
        IWebhookFiringService webhookFiringService,
        IWebHookService webhookService,
        IServerRoleAccessor serverRoleAccessor,
        IOptionsMonitor<WebhookSettings> optionsMonitor)
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

            await _webhookFiringService.FireAsync(webhook, EventName, changeActions, cancellationToken);
        }
    }
}
