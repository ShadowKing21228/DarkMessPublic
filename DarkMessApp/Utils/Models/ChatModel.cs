using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DarkMessApp.Utils.Models;
public class ChatModel : INotifyPropertyChanged
{
    private string _lastMessage;
    private string _lastMessageSenderName;
    private DateTime? _lastMessageTime;
    private string _name;
    public int ChatId { get; set; }
    public string LastMessage {
        get => _lastMessage;
        set {
            if (_lastMessage != value) {
                _lastMessage = value;
                OnPropertyChanged();
            }
        }
    }
    public string LastMessageSenderName {
        get => _lastMessageSenderName;
        set {
            if (_lastMessageSenderName != value) {
                _lastMessageSenderName = value;
                OnPropertyChanged();
            }
        }
    }
    public DateTime? LastMessageTime {
        get => _lastMessageTime;
        set {
            if (_lastMessageTime != value) {
                _lastMessageTime = value;
                OnPropertyChanged();
            }
        }
    }
    public string Name {
        get => _name;
        set {
            if (_name != value) {
                _name = value;
                OnPropertyChanged();
            }
        }
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}