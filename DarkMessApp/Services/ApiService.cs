using System.Diagnostics;
using System.Text;
using System.Text.Json;
using DarkMessApp.Utils;
using DarkMessApp.Utils.Models;

namespace DarkMessApp.Services;

public static class ApiService
{
    public static readonly Uri ServerHttp = new("https://localhost:5001");
    public static async Task SendRegistration(UserModel user)
    {
        Debug.WriteLine($"SendRegistration: {user}");
        var content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");
        var response = await new HttpClient().PostAsync(new Uri(ServerHttp + "map/api/auth/register"), content);
        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            Debug.WriteLine(responseBody);
            var token = MessUtils.GetJsonElementToken(responseBody, "token");
            await SecureStorage.Default.SetAsync("jwt", token);
        }
        else
        {
            Debug.WriteLine($"Ошибка: {response.StatusCode}");
        }
    }
    public static async Task SendLogin(UserModel user)
    {
        HttpClientHandler handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, errors) => true
        };
        Debug.WriteLine($"SendLogin: {user}");
        var content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");
        var response = await new HttpClient(handler).PostAsync(new Uri(ServerHttp + "map/api/auth/login"), content);
        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            Debug.WriteLine(responseBody);
            var token = MessUtils.GetJsonElementToken(responseBody, "token");
            Debug.WriteLine(token);
            await SecureStorage.Default.SetAsync("jwt", token);
        }
        else
        {
            Debug.WriteLine($"Ошибка: {response.StatusCode}");
        }
    }
}