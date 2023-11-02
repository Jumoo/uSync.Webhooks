using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using uSync.Webhooks.Configuration;

namespace uSync.Webhooks.Authorization;

public class uSyncWebhookKeyAuthorizationAttribute : ServiceFilterAttribute
{
    public uSyncWebhookKeyAuthorizationAttribute()
        : base(typeof(uSyncWebhookKeyAuthFilter))
    { }
}

/// <summary>
///  API key in the header authorization filter
/// </summary>
/// <remarks>
///  this is not how we usually secure uSync end points, as these 
///  requests can be replayed or intercepted and altered 
///  
///  we preferer to sign based on content, keys and secrets, but
///  this is what we have with webhooks just now, so API key 
///  is fine. 
/// </remarks>

public class uSyncWebhookKeyAuthFilter : IAuthorizationFilter
{
    private readonly ILogger<uSyncWebhookKeyAuthFilter> _logger;
    private uSyncWebhookOptions _webhookOptions;

    public uSyncWebhookKeyAuthFilter(
        IOptionsMonitor<uSyncWebhookOptions> webhookOptions,
        ILogger<uSyncWebhookKeyAuthFilter> logger)
    {
        _webhookOptions = webhookOptions.CurrentValue;
        webhookOptions.OnChange(w => _webhookOptions = w);
        _logger = logger;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (_webhookOptions.Enabled is false || 
            string.IsNullOrWhiteSpace(_webhookOptions.ApiKey))
        {
            _logger.LogTrace("Missing configuration");
            context.Result = new BadRequestResult();
            return;
        }

        var key = context.HttpContext.Request.Headers[uSyncWebhooks.ApiKeyHeader].ToString();
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogTrace("Missing key in header");
            context.Result = new BadRequestResult();
            return;
        }

        if (key.Equals(_webhookOptions.ApiKey) is false)
        {
            _logger.LogTrace("Key mismatch");
            context.Result = new BadRequestResult();
            return;
        }
    }
}
