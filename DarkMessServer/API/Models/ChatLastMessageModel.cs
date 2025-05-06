namespace DarkMessServer.API.Models;

public class ChatLastMessageModel
{
    public int ChatId { get; set; }
    public string? Name { get; set; }
    public bool IsGroup { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? LastMessage { get; set; }
    public DateTime? LastMessageTime { get; set; }
    public string? LastMessageSenderName { get; set; }
}