using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using DarkMessApp.Services;
using DarkMessApp.Utils.Models;

namespace DarkMessApp.Helpers;

public class MessageStore
{
    public static ObservableCollection<MessageModel> Messages { get; } = new();
    public static event Action<MessageModel>? OnNewMessageAdded;

    public static void HandleMessageList(JsonElement element)
    {
        try
        {
            var messages = JsonSerializer.Deserialize<List<MessageModel>>(element.GetRawText());
            MainThread.BeginInvokeOnMainThread(() => 
            {
                Messages.Clear();
                foreach (var message in messages)
                {
                    message.IsMyMessage = message.SenderId == UserProfileStore.ProfileModel.UserId;
                    Messages.Add(message);
                    Debug.WriteLine($"Added message: {message.Content}");
                }
            });
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Error in HandleMessageList: {e}");
        }
    }

    public static void HandlerMessageAdd(JsonElement element)
    {
        try {
            var message = JsonSerializer.Deserialize<MessageModel>(element.GetRawText());
            if (message == null) return;

            MainThread.BeginInvokeOnMainThread(() => 
            {
                message.IsMyMessage = message.SenderId == UserProfileStore.ProfileModel.UserId;
                Messages.Add(message);
                OnNewMessageAdded?.Invoke(message);
                Debug.WriteLine($"New message added: {message.Content}");
            });
            ChatStore.UpdateChatLastMessage(message.ChatId, message.Content, message.SenderUsername, message.SentAt);
        }
        catch (Exception e) {
            Debug.WriteLine($"Error in HandlerMessageAdd: {e}");
        }
    }
    public static void HandlerMessageRead(JsonElement element)
    {
        try {
            var message = JsonSerializer.Deserialize<MessageStatusModel>(element.GetRawText());
            if (message == null) return;

            MainThread.BeginInvokeOnMainThread(() => {
                Messages.FirstOrDefault(m => m.MessageId == message.MessageId)!.IsRead = true;
            });
        }
        catch (Exception e) {
            Debug.WriteLine($"Error in HandlerMessageAdd: {e}");
        }
    }
}