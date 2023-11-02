namespace uSync.Webhooks.Configuration;

public class uSyncWebhookOptions
{
    /// <summary>
    ///  is the inbound webhook enabled.
    /// </summary>
    public bool Enabled { get; set; } = false; 

    /// <summary>
    ///  the api key that must be in the header of a request. 
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}
