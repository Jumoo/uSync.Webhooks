using System.Xml.Linq;

using Microsoft.Extensions.Options;

using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.HostedServices;

using uSync.BackOffice;
using uSync.Webhooks.Models;

namespace uSync.Webhooks.WebhookEvents;

/// <summary>
///  When any item is exported
/// </summary>
[uSyncWebhook("uSync Item Imported", uSyncWebhookEvent.Import)]
public class uSyncImportedItemWebhookEvent : uSyncItemWebhookEventBase<uSyncImportedItemNotification, XElement>
{
    public uSyncImportedItemWebhookEvent(
        IServerRoleAccessor serverRoleAccessor,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IWebhookFiringService webhookFiringService,
        IWebHookService webHookService,
        IBackgroundTaskQueue backgroundTaskQueue)
        : base(serverRoleAccessor, webhookSettings, webhookFiringService, webHookService, backgroundTaskQueue)
    { }

    protected override bool FireWebhookForNotification(uSyncImportedItemNotification notification)
        => notification.Change > uSync.Core.ChangeType.NoChange;
}
