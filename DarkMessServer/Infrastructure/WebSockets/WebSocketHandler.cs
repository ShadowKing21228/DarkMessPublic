using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using DarkMessServer.API.Models;
using DarkMessServer.Core.Security;
using DarkMessServer.Infrastructure.Data;

namespace DarkMessServer.Infrastructure.WebSockets;

public static class WebSocketHandler
{
    private static readonly ConcurrentDictionary<string, WebSocket> _connectedUsers = new();
    
    public static async Task HandleWebSocketAsync(HttpContext context)
    {
        if (!ConfirmJwtFromWs(context)) {
            Console.WriteLine("Вводимый запрос без Authorization");
            context.Response.StatusCode = 401;
            return;
        }
        var token = context.Request.Headers["Authorization"].ToString().Substring("Bearer ".Length);
        ClaimsPrincipal user;
        try {
            var handler = new JwtSecurityTokenHandler();
            user = handler.ValidateToken(token, JwtHandler.JwtParameters, out _);
        }
        catch {
            Console.WriteLine("Токен неправильный");
            context.Response.StatusCode = 401;
            return;
        }
        var socket = await context.WebSockets.AcceptWebSocketAsync();
        _connectedUsers.AddOrUpdate(
            user.Identity.Name, 
            socket, 
            (_, oldSocket) => {
                try { oldSocket?.Dispose(); } catch {}
                return socket;
            });
        try {
            await ReceiveLoopAsync(socket, user);
        }
        catch (WebSocketException ex) {
            Console.WriteLine($"WebSocket error: {ex.WebSocketErrorCode}");
        }
        finally {
            _connectedUsers.TryRemove(user.Identity.Name, out var _);
            try { socket?.Dispose(); } catch {}
        }
    }
    private static async Task ReceiveLoopAsync(WebSocket socket, ClaimsPrincipal user)
{
    var userId = user.FindFirst(ClaimTypes.Name)?.Value;
    if (userId == null) return;

    var buffer = new byte[1024 * 4];
    WebSocketReceiveResult result;

    try
    {
        while (socket.State == WebSocketState.Open)
        {
            try {
                result = await socket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    CancellationToken.None);
                
                if (result.MessageType == WebSocketMessageType.Close) {
                    await TryCloseSocket(socket, WebSocketCloseStatus.NormalClosure);
                    break;
                }
                
                var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var message = JsonSerializer.Deserialize<RequestModel>(msg);
                
                if (message == null) {
                    Console.WriteLine($"Invalid message format from user {userId}");
                    continue;
                }
                _ = Task.Run(async () => 
                {
                    try {
                        await JsonRequestHandler(message, userId);
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"Error processing message from {userId}: {ex.Message}");
                    }
                });
            }
            catch (WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely) {
                Console.WriteLine($"Connection lost for user {userId}");
                break;
            }
            catch (JsonException ex) {
                Console.WriteLine($"JSON error from user {userId}: {ex.Message}");
                await SendErrorMessage(socket, "Invalid message format");
            }
            catch (Exception ex) {
                Console.WriteLine($"General error for user {userId}: {ex.Message}");
                break;
            }
        }
    }
    finally {
        await CleanupConnection(userId, socket);
    }
}

