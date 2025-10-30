using Microsoft.Extensions.AI;
using TestOllamaAgent.Tools;

namespace TestOllamaAgent.Agents;

public class WeatherAgent : BaseAgent
{
    public override string Name => "WeatherMan";
    
    public override string Description => "🌤️  Agente del tiempo - Proporciona información meteorológica";
    
    public override string Instructions => "You are a professional weather forecaster. Only provide weather information or forecasts if the user requests it. Otherwise, politely indicate that you can answer weather-related questions.";

    protected override AIFunction[] GetTools()
    {
        return [AIFunctionFactory.Create(WeatherTools.GetWeather)];
    }
}