using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

using uSync.BackOffice;
using uSync.Webhooks.Webhooks;

namespace uSync.Webhooks;

[ComposeAfter(typeof(uSyncBackOfficeComposer))]
public class uSyncWebhookComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AdduSyncWorkflows();
    }
}

internal static class uSyncWebHookBuilderExtensions 
{
    public static IUmbracoBuilder AdduSyncWorkflows(this IUmbracoBuilder builder)
    {

        // notifications 
        builder.AddNotificationAsyncHandler<uSyncImportedItemNotification, uSyncImportedItemWebhook>();
        builder.AddNotificationAsyncHandler<uSyncExportedItemNotification, uSyncExportedItemWebhook>();

        builder.AddNotificationAsyncHandler<uSyncImportCompletedNotification, uSyncImportCompletedWebhook>();
        builder.AddNotificationAsyncHandler<uSyncExportCompletedNotification, uSyncExportCompletedWebhook>();

        // webhooks
        builder.WebhookEvents().Append<uSyncImportedItemWebhook>();
        builder.WebhookEvents().Append<uSyncExportedItemWebhook>();
        builder.WebhookEvents().Append<uSyncImportCompletedWebhook>();
        builder.WebhookEvents().Append<uSyncExportCompletedWebhook>();


        return builder;
    }
}
