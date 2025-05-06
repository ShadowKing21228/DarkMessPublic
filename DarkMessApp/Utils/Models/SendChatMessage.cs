namespace DarkMessApp.Utils.Models;

public class SendChatMessageModel
{
    public int SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int ChatId { get; set; }
}