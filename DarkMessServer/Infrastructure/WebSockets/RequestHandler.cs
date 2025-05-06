using System.Diagnostics;
using System.Text.Json;
using DarkMessServer.API.Models;
using DarkMessServer.Infrastructure.Data;
using Npgsql;

namespace DarkMessServer.Infrastructure.WebSockets;

public static class RequestHandler
{
    public static async Task MessageHandler(SendMessageModel sendMessage, string senderId) {
        Console.WriteLine("Обработка сообщения message от " + senderId);
        var message = await MessagesRepository.AddMessageAndGetAsync(sendMessage.ChatId, int.Parse(senderId), sendMessage.Content);
        await WebSocketHandler.ReflectUpdate(new RequestModel {
            Type = "message", Element = JsonSerializer.SerializeToElement(message)
        }, message.ChatId);
    }

    public static async Task ChatAddHandler(string username, string senderId) {
        var chat = await ChatRepository.CreateChatWithUsers(int.Parse(senderId), username);
        await WebSocketHandler.ReflectUpdate(new RequestModel {
            Type = "chat_create",
            Element = JsonSerializer.SerializeToElement(chat)
        }, chat.ChatId);
    }

    public static async Task ChatListHandler(List<ChatLastMessageModel> chats, string senderId)
    {
        var response = new RequestModel {
            Type = "chat_list",
            Element = JsonSerializer.SerializeToElement(chats)
        };
        await WebSocketHandler.SendJsonMessage(senderId, response);
    }

    public static async Task MessageListHandler(List<MessageModel> chats, string senderId)
    {
        var response = new RequestModel {
            Type = "message_list",
            Element = JsonSerializer.SerializeToElement(chats)
        };
        await WebSocketHandler.SendJsonMessage(senderId, response);
    }

    public static async Task UserProfileHandler(UserProfileModel user, string senderId)
    {
        var response = new RequestModel {
            Type = "user_profile", Element = JsonSerializer.SerializeToElement(user)
        };
        Console.WriteLine("Данные пользователя: " + response.Element.GetRawText());
        await WebSocketHandler.SendJsonMessage(senderId, response);
    }

    public static async Task MessageReadStatus(MessageStatusModel status, string senderId)
    {
        var response = new RequestModel {
            Type = "message_read", Element = JsonSerializer.SerializeToElement(status)
        };
        await WebSocketHandler.SendJsonMessage(senderId, response);
    }
    
    public static async Task ChatMemberListHandler(List<ChatMemberModel> memberModel, string senderId)
    {
        var response = new RequestModel {
            Type = "chat_members", Element = JsonSerializer.SerializeToElement(memberModel)
        };
        await WebSocketHandler.SendJsonMessage(senderId, response);
    }

    public static async Task ChatMemberListAddHandler(ChatMemberModel memberModel, string senderId) {
        var response = new RequestModel {
            Type = "chat_member_add", Element = JsonSerializer.SerializeToElement(memberModel)
        };
        await WebSocketHandler.SendJsonMessage(senderId, response);
    }
    public static async Task HandleChatMemberAdd(ChatMemberAddModel model, string requesterId)
    {
        try {
            
            var userToAdd = await UserRepository.GetIdByUsername(model.Username);
            
            await ChatMemberRepository.AddMemberToChat(model.ChatId, userToAdd);
            
            var update = new RequestModel {
                Type = "chat_member_added", Element = JsonSerializer.SerializeToElement(new
                {
                    ChatId = model.ChatId, UserId = userToAdd, Username = model.Username, AddedBy = requesterId
                })
            };
            await WebSocketHandler.ReflectUpdate(update, model.ChatId);
        }
        catch (Exception ex) {
            Console.WriteLine($"Error adding chat member: {ex}");
        }
    }

    public static async Task HandleChatNameChange(ChatChangeModel model, string requesterId)
    {
        try {
            await ChatRepository.UpdateChatName(model.ChatId, model.Name);
            
            var update = new RequestModel {
                Type = "chat_name_changed", Element = JsonSerializer.SerializeToElement(new {
                    ChatId = model.ChatId, NewName = model.Name, ChangedBy = requesterId
                })
            };
            await WebSocketHandler.ReflectUpdate(update, model.ChatId);
            
        }
        catch (Exception ex) {
            Console.WriteLine($"Error changing chat name: {ex}");
        }
    }
}