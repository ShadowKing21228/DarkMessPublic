using DarkMessServer.API.Models;
using Npgsql;

namespace DarkMessServer.Infrastructure.Data;

public static class MessagesRepository
{
    public static async Task CreateMessage(int chatId, string senderId, string content)
    {
        await using var connection = DbConnectionFactory.CreateConnection();
        await connection.OpenAsync();

        var command = new NpgsqlCommand("INSERT INTO message(chat_id, sender_id, content) VALUES (@chatId, @senderId, @content)", connection);
        command.Parameters.AddWithValue("chatId", chatId);
        command.Parameters.AddWithValue("senderId", senderId);
        command.Parameters.AddWithValue("content", content);

        await command.ExecuteNonQueryAsync();
    }

    public static async Task DeleteMessage(int messageId)
    {
        await using var connection = DbConnectionFactory.CreateConnection();
        await connection.OpenAsync();

        var command = new NpgsqlCommand("DELETE FROM message WHERE id = @messageId", connection);
        command.Parameters.AddWithValue("messageId", messageId);

        await command.ExecuteNonQueryAsync();
    }
    public static async Task<MessageModel?> AddMessageAndGetAsync(int chatId, int senderId, string content)
    {
        await using var connection = DbConnectionFactory.CreateConnection();
        await connection.OpenAsync();
    
        var query = @"
            INSERT INTO message (chat_id, sender_id, content)
            VALUES (@chatId, @senderId, @content)
            RETURNING id, chat_id, sender_id, content, sent_at;
        ";
    
        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("chatId", chatId);
        command.Parameters.AddWithValue("senderId", senderId);
        command.Parameters.AddWithValue("content", content);
    
        await using var reader = await command.ExecuteReaderAsync();
    
        if (!await reader.ReadAsync())
            return null;
    
        var messageId = reader.GetInt32(0);
        var returnedChatId = reader.GetInt32(1);
        var returnedSenderId = reader.GetInt32(2);
        var returnedContent = reader.GetString(3);
        var sentAt = reader.GetDateTime(4);
    
        await reader.CloseAsync();
        
        var detailsQuery = @"
            SELECT 
                u.username,
                CASE
                    WHEN EXISTS (
                        SELECT 1
                        FROM message_read_status
                        WHERE message_id = @messageId
                    )
                    THEN TRUE
                    ELSE FALSE
                END AS is_read
            FROM user_account u
            WHERE u.id = @senderId
        ";
    
        await using var detailsCommand = new NpgsqlCommand(detailsQuery, connection);
        detailsCommand.Parameters.AddWithValue("messageId", messageId);
        detailsCommand.Parameters.AddWithValue("senderId", senderId);
    
        await using var detailsReader = await detailsCommand.ExecuteReaderAsync();
    
        string senderUsername = string.Empty;
        bool isRead = false;
    
        if (await detailsReader.ReadAsync()) {
            senderUsername = detailsReader.GetString(0);
            isRead = detailsReader.GetBoolean(1);
        }
    
        return new MessageModel {
            MessageId = messageId,
            ChatId = returnedChatId,
            SenderId = returnedSenderId,
            SenderUsername = senderUsername,
            Content = returnedContent,
            SentAt = sentAt,
            IsRead = isRead
        };
    }

    public static async Task<List<MessageModel>> GetMessagesForChat(int chatId)
    {
        var messages = new List<MessageModel>();

        await using var connection = DbConnectionFactory.CreateConnection();
        await connection.OpenAsync();

        var command = new NpgsqlCommand(@"
        SELECT
            m.id AS message_id,
            m.sender_id,
            u.username AS sender_username,
            m.content,
            m.sent_at,
            EXISTS (
                SELECT 1
                FROM message_read_status rs
                WHERE rs.message_id = m.id
            ) AS is_read
        FROM message m
        JOIN user_account u ON m.sender_id = u.id
        WHERE m.chat_id = @chatId
        ORDER BY m.sent_at ASC;", connection);

        command.Parameters.AddWithValue("chatId", chatId);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var message = new MessageModel
            {
                MessageId = reader.GetInt32(reader.GetOrdinal("message_id")),
                ChatId = chatId,
                SenderId = reader.GetInt32(reader.GetOrdinal("sender_id")),
                SenderUsername = reader.GetString(reader.GetOrdinal("sender_username")),
                Content = reader.GetString(reader.GetOrdinal("content")),
                SentAt = reader.GetDateTime(reader.GetOrdinal("sent_at")),
                IsRead = reader.GetBoolean(reader.GetOrdinal("is_read"))
            };
            Console.WriteLine(message.SenderUsername + " " + message.Content);
            messages.Add(message);
        }

        return messages;
    }
}
