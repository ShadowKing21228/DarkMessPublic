using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using DarkMessApp.Helpers;
using DarkMessApp.Services;
using DarkMessApp.Utils.Models;

namespace DarkMessApp.ViewModels;

public class MessageViewModel : INotifyPropertyChanged
{
    public ObservableCollection<MessageModel> Messages => MessageStore.Messages;

    public MessageViewModel()
    {
        MessageStore.OnNewMessageAdded += OnNewMessageReceived;
    }

    private void OnNewMessageReceived(MessageModel message)
    {
        Debug.WriteLine($"New message received in VM: {message.Content}");
        // Дополнительная логика обработки если нужно
    }
    public void MarkAsRead(int messageId)
    {
        var message = Messages.FirstOrDefault(m => m.MessageId == messageId);
        if (message is { IsRead: false }) {
            message.IsRead = true;
            // Отправка на сервер
            _ = WebSocketService.SendMessageRead(messageId);
        }
    }

    public void CheckMessagesVisibility(IEnumerable<MessageModel> messages)
    {
        foreach (var msg in messages.Where(m => !m.IsRead)) {
            if (IsMessageVisible(msg)) {
                MarkAsRead(msg.MessageId);
            }
        }
    }

    private bool IsMessageVisible(MessageModel message)
    {
        // Логика определения видимости сообщения в интерфейсе
        return true; // Замените реальной проверкой
    }
    public event PropertyChangedEventHandler? PropertyChanged;
}