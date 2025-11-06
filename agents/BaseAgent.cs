using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using TestOllamaAgent.Middlewares;

namespace TestOllamaAgent.Agents;

public abstract class BaseAgent
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract string Instructions { get; }
    
    protected abstract AIFunction[] GetTools();
    
    public virtual Func<AIAgent, FunctionInvocationContext, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>>, CancellationToken, ValueTask<object?>>? GetMiddleware()
    {
        return FunctionCallMiddleware.LoggingMiddleware;
    }
    
    public AIAgent CreateAgent(IChatClient chatClient)
    {
        // Create the base agent
        var agent = new ChatClientAgent(
            chatClient,
            Instructions,
            Name,
            tools: GetTools()
        );

        // Apply middleware if available
        var middleware = GetMiddleware();
        if (middleware != null)
        {
            return agent.AsBuilder()
                .Use(middleware)
                .Build();
        }

        return agent;
    }
}