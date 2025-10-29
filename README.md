# 🌤️ Test Ollama Agent

A demonstration project showing how to create an intelligent AI agent that can use both **Ollama** (local) and **Azure AI** (cloud) to answer weather-related queries. This example showcases AI agents, custom middleware, and multi-provider AI integration.

## 🎯 What You'll Learn

- **AI Agent Architecture**: Structure a conversational agent
- **Provider Abstraction**: Use a common interface for multiple AI services
- **Function Calling**: Implement tools the agent can use
- **Middleware Pattern**: Add cross-cutting functionality like logging
- **Flexible Configuration**: Manage settings for different environments

## 🏗️ Project Structure

```
TestOllamaAgent/
├── Configuration.cs          # Typed configuration classes
├── Program.cs                # Main entry point and logic
├── appsettings.example.json  # Configuration template
├── appsettings.json          # Current config (don't commit)
└── TestOllamaAgent.csproj    # Project dependencies
```

## ⚙️ Setup

### 1. Configure the AI Provider

Copy the example configuration:
```powershell
Copy-Item appsettings.example.json appsettings.json
```

Edit `appsettings.json` for your preferred AI provider:

**Option A: Local Ollama** 🟢
```json
{
  "ClientConfiguration": {
    "UseAzureAI": false,
    "Ollama": {
      "Endpoint": "http://localhost:11434",
      "ModelName": "llama3.1:8b"
    }
  }
}
```

Prerequisites for Ollama:
```powershell
# Install Ollama
winget install --id=Ollama.Ollama -e

# Pull the model
ollama pull llama3.1:8b
```

**Option B: Azure AI** ☁️
```json
{
  "ClientConfiguration": {
    "UseAzureAI": true,
    "AzureAI": {
      "BaseUrl": "https://models.github.ai/inference",
      "ApiKey": "your-actual-api-key",
      "ModelName": "mistral-small-2503"
    }
  }
}
```

### 2. Customize the Agent

```json
{
  "Agent": {
    "Name": "WeatherMan",
    "Instructions": "You are a professional weather forecaster. Only provide weather information or forecasts if the user requests it."
  }
}
```

## 🚀 Running the Application

```powershell
# Restore dependencies
dotnet restore

# Run the project
dotnet run
```

### Example Interaction

```
🌤️  INTERACTIVE WEATHER AGENT CHAT
============================================================

👤 You: How's the weather in Donosti?

🧠 WeatherMan is thinking...

🔧 Function Name: GetWeather - Parameters: location -> Donosti
Response from function: Rainy with a high of 10°C.

🌤️  WeatherMan: According to the information I have, the weather in Donosti 
(San Sebastián) is rainy with a high temperature of 10°C. I recommend 
bringing an umbrella and dressing warmly!

✨ Response complete!
```

## 🔍 Key Components

### Configuration Management

The project uses typed configuration classes for type safety and IntelliSense support:

```csharp
public class AppConfiguration
{
    public ClientConfiguration ClientConfiguration { get; set; } = new();
    public AgentConfiguration Agent { get; set; } = new();
}
```

### AI Provider Abstraction

The application can switch between AI providers without code changes:

```csharp
IChatClient chatClient;

if (appConfig.ClientConfiguration.UseAzureAI)
{
    var azureClient = new ChatCompletionsClient(/*...*/);
    chatClient = azureClient.AsIChatClient(modelName);
}
else
{
    chatClient = new OllamaApiClient(/*...*/);
}
```

### Function Calling

The agent has access to tools it can use to answer questions:

```csharp
[Description("Get the weather for a given location.")]
static string GetWeather([Description("The location to get the weather for.")] string location)
    => location == "Donosti" || location == "San Sebastián"
    ? "Rainy with a high of 10°C."
    : "Sunny with a high of 25°C.";
```

### Middleware Pattern

Middleware provides cross-cutting functionality like logging:

```csharp
async ValueTask<object?> FunctionCallMiddleware(/* parameters */)
{
    // Pre-processing: log function call
    Console.WriteLine($"🔧 Function Name: {context!.Function.Name}");
    
    var stopwatch = Stopwatch.StartNew();
    var result = await next(context, cancellationToken);
    stopwatch.Stop();
    
    // Post-processing: log result and duration
    Console.WriteLine($"Response: {result}");
    Console.WriteLine($"Duration: {stopwatch.ElapsedMilliseconds}ms");
    
    return result;
}
```

## 🛠️ Extending the Agent

### Adding New Functions

```csharp
[Description("Get the current time in a specific timezone.")]
static string GetCurrentTime([Description("The timezone to get time for.")] string timezone)
{
    return DateTime.Now.ToString("HH:mm:ss");
}

// Register in the agent
AIAgent agent = new ChatClientAgent(
    chatClient,
    instructions,
    name,
    tools: [
        AIFunctionFactory.Create(GetWeather),
        AIFunctionFactory.Create(GetCurrentTime)  // New function
    ]
);
```

### Adding New AI Providers

1. Add configuration class in `Configuration.cs`
2. Implement provider initialization in `Program.cs`
3. Update the configuration validation logic

## 🔒 Security Best Practices

### Never commit sensitive data

⚠️ **IMPORTANT**: Never commit `appsettings.json` with real API keys

For development, use User Secrets:
```powershell
dotnet user-secrets init
dotnet user-secrets set "ClientConfiguration:AzureAI:ApiKey" "your-real-key"
```

For production, use environment variables or Azure Key Vault.

## 📚 Technologies Used

- **.NET 9.0**: Modern C# features and performance
- **Microsoft.Extensions.AI**: AI provider abstraction
- **Microsoft.AI.Agents**: Agent framework
- **Ollama**: Local AI models
- **Azure AI**: Cloud AI services

## 🎯 Next Steps

1. **Experiment** with different models and configurations
2. **Add** new functions (real weather API, calculator, etc.)
3. **Implement** conversation persistence
4. **Explore** other AI providers (OpenAI, Anthropic)
5. **Build** a web interface

## 📖 Resources

- [Microsoft Agent Framework Documentation](https://learn.microsoft.com/es-es/agent-framework/overview/agent-framework-overview)
- [Microsoft.Extensions.AI Documentation](https://learn.microsoft.com/es-es/dotnet/ai/microsoft-extensions-ai)
- [Ollama Documentation](https://docs.ollama.com/)
- [Azure AI Services](https://learn.microsoft.com/en-us/azure/ai-services/)

---

*Questions or want to contribute? Open an issue or create a pull request!* 🚀