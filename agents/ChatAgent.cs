using Microsoft.Extensions.AI;
using TestOllamaAgent.Tools;

namespace TestOllamaAgent.Agents;

public class ChatAgent : BaseAgent
{
    public override string Name => "ChatBot";
    
    public override string Description => "💬 Agente de conversación general - Chat amigable con acceso a la hora actual";
    
    public override string Instructions => "You are a friendly assistant. You can help with general questions and provide the current time when asked. Be helpful and conversational.";

    protected override AIFunction[] GetTools()
    {
        return [AIFunctionFactory.Create(TimeTools.GetCurrentTime)];
    }
}