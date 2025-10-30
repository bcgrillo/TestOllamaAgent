using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TestOllamaAgent.Tools;

/// <summary>
/// Mock implementation of weather tools for testing purposes.
/// </summary>
public static class WeatherToolMock
{
    /// <summary>
    /// Mock function that returns fake weather information for testing.
    /// The Description attributes provide metadata for the AI to understand when and how to use this function.
    /// </summary>
    [Description("Get mock weather information for a given location (for testing purposes).")]
    public static string GetWeatherMock([Description("The location to get the weather for.")] string location)
        => location == "Donosti" || location == "San Sebastián"
        ? $"🌧️ Mock Weather: Rainy with a high of 10°C in {location}."
        : $"☀️ Mock Weather: Sunny with a high of 25°C in {location}.";
}

/// <summary>
/// Real weather and geocoding tools using Open Meteo API (free, no API key required).
/// </summary>
public static class WeatherTools
{
    private static readonly HttpClient _httpClient = new HttpClient();
    
    // Open Meteo API endpoints (free, no API key required)
    private const string GEOCODING_URL = "https://geocoding-api.open-meteo.com/v1/search";
    private const string WEATHER_URL = "https://api.open-meteo.com/v1/forecast";

    /// <summary>
    /// Gets coordinates for a location using Open Meteo geocoding API.
    /// </summary>
    [Description("Get geographical coordinates (latitude and longitude) for a given location name.")]
    public static async Task<string> GetCoordinates([Description("The location name to get coordinates for (city name, country code optional).")] string location)
    {
        try
        {
            var url = $"{GEOCODING_URL}?name={Uri.EscapeDataString(location)}&count=5&language=es&format=json";
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                return $"❌ Error en la API de geocodificación (código: {response.StatusCode})\n" +
                       $"URL: {url}";
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var geocodingData = JsonSerializer.Deserialize<GeocodingResponse>(jsonContent);

            if (geocodingData?.Results != null && geocodingData.Results.Length > 0)
            {
                var result = new System.Text.StringBuilder();
                result.AppendLine($"📍 Coordenadas encontradas para '{location}':");
                
                for (int i = 0; i < Math.Min(geocodingData.Results.Length, 3); i++)
                {
                    var place = geocodingData.Results[i];
                    var displayName = !string.IsNullOrEmpty(place.Country) ? 
                        $"{place.Name}, {place.Country}" : place.Name;
                    
                    result.AppendLine($"{i + 1}. {displayName}");
                    result.AppendLine($"   📐 Latitud: {place.Latitude:F4}°");
                    result.AppendLine($"   📐 Longitud: {place.Longitude:F4}°");
                    
                    if (i < Math.Min(geocodingData.Results.Length, 3) - 1)
                        result.AppendLine();
                }

                return result.ToString().TrimEnd();
            }
            else
            {
                return $"❌ No se encontraron coordenadas para '{location}'\n" +
                       $"Respuesta de la API: {jsonContent}";
            }
        }
        catch (Exception ex)
        {
            return $"❌ Error al obtener coordenadas para '{location}': {ex.Message}\n" +
                   $"Tipo de error: {ex.GetType().Name}";
        }
    }

