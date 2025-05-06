using DarkMessServer.API.Models;
using Npgsql;

namespace DarkMessServer.Infrastructure.Data;

public static class ChatRepository
{
    public static async Task CreateChat(string name, bool isGroup)
    {
        await using var connection = DbConnectionFactory.CreateConnection();
        await connection.OpenAsync();

        var command = new NpgsqlCommand("INSERT INTO chat(name, is_group) VALUES (@name, @isGroup)", connection);
        command.Parameters.AddWithValue("name", name);
        command.Parameters.AddWithValue("isGroup", isGroup);

        await command.ExecuteNonQueryAsync();
    }

    public static async Task DeleteChat(int chatId) {
        await using var connection = DbConnectionFactory.CreateConnection();
        await connection.OpenAsync();

        var command = new NpgsqlCommand("DELETE FROM chat WHERE id = @chatId", connection);
        command.Parameters.AddWithValue("chatId", chatId);

        await command.ExecuteNonQueryAsync();
    }
    public static async Task UpdateChatName(int chatId, string name) {
        await using var connection = DbConnectionFactory.CreateConnection();
        await connection.OpenAsync();

        var command = new NpgsqlCommand("UPDATE chat SET name = @newName WHERE id = @chatId", connection);
        command.Parameters.AddWithValue("chatId", chatId);
        command.Parameters.AddWithValue("newName", name);

        await command.ExecuteNonQueryAsync();
    }
    public static async Task<ChatLastMessageModel> CreateChatWithUsers(int userId, string username) 
{
    await using var connection = DbConnectionFactory.CreateConnection();
    await connection.OpenAsync();
    
    string username2;
    await using (var selectCommand = new NpgsqlCommand(
        "SELECT username FROM user_account WHERE id = @userId", connection))
    {
        selectCommand.Parameters.AddWithValue("userId", userId);
        await using var reader = await selectCommand.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            throw new Exception("Пользователь не найден");
        
        username2 = reader.GetString(0);
    }
    
    int userId2;
    await using (var selectCommand2 = new NpgsqlCommand(
        "SELECT id FROM user_account WHERE username = @username", connection))
    {
        selectCommand2.Parameters.AddWithValue("username", username);
        await using var reader = await selectCommand2.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            throw new Exception("Пользователь не найден");
        
        userId2 = reader.GetInt32(0);
    }
    
    int chatId;
    await using (var command = new NpgsqlCommand(
        "INSERT INTO chat(name, is_group) VALUES (@name, @isGroup) RETURNING id", connection))
    {
        command.Parameters.AddWithValue("name", $"{username2} & {username}");
        command.Parameters.AddWithValue("isGroup", false);
        await using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
        chatId = reader.GetInt32(0);
    }
    
    await using (var command2 = new NpgsqlCommand(
        "INSERT INTO chat_member(chat_id, user_id) VALUES (@chatId, @userId), (@chatId, @userId2)", connection))
    {
        command2.Parameters.AddWithValue("chatId", chatId);
        command2.Parameters.AddWithValue("userId", userId);
        command2.Parameters.AddWithValue("userId2", userId2);
        await command2.ExecuteNonQueryAsync();
    }
    
    return new ChatLastMessageModel 
    {
        ChatId = chatId,
        Name = $"{username2} & {username}",
        IsGroup = false
    };
}
    public static async Task<List<ChatLastMessageModel>> GetChatsWithLastMessages(string userId)
    {
        await using var connection = DbConnectionFactory.CreateConnection();
        await connection.OpenAsync();

        var command = new NpgsqlCommand(@"SELECT 
            c.id AS chat_id,
            c.name AS chat_name,
            c.is_group,
            c.created_at AS chat_created,
            last_msg.id AS message_id,
            last_msg.content AS last_message,
            last_msg.sent_at AS message_time,
            sender.username AS sender_username
        FROM chat c
        JOIN chat_member cm ON cm.chat_id = c.id
        LEFT JOIN LATERAL (
            SELECT m.id, m.content, m.sent_at, m.sender_id
            FROM message m
            WHERE m.chat_id = c.id
            ORDER BY m.sent_at DESC
            LIMIT 1
        ) last_msg ON true
        LEFT JOIN user_account sender ON sender.id = last_msg.sender_id
        WHERE cm.user_id = @userId
        ORDER BY c.id;", connection);

        command.Parameters.AddWithValue("userId", int.Parse(userId));

        var reader = await command.ExecuteReaderAsync();

        var result = new List<ChatLastMessageModel>();
        while (await reader.ReadAsync()) {
            result.Add(new ChatLastMessageModel {
                ChatId = reader.GetInt32(0),
                Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                IsGroup = reader.GetBoolean(2),
                CreatedAt = reader.GetDateTime(3),
                LastMessage = reader.IsDBNull(5) ? null : reader.GetString(5),
                LastMessageTime = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                LastMessageSenderName = reader.IsDBNull(7) ? null : reader.GetString(7)
            });
        }

        return result;
    }
}