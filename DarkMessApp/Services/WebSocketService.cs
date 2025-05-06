using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using DarkMessApp.Helpers;
using DarkMessApp.Utils.Models;

namespace DarkMessApp.Services;

public static class WebSocketService
{
    public static readonly Uri ServerWSUri = new("wss://localhost:5001");
    private static ClientWebSocket _socket;
    private static readonly Dictionary<string, Action<JsonElement>> _messageHandlers = new()
    {
        { "chat_list", ChatStore.HandleChatList },
        { "chat_create", ChatStore.HandleChatAdd },
        { "chat_members", ChatStore.HandleChatMemberList},
        { "message_list", MessageStore.HandleMessageList },
        { "message", MessageStore.HandlerMessageAdd },
        { "user_profile", UserProfileStore.UserProfileInit },
        { "message_read", MessageStore.HandlerMessageRead },
        { "chat_member_added", ChatStore.HandleMemberAdded },
        { "chat_name_changed", ChatStore.HandleNameChanged }
        // { "new_message", HandleNewMessage }
    };

    public static async Task ConnectAsync(Uri uri)
    {
        _socket = new ClientWebSocket();
        _socket.Options.SetRequestHeader("Authorization", "Bearer " + await SecureStorage.Default.GetAsync("jwt"));
        try {
            Debug.WriteLine($"Connecting... to {uri}");
            await _socket.ConnectAsync(uri, CancellationToken.None); }
        catch (WebSocketException e) {
            Debug.WriteLine("Аутентификация провалилась: " + e); }
        finally {
            _ = ReceiveLoop(); }
    }

    public static bool IsConnected()
    {
        return _socket.State != WebSocketState.Closed;
    }

    private static async Task ReceiveLoop()
    {
        var buffer = new byte[4096];
        var receivedChunks = new List<byte>();

        while (_socket.State == WebSocketState.Open)
        {
            try
            {
                var result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                
                receivedChunks.AddRange(buffer.Take(result.Count));
                
                if (result.EndOfMessage)
                {
                    // Собираем полное сообщение
                    var messageBytes = receivedChunks.ToArray();
                    receivedChunks.Clear(); // Очищаем для следующего сообщения

                    var message = Encoding.UTF8.GetString(messageBytes);
                    Debug.WriteLine($"Полное сообщение: {message}");

                    try
                    {
                        var request = JsonSerializer.Deserialize<WSRequestModel>(message);
                        if (request != null && _messageHandlers.TryGetValue(request.Type, out var handler))
                        {
                            handler(request.Element);
                        }
                        else
                        {
                            Console.WriteLine($"Неизвестный тип сообщения: {request?.Type}");
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        Debug.WriteLine($"Ошибка десериализации JSON: {jsonEx}\nСырые данные: {message}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Ошибка в ReceiveLoop: {e}");
                break;
            }
        }
    }

    public static async Task SendAsync(string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        await _socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public static async Task DisconnectAsync()
    {
        if (_socket?.State == WebSocketState.Open) await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnected", CancellationToken.None);
        
    }
    public static async Task DataInit(string initType)
    {
        WSRequestModel request = new WSRequestModel {
            Type = initType,
            Element = JsonSerializer.SerializeToElement("")
        };
        await SendAsync(JsonSerializer.Serialize(request));
    }
    public static async Task ChatInit(int chatId)
    {
        WSRequestModel request = new WSRequestModel {
            Type = "message_list",
            Element = JsonSerializer.SerializeToElement(chatId)
        };
        await SendAsync(JsonSerializer.Serialize(request));
    }
    public static async Task SendChatMessage(string message, int chatId)
    {
        WSRequestModel request = new WSRequestModel
        {
            Type = "message",
            Element = JsonSerializer.SerializeToElement(new SendChatMessageModel() {
                ChatId = chatId,
                Content = message
            })
        };
        await SendAsync(JsonSerializer.Serialize(request));
    }

    public static async Task SendEmailChange(string email)
    {
        WSRequestModel request = new WSRequestModel
        {
            Type = "change_email",
            Element = JsonSerializer.SerializeToElement(email)
        };
        await SendAsync(JsonSerializer.Serialize(request));
            
    }

    public static async Task SendPasswordChange(string password)
    {
        WSRequestModel request = new WSRequestModel
        {
            Type = "change_password",
            Element = JsonSerializer.SerializeToElement(password)
        };
        await SendAsync(JsonSerializer.Serialize(request));
    }

    public static async Task SendUsernameChange(string username)
    {
        WSRequestModel request = new WSRequestModel {
            Type = "change_password",
            Element = JsonSerializer.SerializeToElement(username)
        };
        await SendAsync(JsonSerializer.Serialize(request));
    }

    public static async Task SendMessageRead(int messageId)
    {
        WSRequestModel request = new WSRequestModel {
            Type = "message_read",
            Element = JsonSerializer.SerializeToElement(messageId)
        };
        await SendAsync(JsonSerializer.Serialize(request));
    }
    public static async Task SendCreateChat(string username)
    {
        WSRequestModel request = new WSRequestModel {
            Type = "chat_create",
            Element = JsonSerializer.SerializeToElement(username)
        };
        await SendAsync(JsonSerializer.Serialize(request));
    }

    public static async Task SendChatMemberInit(int chatId)
    {
        WSRequestModel request = new WSRequestModel {
            Type = "chat_members",
            Element = JsonSerializer.SerializeToElement(chatId)
        };
        await SendAsync(JsonSerializer.Serialize(request));
    }

    public static async Task SendChatMemberAdd(int chatId, string username)
    {
        WSRequestModel request = new WSRequestModel {
            Type = "chat_member_add",
            Element = JsonSerializer.SerializeToElement(new ChatMemberAddModel(username, chatId))
        };
        await SendAsync(JsonSerializer.Serialize(request));
    }
    public static async Task SendChatNameChange(int chatId, string newChatName)
    {
        WSRequestModel request = new WSRequestModel {
            Type = "chat_name_change",
            Element = JsonSerializer.SerializeToElement(new ChatChangeModel {
                ChatId = chatId,
                Name = newChatName
            })
        };
        await SendAsync(JsonSerializer.Serialize(request));
    }
}