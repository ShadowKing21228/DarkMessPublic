namespace DarkMessServer.API.Models;

public class MessageModel
{
    public int MessageId { get; set; }
    public int ChatId { get; set; }
    public int SenderId { get; set; }
    public string SenderUsername { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
}