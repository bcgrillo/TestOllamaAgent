namespace TestOllamaAgent.Configuration;

public class AppConfiguration
{
    public ClientConfiguration ClientConfiguration { get; set; } = new();
}

public class ClientConfiguration
{
    public OllamaConfiguration? Ollama { get; set; }
    public AzureAIConfiguration? AzureAI { get; set; }
}

public class OllamaConfiguration
{
    public string Endpoint { get; set; }
    public string ModelName { get; set; }
}

public class AzureAIConfiguration
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public List<string> AvailableModels { get; set; } = new();
}