private static async Task TryCloseSocket(WebSocket socket, WebSocketCloseStatus status)
{
    try
    {
        if (socket?.State == WebSocketState.Open)
        {
            await socket.CloseAsync(
                status,
                "Closed by server",
                CancellationToken.None);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error closing socket: {ex.Message}");
    }
}

    private static async Task CleanupConnection(string userId, WebSocket socket)
    {
        _connectedUsers.TryRemove(userId, out _);
        Console.WriteLine($"User {userId} disconnected");
    
        try
        {
            if (socket?.State == WebSocketState.Open)
            {
                await socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Connection ended",
                    CancellationToken.None);
            }
            socket?.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Cleanup error: {ex.Message}");
        }
    }

    private static async Task SendErrorMessage(WebSocket socket, string message)
    {
        try
        {
            if (socket?.State == WebSocketState.Open)
            {
                var error = new { Type = "error", Message = message };
                var json = JsonSerializer.Serialize(error);
                var bytes = Encoding.UTF8.GetBytes(json);
                
                await socket.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send error: {ex.Message}");
        }
    }
    private static bool ConfirmJwtFromWs(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ")) {
                context.Response.StatusCode = 401;
            }
            else {
                return true;
            }
        }
        return false;
    }

    private static async Task JsonRequestHandler(RequestModel message, string userId)
    {
        switch (message.Type)
        {
            case "user_profile":
                await RequestHandler.UserProfileHandler(await UserRepository.GetUserProfile(int.Parse(userId)), userId);
                break;
            case "message":
                var mess = message.Element.Deserialize<SendMessageModel>();
                if (mess != null) await RequestHandler.MessageHandler(mess, userId);
                break;
            case "chat_create":
                Console.WriteLine(message.Element.GetRawText());
                var usernameWith = message.Element.Deserialize<string>();
                Console.WriteLine(usernameWith);
                await RequestHandler.ChatAddHandler(usernameWith, userId);
                break;
            case "chat_list":
                var chats = await ChatRepository.GetChatsWithLastMessages(userId);
                await RequestHandler.ChatListHandler(chats, userId);
                break;
            case "message_list":
                var chatId = message.Element.Deserialize<int>();
                Console.WriteLine("Запрашиваемый чат: " + chatId);
                var messages = await MessagesRepository.GetMessagesForChat(chatId);
                await RequestHandler.MessageListHandler(messages, userId);
                break;
            case "message_read":
                var messageId = message.Element.Deserialize<int>();
                var status = await MessageReadStatusRepository.AddReadStatus(messageId, int.Parse(userId));
                await ReflectUpdate(new RequestModel {
                    Type = "message_read", Element = JsonSerializer.SerializeToElement(message)
                }, status.ChatId);
                break;
            case "change_password":
                var password = message.Element.Deserialize<string>();
                await UserRepository.UpdatePassword(int.Parse(userId), password);
                break;
            case "change_username":
                var username = message.Element.Deserialize<string>();
                await UserRepository.UpdateUsername(int.Parse(userId), username);
                break;
            case "change_email":
                var email = message.Element.Deserialize<string>();
                await UserRepository.UpdateEmail(int.Parse(userId), email);
                break;
            case "chat_members":
                var chatMemberId = message.Element.Deserialize<int>();
                var chatMemberList = await ChatMemberRepository.GetChatMembersWithDetails(chatMemberId);
                await RequestHandler.ChatMemberListHandler(chatMemberList, userId);
                break;
            case "chat_member_add":
                var addModel = message.Element.Deserialize<ChatMemberAddModel>();
                if (addModel != null) {
                    await RequestHandler.HandleChatMemberAdd(addModel, userId);
                }
                break;
            case "chat_name_change":
                var changeModel = message.Element.Deserialize<ChatChangeModel>();
                if (changeModel != null) {
                    await RequestHandler.HandleChatNameChange(changeModel, userId);
                }
                break;
            default:
                Console.WriteLine($"Неизвестный тип сообщения: {message.Type}");
                break;
        }
    }

    public static async Task SendJsonMessage(string userId, RequestModel message)
    {
        if (_connectedUsers.TryGetValue(userId, out var socket) && socket.State == WebSocketState.Open)
        {
            var json = JsonSerializer.Serialize(message);
            var buffer = Encoding.UTF8.GetBytes(json);
            var segment = new ArraySegment<byte>(buffer); 
            Console.WriteLine("Посылаем ответ к " + userId);
            await socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
    public static async Task ReflectUpdate(RequestModel message, int chatId) {
        var memberList = await ChatMemberRepository.GetChatMembers(chatId);
        foreach (var i in memberList) {
            if (_connectedUsers.ContainsKey(i.ToString())) {
                Console.WriteLine("Отражение пользователю: " + i);
                await SendJsonMessage(i.ToString(), message);
            }
        }
    }
}
