using System.Xml.Linq;

using Microsoft.Extensions.Options;

using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.HostedServices;

using UmbTest.Webhooks;

using uSync.BackOffice;

namespace uSync.Webhooks.Webhooks;
/// <summary>
///  Wen any item is exported
/// </summary>
public class uSyncExportedItemWebhook : uSyncItemWebhookBase<uSyncExportedItemNotification, XElement>
{
    public uSyncExportedItemWebhook(
        IServerRoleAccessor serverRoleAccessor,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IWebhookFiringService webhookFiringService,
        IWebHookService webHookService,
        IBackgroundTaskQueue backgroundTaskQueue)
        : base("uSync Item Exported", serverRoleAccessor, webhookSettings, webhookFiringService, webHookService, backgroundTaskQueue)
    { }
}
