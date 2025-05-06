using System.Text.Json;

namespace DarkMessApp.Utils.Models;

public class WSRequestModel
{
    public string Type { get; set; } = "";
    public JsonElement Element { get; set; }
}