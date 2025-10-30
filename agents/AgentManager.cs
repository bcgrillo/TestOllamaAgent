using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace TestOllamaAgent.Agents;

public static class AgentManager
{
    public static Dictionary<int, BaseAgent> GetAvailableAgents()
    {
        return new Dictionary<int, BaseAgent>
        {
            { 1, new WeatherAgent() },
            { 2, new ChatAgent() },
            { 3, new SearchAgent() }
        };
    }

    public static void DisplayAgentMenu()
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("🤖 SELECCIÓN DE AGENTE");
        Console.WriteLine("=" + new string('=', 40));
        Console.ResetColor();

        var agents = GetAvailableAgents();
        foreach (var agent in agents)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{agent.Key}. ");
            Console.ResetColor();
            Console.WriteLine(agent.Value.Description);
        }

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("0. Salir");
        Console.WriteLine("=" + new string('=', 40));
        Console.ResetColor();
    }

    public static int GetUserSelection()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("Selecciona un agente (número): ");
        Console.ResetColor();
        
        if (int.TryParse(Console.ReadLine(), out int selection))
        {
            return selection;
        }
        
        return -1;
    }
}