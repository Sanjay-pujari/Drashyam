namespace Drashyam.API.Configuration;

public class AzureStorageSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string VideoContainerName { get; set; } = "videos";
    public string ImageContainerName { get; set; } = "images";
}

