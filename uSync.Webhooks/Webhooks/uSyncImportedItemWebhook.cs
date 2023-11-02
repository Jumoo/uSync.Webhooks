using System.Xml.Linq;

using Microsoft.Extensions.Options;

using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

using UmbTest.Webhooks;

using uSync.BackOffice;

namespace uSync.Webhooks.Webhooks;

/// <summary>
///  When any item is exported
/// </summary>
public class uSyncImportedItemWebhook : uSyncItemWebhookBase<uSyncImportedItemNotification, XElement>
{
    public uSyncImportedItemWebhook(
        IServerRoleAccessor serverRoleAccessor,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IWebhookFiringService webhookFiringService,
        IWebHookService webHookService)
        : base("uSync Item Imported", serverRoleAccessor, webhookSettings, webhookFiringService, webHookService)
    { }

    protected override bool FireWebhookForNotification(uSyncImportedItemNotification notification)
        => notification.Change > uSync.Core.ChangeType.NoChange;
}
