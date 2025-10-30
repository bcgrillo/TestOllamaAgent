using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace TestOllamaAgent.Middlewares;

public static class FunctionCallMiddleware
{
    /// <summary>
    /// Custom middleware for intercepting and logging function calls.
    /// This demonstrates how to add cross-cutting concerns like logging, monitoring, or validation.
    /// </summary>
    public static async ValueTask<object?> LoggingMiddleware(
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
}