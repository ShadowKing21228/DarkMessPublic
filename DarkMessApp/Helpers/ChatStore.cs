using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using DarkMessApp.Services;
using DarkMessApp.Utils.Models;

namespace DarkMessApp.Helpers;

public class ChatStore
{
    public static ObservableCollection<ChatModel> Chats { get; } = new();
    public static ObservableCollection<ChatMemberModel> ChatMembers { get; set; } = new();

    public static void HandleChatList(JsonElement element)
    {
        try {
            var newChats = JsonSerializer.Deserialize<List<ChatModel>>(element.GetRawText());
            MainThread.BeginInvokeOnMainThread(() => {
                Chats.Clear();
                foreach (var chat in newChats)
                {
                    Chats.Add(chat);
                    Debug.WriteLine($"Added chat: {chat.ChatId}");
                }
            });
        }
        catch (Exception e) {
            Debug.WriteLine($"Error in HandleChatList: {e}");
        }
    }

    public static void UpdateChatLastMessage(int chatId, string message, string sender, DateTime time)
    {
        var chat = Chats.FirstOrDefault(c => c.ChatId == chatId);
        if (chat == null) return;

        MainThread.BeginInvokeOnMainThread(() => {
            chat.LastMessage = message;
            chat.LastMessageSenderName = sender;
            chat.LastMessageTime = time;
            Debug.WriteLine($"Updated last message in chat {chatId}");
        });
    }
    public static void HandleChatAdd(JsonElement element)
    {
        try {
            var newChat = JsonSerializer.Deserialize<ChatModel>(element.GetRawText());
            MainThread.BeginInvokeOnMainThread(() => {
                Chats.Add(newChat);
                Debug.WriteLine($"Added chat: {newChat.ChatId}");
            });
        }
        catch (Exception e) {
            Debug.WriteLine($"Error in HandleChatAdd: {e}");
        }
    }
    public static void HandleChatMemberList(JsonElement element)
    {
        try {
            var chatMembers = JsonSerializer.Deserialize<List<ChatMemberModel>>(element.GetRawText());
            MainThread.BeginInvokeOnMainThread(() => {
                ChatMembers.Clear();
                foreach (var chatMember in chatMembers) {
                    ChatMembers.Add(chatMember);
                }
            });
        }
        catch (Exception e) {
            Debug.WriteLine($"Error in HandleChatAdd: {e}");
        }
    }
    public static void HandleMemberAdded(JsonElement element)
    {
        try
        {
            var data = JsonSerializer.Deserialize<ChatMemberAddedModel>(element.GetRawText());
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var chat = Chats.FirstOrDefault(c => c.ChatId == data.ChatId);
            });
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Error in HandleMemberAdded: {e}");
        }
    }

    public static void HandleNameChanged(JsonElement element)
    {
        try {
            var data = JsonSerializer.Deserialize<ChatNameChangedModel>(element.GetRawText());
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var chat = Chats.FirstOrDefault(c => c.ChatId == data.ChatId);
                if (chat != null) {
                    chat.Name = data.NewName;
                }
            });
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Error in HandleNameChanged: {e}");
        }
    }
    
    public class ChatMemberAddedModel
    {
        public int ChatId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string AddedBy { get; set; }
    }

    public class ChatNameChangedModel
    {
        public int ChatId { get; set; }
        public string NewName { get; set; }
        public string ChangedBy { get; set; }
    }
}