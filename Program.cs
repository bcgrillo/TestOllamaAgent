using System.ComponentModel;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OllamaSharp;
using Azure.AI.Inference;
using Azure;
using TestOllamaAgent.Configuration;

// ============================================================================
// CONFIGURATION LOADING - Load settings from appsettings.json
// ============================================================================
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var appConfig = new AppConfiguration();
configuration.Bind(appConfig);

// Validate configuration
if (appConfig.ClientConfiguration.UseAzureAI)
{
    if (string.IsNullOrEmpty(appConfig.ClientConfiguration.AzureAI.ApiKey) || 
        appConfig.ClientConfiguration.AzureAI.ApiKey == "YOUR_AZURE_AI_KEY_HERE")
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("❌ ERROR: Azure AI API key is not configured properly.");
        Console.WriteLine("Please update the 'AzureAI.ApiKey' value in appsettings.json");
        Console.ResetColor();
        Environment.Exit(1);
    }
}

/// <summary>
/// Example function tool that the agent can call to get weather information.
/// The Description attributes provide metadata for the AI to understand when and how to use this function.
/// </summary>
[Description("Get the weather for a given location.")]
static string GetWeather([Description("The location to get the weather for.")] string location)
    => location == "Donosti" || location == "San Sebastián"
    ? $"Rainy with a high of 10°C."
    : $"Sunny with a high of 25°C.";

// ============================================================================
// Initialize the selected chat client based on configuration
// ============================================================================
IChatClient chatClient;

if (appConfig.ClientConfiguration.UseAzureAI)
{
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.WriteLine("🔵 Initializing Azure AI client...");
    Console.ResetColor();
    
    try
    {
        var azureClient = new ChatCompletionsClient(
            new Uri(appConfig.ClientConfiguration.AzureAI.BaseUrl), 
            new AzureKeyCredential(appConfig.ClientConfiguration.AzureAI.ApiKey));
        chatClient = azureClient.AsIChatClient(appConfig.ClientConfiguration.AzureAI.ModelName);
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException("Failed to initialize Azure AI service.", ex);
    }
}
else
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("🟢 Initializing Ollama client...");
    Console.ResetColor();
    
    chatClient = new OllamaApiClient(
        new Uri(appConfig.ClientConfiguration.Ollama.Endpoint), 
        appConfig.ClientConfiguration.Ollama.ModelName);
}

// Create the AI agent with the selected client
AIAgent agent = new ChatClientAgent(
    chatClient,
    appConfig.Agent.Instructions,
    appConfig.Agent.Name,
    tools: [AIFunctionFactory.Create(GetWeather)]  // Register our weather function
);

/// <summary>
/// Custom middleware for intercepting and logging function calls.
/// This demonstrates how to add cross-cutting concerns like logging, monitoring, or validation.
/// </summary>
async ValueTask<object?> FunctionCallMiddleware(
    AIAgent agent,
    FunctionInvocationContext context,
    Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next,
    CancellationToken cancellationToken)
{
    // Pre-invocation logging
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write($"\n\n🔧 Function Name: {context!.Function.Name} - ");
    Console.WriteLine($"Parameters: {string.Join(", ", context.Arguments.Select(kv => $"{kv.Key} -> {kv.Value}"))}");
    Console.ResetColor();

    // Execute the function and capture timing
    var startTime = DateTime.UtcNow;
    var result = await next(context, cancellationToken);
    var duration = DateTime.UtcNow - startTime;

    // Post-invocation logging with timing information
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"Response from function: {result} (Duration: {duration.TotalMilliseconds}ms)\n");
    Console.ResetColor();

    return result;
}

// Create an agent with middleware pipeline
var agentWithMiddleware = agent.AsBuilder()
    .Use(FunctionCallMiddleware)  // Add our custom middleware
    .Build();

/// <summary>
/// Helper method for displaying streaming responses with visual formatting.
/// </summary>
async Task DisplayStreamingResponse(string prompt, AIAgent agentToUse)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"\n🧠 {appConfig.Agent.Name} is thinking...");
    Console.ResetColor();

    var isFirstUpdate = true;
    var stream = agentToUse.RunStreamingAsync(prompt);

    await foreach (var update in stream)
    {
        if (isFirstUpdate)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"\n🌤️  {appConfig.Agent.Name}: ");
            Console.ResetColor();
            isFirstUpdate = false;
        }

        Console.Write(update);
        await Task.Delay(10);
    }

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("\n✨ Response complete!");
    Console.ResetColor();
}

// ============================================================================
// Interactive Weather Chat Agent
// Chat with the weather agent using custom messages until Ctrl+C
// ============================================================================
Console.ForegroundColor = ConsoleColor.Magenta;
Console.WriteLine("🌤️  INTERACTIVE WEATHER AGENT CHAT");
Console.WriteLine("=" + new string('=', 60));
Console.WriteLine("💬 Type your weather questions and press Enter");
Console.WriteLine("🛑 Press Ctrl+C to exit");
Console.WriteLine("=" + new string('=', 60));
Console.ResetColor();

try
{
    while (true)
    {
        // Prompt for user input
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("\n👤 You: ");
        Console.ResetColor();
        
        string? userInput = Console.ReadLine();
        
        // Check if user wants to exit or if input is empty
        if (string.IsNullOrWhiteSpace(userInput))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚠️  Please enter a message or press Ctrl+C to exit.");
            Console.ResetColor();
            continue;
        }
        
        // Process the user's message with the weather agent
        await DisplayStreamingResponse(userInput, agentWithMiddleware);
    }
}
catch (OperationCanceledException)
{
    // Handle Ctrl+C gracefully
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("\n\n👋 Thanks for chatting with the Weather Agent!");
    Console.WriteLine("🎉 Chat session ended.");
    Console.ResetColor();
}
