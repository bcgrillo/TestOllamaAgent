using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OllamaSharp;
using Azure.AI.Inference;
using Azure;
using TestOllamaAgent.Configuration;
using TestOllamaAgent.Agents;

// ============================================================================
// CONFIGURATION LOADING - Load settings from appsettings.json
// ============================================================================
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var appConfig = new AppConfiguration();
configuration.Bind(appConfig);

// ============================================================================
// Helper Methods for Provider and Model Selection
// ============================================================================

/// <summary>
/// Checks if Ollama is available and properly configured.
/// </summary>
static async Task<bool> IsOllamaAvailableAsync(string? endpoint)
{
    try
    {
        if (string.IsNullOrEmpty(endpoint))
            return false;
            
        var ollamaClient = new OllamaApiClient(new Uri(endpoint));
        var models = await ollamaClient.ListLocalModelsAsync();
        return models.Any();
    }
    catch
    {
        return false;
    }
}

/// <summary>
/// Checks if Azure AI is available and properly configured.
/// </summary>
static bool IsAzureAIAvailable(AzureAIConfiguration? azureConfig)
{
    if (azureConfig == null)
        return false;
        
    return !string.IsNullOrEmpty(azureConfig.ApiKey) && 
           azureConfig.ApiKey != "YOUR_AZURE_AI_KEY_HERE" &&
           !string.IsNullOrEmpty(azureConfig.BaseUrl) &&
           azureConfig.AvailableModels.Any();
}

/// <summary>
/// Gets available providers based on configuration and connectivity.
/// </summary>
static async Task<Dictionary<int, (string Name, bool IsAzure)>> GetAvailableProvidersAsync(AppConfiguration config)
{
    var availableProviders = new Dictionary<int, (string Name, bool IsAzure)>();
    int optionNumber = 1;

    // Check Ollama availability
    bool ollamaAvailable = await IsOllamaAvailableAsync(config.ClientConfiguration.Ollama.Endpoint);
    if (ollamaAvailable)
    {
        availableProviders.Add(optionNumber++, ("🦙 Ollama (Modelos locales)", false));
    }

    // Check Azure AI availability
    bool azureAvailable = IsAzureAIAvailable(config.ClientConfiguration.AzureAI);
    if (azureAvailable)
    {
        availableProviders.Add(optionNumber++, ("☁️  Azure AI (Modelos en la nube)", true));
    }

    return availableProviders;
}

/// <summary>
/// Displays the provider selection menu and returns the user's choice.
/// </summary>
static int DisplayProviderMenu(Dictionary<int, (string Name, bool IsAzure)> availableProviders)
{
    Console.ForegroundColor = ConsoleColor.Magenta;
    Console.WriteLine("\n🔧 SELECCIÓN DE PROVEEDOR DE IA");
    Console.WriteLine("=" + new string('=', 40));
    Console.ResetColor();
    
    if (!availableProviders.Any())
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("❌ No hay proveedores de IA disponibles.");
        Console.WriteLine("   Verifica la configuración de Ollama y Azure AI.");
        Console.ResetColor();
        return 0;
    }
    
    foreach (var provider in availableProviders)
    {
        Console.WriteLine($"{provider.Key}. {provider.Value.Name}");
    }
    
    Console.WriteLine("0. 🚪 Salir");
    
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("\n👉 Selecciona una opción: ");
    Console.ResetColor();
    
    if (int.TryParse(Console.ReadLine(), out int choice))
    {
        return choice;
    }
    
    return -1;
}

/// <summary>
/// Gets available Ollama models from the local installation.
/// </summary>
static async Task<List<string>> GetAvailableOllamaModelsAsync(string? endpoint)
{
    try
    {
        if (string.IsNullOrEmpty(endpoint))
            return new List<string>();
            
        var ollamaClient = new OllamaApiClient(new Uri(endpoint));
        var models = await ollamaClient.ListLocalModelsAsync();
        return models.Select(m => m.Name).ToList();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Error al obtener modelos de Ollama: {ex.Message}");
        Console.ResetColor();
        return new List<string>();
    }
}

