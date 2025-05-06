using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using DarkMessApp.Helpers;
using DarkMessApp.Utils.Models;
using DarkMessApp.ViewModels;

public class MainPageViewModel : INotifyPropertyChanged
{
    public ChatViewModel ChatVM { get; }
    public MessageViewModel MessageVM { get; }
    public UserProfileViewModel UserProfileVM { get; set; }

    public MainPageViewModel() {
        ChatVM = new ChatViewModel();
        MessageVM = new MessageViewModel();
        UserProfileVM = new UserProfileViewModel();
        MessageStore.OnNewMessageAdded += UpdateChatLastMessage;
    }

    private void UpdateChatLastMessage(MessageModel message)
    {
        var chat = ChatVM.Chats.FirstOrDefault(c => c.ChatId == message.ChatId);
        if (chat == null) return;
    
        MainThread.BeginInvokeOnMainThread(() => {
            chat.LastMessage = message.Content;
            chat.LastMessageSenderName = message.SenderUsername;
            chat.LastMessageTime = message.SentAt;
            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ChatVM)));
            Debug.WriteLine($"Updated last message for chat {chat.ChatId}");
        });
    }
    public event PropertyChangedEventHandler? PropertyChanged;
}