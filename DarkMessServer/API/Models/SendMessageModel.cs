namespace DarkMessServer.API.Models;

public class SendMessageModel
{
    public int SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int ChatId { get; set; }
}