/// <summary>
/// Displays model selection menu and returns the selected model.
/// </summary>
static string? DisplayModelSelection(List<string> models, string providerName)
{
    if (!models.Any())
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ No hay modelos disponibles para {providerName}.");
        Console.ResetColor();
        return null;
    }
    
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"\n🎯 MODELOS DISPONIBLES - {providerName.ToUpper()}");
    Console.WriteLine("=" + new string('=', 50));
    Console.ResetColor();
    
    for (int i = 0; i < models.Count; i++)
    {
        Console.WriteLine($"{i + 1}. {models[i]}");
    }
    Console.WriteLine("0. 🔙 Volver atrás");
    
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("\n👉 Selecciona un modelo: ");
    Console.ResetColor();
    
    if (int.TryParse(Console.ReadLine(), out int choice))
    {
        if (choice == 0) return null;
        if (choice > 0 && choice <= models.Count)
        {
            return models[choice - 1];
        }
    }
    
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("❌ Selección inválida.");
    Console.ResetColor();
    return null;
}

/// <summary>
/// Creates the appropriate chat client based on provider and model selection.
/// </summary>
static IChatClient CreateChatClient(bool useAzureAI, string modelName, AppConfiguration config)
{
    if (useAzureAI)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"🔵 Inicializando cliente Azure AI con modelo: {modelName}");
        Console.ResetColor();
        
        try
        {
            var baseUrl = config.ClientConfiguration.AzureAI.BaseUrl ?? throw new InvalidOperationException("Azure AI BaseUrl no configurada");
            var apiKey = config.ClientConfiguration.AzureAI.ApiKey ?? throw new InvalidOperationException("Azure AI ApiKey no configurada");
            
            var azureClient = new ChatCompletionsClient(
                new Uri(baseUrl), 
                new AzureKeyCredential(apiKey));
            
            return azureClient.AsIChatClient(modelName);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to initialize Azure AI service.", ex);
        }
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"🟢 Inicializando cliente Ollama con modelo: {modelName}");
        Console.ResetColor();
        
        var endpoint = config.ClientConfiguration.Ollama.Endpoint ?? throw new InvalidOperationException("Ollama Endpoint no configurado");
        
        return new OllamaApiClient(
            new Uri(endpoint), 
            modelName);
    }
}

/// <summary>
/// Helper method for displaying streaming responses with visual formatting.
/// </summary>
async Task DisplayStreamingResponse(string prompt, AIAgent agentToUse, string agentName)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"\n🧠 {agentName} está pensando...");
    Console.ResetColor();

    var isFirstUpdate = true;
    var stream = agentToUse.RunStreamingAsync(prompt);

    await foreach (var update in stream)
    {
        if (isFirstUpdate)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"\n🤖 {agentName}: ");
            Console.ResetColor();
            isFirstUpdate = false;
        }

        Console.Write(update);
        await Task.Delay(10);
    }

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("\n✨ Respuesta completada!");
    Console.ResetColor();
}

