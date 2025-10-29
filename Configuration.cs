namespace TestOllamaAgent.Configuration;

public class AppConfiguration
{
    public ClientConfiguration ClientConfiguration { get; set; } = new();
    public AgentConfiguration Agent { get; set; } = new();
}

public class ClientConfiguration
{
    public bool UseAzureAI { get; set; }
    public OllamaConfiguration Ollama { get; set; } = new();
    public AzureAIConfiguration AzureAI { get; set; } = new();
}

public class OllamaConfiguration
{
    public string Endpoint { get; set; } = "http://localhost:11434";
    public string ModelName { get; set; } = "llama3.2:3b";
}

public class AzureAIConfiguration
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
}

public class AgentConfiguration
{
    public string Name { get; set; } = "WeatherMan";
    public string Instructions { get; set; } = "You are a professional weather forecaster. Only provide weather information or forecasts if the user requests it. Otherwise, politely indicate that you can answer weather-related questions.";
}