namespace Drashyam.API.Configuration;

public class AzureSignalRSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string HubPrefix { get; set; } = "drashyam";
    public bool UseAzureSignalR { get; set; } = false;
}
