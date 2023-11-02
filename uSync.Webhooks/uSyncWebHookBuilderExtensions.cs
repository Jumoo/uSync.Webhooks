using Microsoft.Extensions.DependencyInjection;

using Org.BouncyCastle.Asn1.Cms.Ecc;

using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

using uSync.BackOffice;
using uSync.Webhooks.Authorization;
using uSync.Webhooks.Configuration;
using uSync.Webhooks.WebhookEvents;

namespace uSync.Webhooks;

[ComposeAfter(typeof(uSyncBackOfficeComposer))]
public class uSyncWebhookComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AdduSyncWorkflows();
        builder.AdduSyncWebhooks();
    }
}

internal static class uSyncWebHookBuilderExtensions 
{
    public static IUmbracoBuilder AdduSyncWorkflows(this IUmbracoBuilder builder)
    {

        // notifications 
        builder.AddNotificationAsyncHandler<uSyncImportedItemNotification, uSyncImportedItemWebhookEvent>();
        builder.AddNotificationAsyncHandler<uSyncExportedItemNotification, uSyncExportedItemWebhookEvent>();

        builder.AddNotificationAsyncHandler<uSyncImportCompletedNotification, uSyncImportCompletedWebhookEvent>();
        builder.AddNotificationAsyncHandler<uSyncExportCompletedNotification, uSyncExportCompletedWebhookEvent>();

        // webhooks
        builder.WebhookEvents().Append<uSyncImportedItemWebhookEvent>();
        builder.WebhookEvents().Append<uSyncExportedItemWebhookEvent>();
        builder.WebhookEvents().Append<uSyncImportCompletedWebhookEvent>();
        builder.WebhookEvents().Append<uSyncExportCompletedWebhookEvent>();


        return builder;
    }

    public static IUmbracoBuilder AdduSyncWebhooks(this IUmbracoBuilder builder)
    {
        builder.Services.AddOptions<uSyncWebhookOptions>()
            .Bind(builder.Config.GetSection(uSyncWebhooks.OptionsKey));

        builder.Services.AddScoped<uSyncWebhookKeyAuthFilter>();

        return builder;
    }
}
