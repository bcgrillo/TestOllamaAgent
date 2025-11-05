# 🤖 Test Ollama Agent

A comprehensive demonstration project showing how to create an intelligent multi-agent AI system that can use both **Ollama** (local) and **Azure AI** (cloud) services. This example showcases a modular AI agent architecture with specialized agents, interactive selection, and web search capabilities.

## 🎯 What You'll Learn

- **Multi-Agent Architecture**: Multiple specialized agents with unique capabilities
- **Interactive Agent Selection**: Runtime selection of AI providers and agents
- **Web Search Integration**: DuckDuckGo search functionality for real-time information
- **Provider Abstraction**: Use a common interface for multiple AI services
- **Function Calling**: Implement reusable tools that agents can use
- **Middleware Pattern**: Add cross-cutting functionality like logging
- **Flexible Configuration**: Manage settings for different environments
- **Extensible Design**: Easy to add new agents, tools, and middleware

## 🏗️ Project Structure

```
TestOllamaAgent/
├── agents/                     # Agent definitions and management
│   ├── BaseAgent.cs           # Abstract base class for all agents
│   ├── AgentManager.cs        # Agent registry and interactive selection
│   ├── WeatherAgent.cs        # Weather-specific agent
│   ├── ChatAgent.cs           # General conversation agent
│   └── SearchAgent.cs         # Web search specialized agent
├── tools/                      # Reusable function tools
│   ├── WeatherTools.cs        # Weather-related functions
│   ├── TimeTools.cs           # Time-related functions
│   └── SearchTools.cs         # Web search with DuckDuckGo
├── middlewares/                # Cross-cutting concerns
│   └── FunctionCallMiddleware.cs # Logging middleware
├── Configuration.cs           # Typed configuration classes
├── Program.cs                 # Main entry point and interactive logic
├── appsettings.example.json   # Configuration template
├── appsettings.json          # Current config (don't commit)
└── TestOllamaAgent.csproj     # Project dependencies
```

## ✨ Key Features

### 🤖 Multi-Agent System
- **Specialized Agents**: Weather, Chat, and Web Search agents with unique capabilities
- **Interactive Selection**: Runtime selection of both AI provider and agent type
- **BaseAgent Architecture**: Abstract class that defines common structure for all agents
- **AgentManager**: Centralized registry and menu system for agent selection

### 🔍 Web Search Integration
- **DuckDuckGo Search**: Real-time web search capabilities via SearchAgent
- **SearchTools**: Comprehensive web search with result parsing and formatting
- **Information Retrieval**: Find current information, news, and specific data

### 🔧 Reusable Tools
- **Organized by Function**: Tools are grouped in logical namespaces
- **Discoverable**: AI automatically discovers and uses appropriate tools
- **Testable**: Tools are isolated and easy to test independently

### 🔄 Flexible Middleware
- **Logging**: Track function calls and performance
- **Extensible**: Easy to add new middleware for authentication, validation, etc.
- **Composable**: Stack multiple middleware layers

## 🤖 Available Agents

### 🌤️ Weather Agent
- **Purpose**: Provides weather information and forecasts
- **Tools**: Weather data simulation (can be extended with real APIs)
- **Use Cases**: Current weather, forecasts, weather-related questions

### 💬 Chat Agent  
- **Purpose**: General conversation and assistance
- **Tools**: Time information and basic utilities
- **Use Cases**: General questions, casual conversation, help with various topics

### 🔍 Search Agent
- **Purpose**: Web search and real-time information retrieval
- **Tools**: DuckDuckGo web search integration
- **Use Cases**: Current events, research, fact-checking, finding recent information
- **Features**: 
  - Real-time web search
  - Result formatting and summarization
  - Source attribution
  - Search query optimization

## ⚙️ Setup

### Configure the AI Provider

Copy the example configuration:
```powershell
Copy-Item appsettings.example.json appsettings.json
```

Edit `appsettings.json` for your preferred AI provider:

