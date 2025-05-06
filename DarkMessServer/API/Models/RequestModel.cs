using System.Text.Json;

namespace DarkMessServer.API.Models;

public class RequestModel
{
    public string Type { get; set; } = string.Empty;
    public JsonElement Element { get; set; }
}