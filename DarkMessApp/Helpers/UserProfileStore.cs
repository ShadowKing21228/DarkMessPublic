using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using DarkMessApp.Utils.Models;

namespace DarkMessApp.Helpers;

public class UserProfileStore
{
    public static UserProfileModel ProfileModel { get; set; }
    public static void UserProfileInit(JsonElement element)
    {
        try {
            Debug.WriteLine(element.GetRawText());
            var message = JsonSerializer.Deserialize<UserProfileModel>(element.GetRawText());
            if (message == null) return;
            ProfileModel = message;
            OnNewMessageAdded?.Invoke();
        }
        catch (Exception e) {
            Debug.WriteLine($"Error in HandlerMessageAdd: {e}");
        }
    }
    public static event Action? OnNewMessageAdded;
}