**Option A: Local Ollama** 🟢
```json
{
  "ClientConfiguration": {
    "Ollama": {
      "Endpoint": "http://localhost:11434",
      "ModelName": "llama3.2:3b"
    }
  }
}
```

Prerequisites for Ollama:
```powershell
# Install Ollama
winget install --id=Ollama.Ollama -e

# Pull the model
ollama pull llama3.2:3b
```

**Option B: Azure AI** ☁️
```json
{
  "ClientConfiguration": {
    "AzureAI": {
      "BaseUrl": "https://models.github.ai/inference",
      "ApiKey": "your-actual-api-key",
      "ModelName": "mistral-small-2503"
    }
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

The application will present you with two interactive menus:

1. **Provider Selection**: Choose between Ollama (local) or Azure AI (cloud)
2. **Agent Selection**: Choose from available specialized agents:
   - 🌤️ **Weather Agent**: Weather forecasts and information
   - 💬 **Chat Agent**: General conversation and assistance  
   - 🔍 **Search Agent**: Web search and information retrieval

### Example Interaction

```
🔧 SELECCIÓN DE PROVEEDOR DE IA
========================================
1. 🦙 Ollama (Modelos locales)
2. ☁️  Azure AI (Modelos en la nube)
0. 🚪 Salir

👉 Selecciona una opción: 1

🤖 SELECCIÓN DE AGENTE
========================================
1. 🌤️ Agente del Clima - Proporciona información meteorológica
2. 💬 Agente de Chat - Conversación general y asistencia
3. 🔍 Agente de Búsqueda Web - Busca información en internet usando DuckDuckGo
0. Salir

Selecciona un agente (número): 3

🔍 CHAT INTERACTIVO CON AGENTE DE BÚSQUEDA WEB
============================================================

👤 You: What's happening with AI developments today?

🧠 🔍 Agente de Búsqueda Web is thinking...

🔧 Function Name: SearchWeb - Parameters: query -> AI developments today news

📊 Resultados de búsqueda para: AI developments today news
🔍 Se encontraron 8 resultados relevantes:

1. 📰 **Latest AI Breakthroughs Shape Industry** - TechNews
   Recent developments in artificial intelligence are revolutionizing...

2. 📰 **OpenAI Announces New Model Updates** - AI Times  
   Major updates to language models with improved capabilities...

🔍 Agente de Búsqueda Web: Basándome en los resultados de búsqueda más recientes, aquí tienes un resumen de los desarrollos de IA más importantes de hoy...

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