/// <summary>
/// Runs the agent selection and interaction loop.
/// </summary>
static async Task RunAgentInteractionLoop(IChatClient chatClient, Func<string, AIAgent, string, Task> displayStreamingResponse)
{
    while (true)
    {
        AgentManager.DisplayAgentMenu();
        int selection = AgentManager.GetUserSelection();
        
        if (selection == 0)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("🔄 Volviendo al menú de proveedores...\n");
            Console.ResetColor();
            break;
        }

        var availableAgents = AgentManager.GetAvailableAgents();
        
        if (!availableAgents.ContainsKey(selection))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ Selección inválida. Por favor, elige una opción válida.");
            Console.ResetColor();
            continue;
        }

        var selectedAgent = availableAgents[selection];
        
        // Create the selected agent with middleware already applied
        var agentToUse = selectedAgent.CreateAgent(chatClient);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n✅ Agente '{selectedAgent.Name}' activado!");
        Console.WriteLine("💬 Escribe tus mensajes y presiona Enter");
        Console.WriteLine("🔙 Escribe 'menu' para volver al menú de agentes");
        Console.WriteLine("🔧 Escribe 'provider' para cambiar de proveedor");
        Console.WriteLine("🛑 Presiona Ctrl+C para salir");
        Console.WriteLine("=" + new string('=', 60));
        Console.ResetColor();

        // Chat loop for the selected agent
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\n👤 Tú: ");
            Console.ResetColor();
            
            string? userInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(userInput))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠️  Por favor, introduce un mensaje.");
                Console.ResetColor();
                continue;
            }
            
            string inputLower = userInput.Trim().ToLower();
            
            if (inputLower == "menu")
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("🔄 Volviendo al menú de agentes...\n");
                Console.ResetColor();
                break;
            }
            
            if (inputLower == "provider")
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("🔄 Volviendo al menú de proveedores...\n");
                Console.ResetColor();
                return; // Exit the agent loop and go back to provider selection
            }
            
            await displayStreamingResponse(userInput, agentToUse, selectedAgent.Name);
        }
    }
}

// ============================================================================
// Main Application Flow
// ============================================================================
Console.ForegroundColor = ConsoleColor.Magenta;
Console.WriteLine("🤖 SISTEMA DE AGENTES DE PRUEBA");
Console.WriteLine("=" + new string('=', 60));
Console.ResetColor();

try
{
    while (true)
    {
        // Step 1: Get Available Providers
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("� Verificando proveedores disponibles...");
        Console.ResetColor();
        
        var availableProviders = await GetAvailableProvidersAsync(appConfig);
        
        if (!availableProviders.Any())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ No se encontraron proveedores de IA disponibles.");
            Console.WriteLine("\n📋 Verifica la configuración:");
            Console.WriteLine("   • Para Ollama: Asegúrate de que esté ejecutándose y tenga modelos instalados");
            Console.WriteLine("   • Para Azure AI: Verifica la API key y modelos disponibles en appsettings.json");
            Console.ResetColor();
            break;
        }
        
        // Step 2: Provider Selection
        int providerChoice = DisplayProviderMenu(availableProviders);
        
        if (providerChoice == 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("👋 ¡Hasta luego!");
            Console.ResetColor();
            break;
        }
        
        if (!availableProviders.ContainsKey(providerChoice))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ Selección inválida. Por favor, elige una opción válida.");
            Console.ResetColor();
            continue;
        }
        
        var selectedProvider = availableProviders[providerChoice];
        bool useAzureAI = selectedProvider.IsAzure;
        
        // Step 3: Model Selection
        List<string> availableModels;
        string providerName;
        
        if (useAzureAI)
        {
            availableModels = appConfig.ClientConfiguration.AzureAI.AvailableModels;
            providerName = "Azure AI";
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("🔍 Obteniendo modelos disponibles de Ollama...");
            Console.ResetColor();
            
            availableModels = await GetAvailableOllamaModelsAsync(appConfig.ClientConfiguration.Ollama.Endpoint);
            providerName = "Ollama";
        }
        
        string? selectedModel = DisplayModelSelection(availableModels, providerName);
        
        if (selectedModel == null)
        {
            continue; // Go back to provider selection
        }
        
        // Step 4: Create Chat Client
        IChatClient chatClient;
        try
        {
            chatClient = CreateChatClient(useAzureAI, selectedModel, appConfig);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Error al inicializar el cliente: {ex.Message}");
            Console.ResetColor();
            continue;
        }
        
        // Step 5: Agent Selection and Interaction
        await RunAgentInteractionLoop(chatClient, DisplayStreamingResponse);
    }
}
catch (OperationCanceledException)
{
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("\n\n👋 ¡Gracias por probar el sistema de agentes!");
    Console.WriteLine("🎉 Sesión finalizada.");
    Console.ResetColor();
}