    /// <summary>
    /// Gets current weather information using coordinates.
    /// </summary>
    [Description("Get current weather information for specific coordinates (latitude and longitude).")]
    public static async Task<string> GetWeatherByCoordinates(
        [Description("Latitude coordinate (decimal degrees, e.g. 43.3183)")] double latitude,
        [Description("Longitude coordinate (decimal degrees, e.g. -1.9812)")] double longitude,
        [Description("Optional location name for display purposes")] string locationName = "")
    {
        try
        {
            var weatherUrl = $"{WEATHER_URL}?latitude={latitude.ToString("F4", System.Globalization.CultureInfo.InvariantCulture)}&longitude={longitude.ToString("F4", System.Globalization.CultureInfo.InvariantCulture)}&current=temperature_2m,relative_humidity_2m,apparent_temperature,weather_code,wind_speed_10m&timezone=auto";
            
            var response = await _httpClient.GetAsync(weatherUrl);
            
            if (!response.IsSuccessStatusCode)
            {
                return $"❌ Error en la API del clima (código: {response.StatusCode})\n" +
                       $"URL: {weatherUrl}";
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var weatherData = JsonSerializer.Deserialize<OpenMeteoResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            if (weatherData?.Current != null)
            {
                var temp = Math.Round(weatherData.Current.Temperature2m, 1);
                var feelsLike = Math.Round(weatherData.Current.ApparentTemperature, 1);
                var humidity = weatherData.Current.RelativeHumidity2m;
                var windSpeed = Math.Round(weatherData.Current.WindSpeed10m, 1);
                var weatherCode = weatherData.Current.WeatherCode;
                
                var weatherIcon = GetWeatherIconFromCode(weatherCode);
                var weatherDescription = GetWeatherDescription(weatherCode);

                var locationDisplay = !string.IsNullOrEmpty(locationName) ? 
                    locationName : $"({latitude:F4}°, {longitude:F4}°)";

                return $"{weatherIcon} Clima actual en {locationDisplay}:\n" +
                       $"🌡️ Temperatura: {temp}°C (se siente como {feelsLike}°C)\n" +
                       $"📝 Descripción: {weatherDescription}\n" +
                       $"💧 Humedad: {humidity}%\n" +
                       $"💨 Viento: {windSpeed} km/h\n" +
                       $"📡 Datos de Open Meteo\n" +
                       $"📍 Coordenadas: {latitude:F4}°, {longitude:F4}°";
            }
            else
            {
                return $"❌ No se pudieron procesar los datos del clima\n" +
                       $"Respuesta de la API: {jsonContent}";
            }
        }
        catch (Exception ex)
        {
            return $"❌ Error al obtener el clima para las coordenadas ({latitude:F4}°, {longitude:F4}°): {ex.Message}\n" +
                   $"Tipo de error: {ex.GetType().Name}";
        }
    }

    /// <summary>
    /// Gets weather information for a location (combines geocoding and weather lookup).
    /// </summary>
    [Description("Get current weather information for a given location name (combines coordinate lookup and weather data).")]
    public static async Task<string> GetWeather([Description("The location to get the weather for (ONLY city or place name, without comma, symbols or country name).")] string location)
    {
        try
        {
            // Step 1: Get coordinates for the location using geocoding
            var coordinates = await GetCoordinatesInternalAsync(location);
            if (coordinates == null)
            {
                return $"❌ No se pudieron encontrar las coordenadas para '{location}'.\n" +
                       $"Intenta ser más específico (ej: 'Madrid, España' en lugar de solo 'Madrid').";
            }

            // Step 2: Get weather data using coordinates
            return await GetWeatherByCoordinates(coordinates.Value.lat, coordinates.Value.lon, coordinates.Value.name);
        }
        catch (Exception ex)
        {
            return $"❌ Error al obtener el clima para '{location}': {ex.Message}\n" +
                   $"Tipo de error: {ex.GetType().Name}";
        }
    }

    /// <summary>
    /// Internal method to get coordinates (returns structured data instead of formatted string).
    /// </summary>
    private static async Task<(double lat, double lon, string name)?> GetCoordinatesInternalAsync(string location)
    {
        try
        {
            var url = $"{GEOCODING_URL}?name={Uri.EscapeDataString(location)}&count=1&language=es&format=json";
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var geocodingData = JsonSerializer.Deserialize<GeocodingResponse>(jsonContent);

            if (geocodingData?.Results != null && geocodingData.Results.Length > 0)
            {
                var result = geocodingData.Results[0];
                var displayName = !string.IsNullOrEmpty(result.Country) ? 
                    $"{result.Name}, {result.Country}" : result.Name;
                    
                return (result.Latitude, result.Longitude, displayName);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets appropriate weather icon based on Open Meteo weather code.
    /// </summary>
    private static string GetWeatherIconFromCode(int weatherCode)
    {
        return weatherCode switch
        {
            0 => "☀️", // Clear sky
            1 or 2 or 3 => "⛅", // Mainly clear, partly cloudy, overcast
            45 or 48 => "🌫️", // Fog
            51 or 53 or 55 => "🌦️", // Drizzle
            56 or 57 => "🌨️", // Freezing drizzle
            61 or 63 or 65 => "🌧️", // Rain
            66 or 67 => "🌨️", // Freezing rain
            71 or 73 or 75 => "❄️", // Snow fall
            77 => "🌨️", // Snow grains
            80 or 81 or 82 => "🌦️", // Rain showers
            85 or 86 => "🌨️", // Snow showers
            95 => "⛈️", // Thunderstorm
            96 or 99 => "⛈️", // Thunderstorm with hail
            _ => "🌤️" // Default
        };
    }

    /// <summary>
    /// Gets weather description based on Open Meteo weather code.
    /// </summary>
    private static string GetWeatherDescription(int weatherCode)
    {
        return weatherCode switch
        {
            0 => "Cielo despejado",
            1 => "Principalmente despejado",
            2 => "Parcialmente nublado",
            3 => "Nublado",
            45 => "Niebla",
            48 => "Niebla con escarcha",
            51 => "Llovizna ligera",
            53 => "Llovizna moderada",
            55 => "Llovizna intensa",
            56 => "Llovizna helada ligera",
            57 => "Llovizna helada intensa",
            61 => "Lluvia ligera",
            63 => "Lluvia moderada",
            65 => "Lluvia intensa",
            66 => "Lluvia helada ligera",
            67 => "Lluvia helada intensa",
            71 => "Nevada ligera",
            73 => "Nevada moderada",
            75 => "Nevada intensa",
            77 => "Granizo blando",
            80 => "Chubascos ligeros",
            81 => "Chubascos moderados",
            82 => "Chubascos intensos",
            85 => "Chubascos de nieve ligeros",
            86 => "Chubascos de nieve intensos",
            95 => "Tormenta",
            96 => "Tormenta con granizo ligero",
            99 => "Tormenta con granizo intenso",
            _ => "Condición desconocida"
        };
    }
}

/// <summary>
/// Data models for Open Meteo API response.
/// </summary>
public class OpenMeteoResponse
{
    [JsonPropertyName("current")]
    public CurrentWeather Current { get; set; } = new();
}

public class CurrentWeather
{
    [JsonPropertyName("temperature_2m")]
    public double Temperature2m { get; set; }
    
    [JsonPropertyName("apparent_temperature")]
    public double ApparentTemperature { get; set; }
    
    [JsonPropertyName("relative_humidity_2m")]
    public int RelativeHumidity2m { get; set; }
    
    [JsonPropertyName("wind_speed_10m")]
    public double WindSpeed10m { get; set; }
    
    [JsonPropertyName("weather_code")]
    public int WeatherCode { get; set; }
}

/// <summary>
/// Data models for geocoding API response.
/// </summary>
public class GeocodingResponse
{
    [JsonPropertyName("results")]
    public GeocodingResult[] Results { get; set; } = Array.Empty<GeocodingResult>();
    
    [JsonPropertyName("generationtime_ms")]
    public double GenerationTimeMs { get; set; }
}

public class GeocodingResult
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
    
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }
    
    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }
    
    [JsonPropertyName("elevation")]
    public double Elevation { get; set; }
    
    [JsonPropertyName("feature_code")]
    public string FeatureCode { get; set; } = "";
    
    [JsonPropertyName("country_code")]
    public string CountryCode { get; set; } = "";
    
    [JsonPropertyName("admin1_id")]
    public int? Admin1Id { get; set; }
    
    [JsonPropertyName("admin2_id")]
    public int? Admin2Id { get; set; }
    
    [JsonPropertyName("admin3_id")]
    public int? Admin3Id { get; set; }
    
    [JsonPropertyName("timezone")]
    public string Timezone { get; set; } = "";
    
    [JsonPropertyName("population")]
    public int? Population { get; set; }
    
    [JsonPropertyName("postcodes")]
    public string[] Postcodes { get; set; } = Array.Empty<string>();
    
    [JsonPropertyName("country_id")]
    public int CountryId { get; set; }
    
    [JsonPropertyName("country")]
    public string Country { get; set; } = "";
    
    [JsonPropertyName("admin1")]
    public string Admin1 { get; set; } = "";
    
    [JsonPropertyName("admin2")]
    public string Admin2 { get; set; } = "";
    
    [JsonPropertyName("admin3")]
    public string Admin3 { get; set; } = "";
}