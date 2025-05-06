using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using DarkMessApp.Services;

namespace DarkMessApp.Utils;

public static class MessUtils
{
    public static string GetJsonElementToken(string responseBody, string jsonElement)
    {
        using JsonDocument jsonDocument = JsonDocument.Parse(responseBody);
        jsonDocument.RootElement.TryGetProperty(jsonElement, out JsonElement accessToken);
        return accessToken.ToString();
    }
    public static async Task<bool> PingServer()
    {
        var server = new Uri(ApiService.ServerHttp, "map/api/auth/ping");
        Debug.WriteLine(server);
        Debug.WriteLine(server.ToString());
        try {
            using var client = new HttpClient();
            var content = new StringContent(string.Empty);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await client.PostAsync(server, content);
            Debug.WriteLine($"PingServer: {response.StatusCode}");
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException e)
        {
            Debug.WriteLine("Server is closed: " + e.Message);
            return false;
        }
        catch (Exception e)
        {
            Debug.WriteLine("Critical Error: " + e);
            throw;
        }
    }
}