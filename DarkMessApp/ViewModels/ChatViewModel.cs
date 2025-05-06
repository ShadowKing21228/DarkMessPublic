using System.Collections.ObjectModel;
using System.ComponentModel;
using DarkMessApp.Helpers;
using DarkMessApp.Utils.Models;

namespace DarkMessApp.ViewModels;

public class ChatViewModel : INotifyPropertyChanged
{
    public ObservableCollection<ChatModel> Chats => ChatStore.Chats;
    public ObservableCollection<ChatMemberModel> ChatMembers => ChatStore.ChatMembers;
    public ChatViewModel()
    {
        ChatStore.Chats.CollectionChanged += (s, e) => {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Chats)));
        };
        ChatStore.ChatMembers.CollectionChanged += (s, e) => {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ChatMembers)));
        };
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}