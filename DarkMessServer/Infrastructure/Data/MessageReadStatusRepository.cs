using DarkMessServer.API.Models;
using Npgsql;

namespace DarkMessServer.Infrastructure.Data;

public static class MessageReadStatusRepository
{
    public static async Task MarkMessageAsRead(int messageId, int userId)
    {
        using var connection = DbConnectionFactory.CreateConnection();
        await connection.OpenAsync();

        var command = new NpgsqlCommand("INSERT INTO message_read_status(message_id, user_id) VALUES (@messageId, @userId) ON CONFLICT (message_id, user_id) DO UPDATE SET read_at = CURRENT_TIMESTAMP", connection);
        command.Parameters.AddWithValue("messageId", messageId);
        command.Parameters.AddWithValue("userId", userId);

        await command.ExecuteNonQueryAsync();
    }

    public static async Task DeleteReadStatus(int messageId, int userId)
    {
        using var connection = DbConnectionFactory.CreateConnection();
        await connection.OpenAsync();

        var command = new NpgsqlCommand("DELETE FROM message_read_status WHERE message_id = @messageId AND user_id = @userId", connection);
        command.Parameters.AddWithValue("messageId", messageId);
        command.Parameters.AddWithValue("userId", userId);

        await command.ExecuteNonQueryAsync();
    }

    public static async Task<MessageStatusModel> AddReadStatus(int messageId, int userId) {
        var chatId = 0;
        await using var connection = DbConnectionFactory.CreateConnection();
        await connection.OpenAsync();
        
        var command = new NpgsqlCommand("INSERT INTO message_read_status(message_id, user_id) VALUES (@messageId, @userId) ON CONFLICT DO NOTHING", connection);
        command.Parameters.AddWithValue("messageId", messageId);
        command.Parameters.AddWithValue("userId", userId);

        await command.ExecuteNonQueryAsync();
        var selectCommand = new NpgsqlCommand("SELECT chat_id FROM message WHERE id = @messageId", connection);
        selectCommand.Parameters.AddWithValue("messageId", messageId);
        
        await using var reader = await selectCommand.ExecuteReaderAsync();
        if (await reader.ReadAsync()) {
            chatId = reader.GetInt32(0);
        }

        return new MessageStatusModel(chatId, messageId, userId);
    }
}
