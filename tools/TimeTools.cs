using System.ComponentModel;

namespace TestOllamaAgent.Tools;

public static class TimeTools
{
    /// <summary>
    /// Function tool for getting the current time.
    /// </summary>
    [Description("Get the current time.")]
    public static string GetCurrentTime()
        => DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");
}