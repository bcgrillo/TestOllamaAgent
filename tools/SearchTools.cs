using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Web;

namespace TestOllamaAgent.Tools;

/// <summary>
/// Web search tools using DuckDuckGo HTML search.
/// </summary>
public static class SearchTools
{
    private static readonly HttpClient _httpClient = new HttpClient();
    
    // DuckDuckGo HTML search endpoint
    private const string DUCKDUCKGO_HTML_URL = "https://html.duckduckgo.com/html/";

    static SearchTools()
    {
        // Set a user agent to identify our application
        _httpClient.DefaultRequestHeaders.Add("User-Agent", 
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
    }

    /// <summary>
    /// Searches the web using DuckDuckGo HTML search.
    /// </summary>
    [Description("Search the web for information using DuckDuckGo. Returns up to 10 relevant results.")]
    public static async Task<string> SearchWeb(
        [Description("The search query - what you want to search for on the web")] string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return "❌ La consulta de búsqueda no puede estar vacía.";
            }

            // DuckDuckGo HTML search
            var encodedQuery = HttpUtility.UrlEncode(query);
            var url = $"{DUCKDUCKGO_HTML_URL}?q={encodedQuery}";
            
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                return $"❌ Error en la búsqueda (código: {response.StatusCode})\n" +
                       $"URL: {url}";
            }

            var htmlContent = await response.Content.ReadAsStringAsync();
            var searchResults = ParseDuckDuckGoHtml(htmlContent);

            var result = new System.Text.StringBuilder();
            result.AppendLine($"🔍 Resultados de búsqueda para: '{query}'\n");

            if (searchResults.Count == 0)
            {
                result.AppendLine("❌ No se encontraron resultados para esta búsqueda.");
                result.AppendLine("💡 Intenta con palabras clave diferentes o más específicas.");
                return result.ToString().TrimEnd();
            }

            result.AppendLine("🌐 **Resultados web:**");
            
            for (int i = 0; i < Math.Min(searchResults.Count, 10); i++)
            {
                var webResult = searchResults[i];
                result.AppendLine($"{i + 1}. **{webResult.Title}**");
                if (!string.IsNullOrEmpty(webResult.Snippet))
                {
                    result.AppendLine($"   {webResult.Snippet}");
                }
                if (!string.IsNullOrEmpty(webResult.Url))
                {
                    result.AppendLine($"   🔗 {webResult.Url}");
                }
                result.AppendLine();
            }

            return result.ToString().TrimEnd();
        }
        catch (Exception ex)
        {
            return $"❌ Error al realizar la búsqueda web: {ex.Message}\n" +
                   $"Consulta: '{query}'\n" +
                   $"Tipo de error: {ex.GetType().Name}";
        }
    }

    private static List<SearchResult> ParseDuckDuckGoHtml(string html)
    {
        var results = new List<SearchResult>();
        
        try
        {
            // Extract search results using regex patterns
            // Pattern to match result containers
            var resultPattern = @"<div[^>]*class=""[^""]*result[^""]*results_links[^""]*""[^>]*>.*?</div>\s*</div>";
            var resultMatches = Regex.Matches(html, resultPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);

            foreach (Match resultMatch in resultMatches)
            {
                var resultHtml = resultMatch.Value;
                
                // Extract title
                var titlePattern = @"<h2[^>]*class=""[^""]*result__title[^""]*""[^>]*>.*?<a[^>]*>([^<]+)</a>";
                var titleMatch = Regex.Match(resultHtml, titlePattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                var title = titleMatch.Success ? CleanHtmlText(titleMatch.Groups[1].Value) : "";

                // Extract URL
                var urlPattern = @"<a[^>]*class=""[^""]*result__url[^""]*""[^>]*[^>]*>([^<]+)</a>";
                var urlMatch = Regex.Match(resultHtml, urlPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                var url = urlMatch.Success ? CleanHtmlText(urlMatch.Groups[1].Value) : "";

                // Extract snippet
                var snippetPattern = @"<a[^>]*class=""[^""]*result__snippet[^""]*""[^>]*>([^<]+(?:<b>[^<]*</b>[^<]*)*)</a>";
                var snippetMatch = Regex.Match(resultHtml, snippetPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                var snippet = snippetMatch.Success ? CleanHtmlText(snippetMatch.Groups[1].Value) : "";

                if (!string.IsNullOrEmpty(title))
                {
                    results.Add(new SearchResult
                    {
                        Title = title,
                        Url = url,
                        Snippet = snippet
                    });
                }
            }
        }
        catch (Exception ex)
        {
            // If regex parsing fails, try a simpler approach
            Console.WriteLine($"Error parsing HTML: {ex.Message}");
        }

        return results;
    }

    private static string CleanHtmlText(string htmlText)
    {
        if (string.IsNullOrEmpty(htmlText))
            return "";

        // Remove HTML tags
        var cleanText = Regex.Replace(htmlText, @"<[^>]+>", "");
        
        // Decode HTML entities
        cleanText = HttpUtility.HtmlDecode(cleanText);
        
        // Clean up whitespace
        cleanText = Regex.Replace(cleanText, @"\s+", " ").Trim();
        
        return cleanText;
    }
}

/// <summary>
/// Data model for search results from HTML parsing.
/// </summary>
public class SearchResult
{
    public string Title { get; set; } = "";
    public string Url { get; set; } = "";
    public string Snippet { get; set; } = "";
}