public class ClientConfiguration
{
    public bool UseAzureAI { get; set; }
    public OllamaConfiguration Ollama { get; set; } = new();
    public AzureAIConfiguration AzureAI { get; set; } = new();
}
```

### Multi-Agent Architecture

The application supports multiple specialized agents managed through the AgentManager:

```csharp
public static Dictionary<int, BaseAgent> GetAvailableAgents()
{
    return new Dictionary<int, BaseAgent>
    {
        { 1, new WeatherAgent() },
        { 2, new ChatAgent() },
        { 3, new SearchAgent() }
    };
}
```

### Web Search Integration

The SearchAgent uses DuckDuckGo for real-time web searches:

```csharp
[Description("Search the web for information using DuckDuckGo. Returns up to 10 relevant results.")]
public static async Task<string> SearchWeb(
    [Description("The search query - what you want to search for on the web")] string query)
{
    // Implementation handles HTTP requests to DuckDuckGo HTML search
    // and parses results into formatted output
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

## 🛠️ Extending the System

### Adding New Agents

To create a new agent, follow these steps:

1. **Create Tools** (if needed) in the `tools/` folder:
```csharp
// tools/MathTools.cs
using System.ComponentModel;

namespace TestOllamaAgent.Tools;

public static class MathTools
{
    [Description("Calculate the sum of two numbers.")]
    public static int Add([Description("First number")] int a, [Description("Second number")] int b)
        => a + b;
}
```

2. **Create the Agent** class in the `agents/` folder:
```csharp
// agents/MathAgent.cs
using Microsoft.Extensions.AI;
using TestOllamaAgent.Tools;

namespace TestOllamaAgent.Agents;

public class MathAgent : BaseAgent
{
    public override string Name => "🧮 Agente Matemático";
    
    public override string Description => "🧮 Agente matemático - Resuelve operaciones matemáticas";
    
    public override string Instructions => "You are a mathematics expert. Help users with mathematical calculations and problems.";

    protected override AIFunction[] GetTools()
    {
        return [AIFunctionFactory.Create(MathTools.Add)];
    }
}
```

3. **Register the Agent** in `AgentManager.cs`:
```csharp
public static Dictionary<int, BaseAgent> GetAvailableAgents()
{
    return new Dictionary<int, BaseAgent>
    {
        { 1, new WeatherAgent() },
        { 2, new ChatAgent() },
        { 3, new SearchAgent() },
        { 4, new MathAgent() }  // Add your new agent
    };
}
```

### Adding New Tools

Create tool classes in the `tools/` folder. Each tool should be a static method with proper descriptions:

```csharp
// tools/DatabaseTools.cs
using System.ComponentModel;

namespace TestOllamaAgent.Tools;

public static class DatabaseTools
{
    [Description("Search for user information in the database.")]
    public static string SearchUser([Description("The user ID to search for.")] string userId)
    {
        // Your implementation here
        return $"User {userId} found";
    }
}
```

For async operations (like web search), use async methods:

```csharp
// tools/ApiTools.cs
[Description("Fetch data from an external API.")]
public static async Task<string> FetchApiData([Description("The API endpoint URL")] string url)
{
    using var client = new HttpClient();
    var response = await client.GetStringAsync(url);
    return response;
}
```

### Adding New Middleware

Create middleware in the `middlewares/` folder:

```csharp
// middlewares/AuthenticationMiddleware.cs
public static class AuthenticationMiddleware
{
    public static async ValueTask<object?> ValidateUser(
        AIAgent agent,
        FunctionInvocationContext context,
        Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next,
        CancellationToken cancellationToken)
    {
        // Add authentication logic here
        Console.WriteLine("🔐 Validating user permissions...");
        
        return await next(context, cancellationToken);
    }
}
```

Then override `GetMiddleware()` in your agent to use custom middleware:

```csharp
public override Func<AIAgent, FunctionInvocationContext, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>>, CancellationToken, ValueTask<object?>>? GetMiddleware()
{
    return AuthenticationMiddleware.ValidateUser;
}
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
- **Microsoft.Agents.AI**: Agent framework for building AI agents
- **Microsoft.Extensions.AI**: AI provider abstraction layer
- **Microsoft.Extensions.AI.AzureAIInference**: Azure AI integration
- **OllamaSharp**: Local AI models with Ollama
- **DuckDuckGo**: Web search integration for real-time information

## 🎯 Next Steps

1. **Experiment** with different models and configurations
2. **Add** new specialized agents (Email, Calendar, Code Analysis, etc.)
3. **Enhance** search capabilities with additional providers
4. **Implement** conversation persistence and history
5. **Explore** other AI providers (OpenAI, Anthropic, Google)
6. **Build** a web interface or API
7. **Add** authentication and user management

## 📖 Resources

- [Microsoft Agent Framework Documentation](https://learn.microsoft.com/es-es/agent-framework/overview/agent-framework-overview)
- [Microsoft.Extensions.AI Documentation](https://learn.microsoft.com/es-es/dotnet/ai/microsoft-extensions-ai)
- [Ollama Documentation](https://docs.ollama.com/)
- [Azure AI Services](https://learn.microsoft.com/en-us/azure/ai-services/)

## 🙏 Acknowledgments

This project was inspired by Anto Subash's excellent article ["Building an Agent Framework with Ollama"](https://blog.antosubash.com/posts/agent-framework-ollama) and his [example repository](https://github.com/antosubash/agent-framework-ollama). The code has been enhanced with additional features and modifications to deepen the learning about Microsoft Agent Framework.

---

*Questions or want to contribute? Open an issue or create a pull request!* 🚀