using DarkMessServer.API.Models;
using Npgsql;

namespace DarkMessServer.Infrastructure.Data;

public static class ChatMemberRepository
{
    public static async Task AddMemberToChat(int chatId, int userId)
    {
        using var connection = DbConnectionFactory.CreateConnection();
        await connection.OpenAsync();

        var command = new NpgsqlCommand("INSERT INTO chat_member(chat_id, user_id) VALUES (@chatId, @userId)", connection);
        command.Parameters.AddWithValue("chatId", chatId);
        command.Parameters.AddWithValue("userId", userId);

        await command.ExecuteNonQueryAsync();
    }

    public static async Task RemoveMemberFromChat(int chatId, int userId)
    {
        using var connection = DbConnectionFactory.CreateConnection();
        await connection.OpenAsync();

        var command = new NpgsqlCommand("DELETE FROM chat_member WHERE chat_id = @chatId AND user_id = @userId", connection);
        command.Parameters.AddWithValue("chatId", chatId);
        command.Parameters.AddWithValue("userId", userId);

        await command.ExecuteNonQueryAsync();
    }

    public static async Task<List<int>> GetChatMembers(int chatId)
    {
        using var connection = DbConnectionFactory.CreateConnection();
        await connection.OpenAsync();
        
        var command = new NpgsqlCommand("SELECT user_id FROM chat_member WHERE chat_id = @chatId", connection);
        command.Parameters.AddWithValue("chatId", chatId);
        
        var reader = await command.ExecuteReaderAsync();
        var result = new List<int>();
        while (await reader.ReadAsync()) {
            result.Add(reader.GetInt32(0));
        }
        return result;
    }
    public static async Task<List<ChatMemberModel>> GetChatMembersWithDetails(int chatId)
    {
        await using var connection = DbConnectionFactory.CreateConnection();
        await connection.OpenAsync();
    
        var query = @"
        SELECT 
            u.id AS user_id,
            u.username,
            cm.joined_at
        FROM chat_member cm
        JOIN user_account u ON cm.user_id = u.id
        WHERE cm.chat_id = @chatId
        ORDER BY cm.joined_at";
    
        var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("chatId", chatId);
    
        var members = new List<ChatMemberModel>();
    
        await using (var reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                members.Add(new ChatMemberModel(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetDateTime(2)));
            }
        }
    
        return members;
    }
}