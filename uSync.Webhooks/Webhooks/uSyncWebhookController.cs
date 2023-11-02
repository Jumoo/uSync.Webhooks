using System.Xml.Linq;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Controllers;

using uSync.BackOffice;
using uSync.BackOffice.SyncHandlers;
using uSync.Webhooks.Authorization;
using uSync.Webhooks.Models;

namespace uSync.Webhooks.Webhooks;

[PluginController("uSync")]
[uSyncWebhookKeyAuthorization]
public class uSyncWebhookController : UmbracoApiController
{
    private ILogger<uSyncWebhookController> _logger;
    private SyncHandlerFactory _syncHandlerFactory;

    public uSyncWebhookController(
        ILogger<uSyncWebhookController> logger,
        SyncHandlerFactory syncHandlerFactory)
    {
        _logger = logger;
        _syncHandlerFactory = syncHandlerFactory;
    }

    [HttpGet]
    public bool GetApi() => true;

    [HttpPost]
    public bool Post(uSyncWebhookData data)
    {
        _logger.LogInformation("Processing Webhook Data: {eventName}", data.EventName);

        // todo : there is lots to tidy up and consider here.
        //          we might not always want to import because exports might
        //          be coming from other things like events, or publishes
        //          or something. 
        //  
        //          so likely we need to be more decerning and or, listen for
        //          other events. 
        //
        //          this at the moment is proof of concept. 

        // choices here. we can de-stream the item, and import it ! 
        switch (data.EventType)
        {
            case uSyncWebhookEvent.Export:
                // when an item is exported to disk, 
                // we can choose to import it here
                // which means we will keep the two sites
                // in sync.
                ImportItem(data.Data);
                break;
            case uSyncWebhookEvent.Import:
                break;
            case uSyncWebhookEvent.BulkImport:
                break;
            case uSyncWebhookEvent.BulkExport:
                break;
        }

        return true;
    }

    private void ImportItem(object? data)
    {
        if (data is null) return;

        if (data is JToken token)
        {
            var node = token.ToObject<XElement>();
            if (node is null) return;

            var handler = _syncHandlerFactory.GetValidHandlerByTypeName(node.Name.LocalName);
            if (handler is null) return;

            var result = handler.Handler.ImportElement(node, "Webhook.Import", handler.Settings, new uSyncImportOptions
            { });

            _logger.LogInformation("Imported : {result}", result.FirstOrDefault().Success);
        }
    }
}
