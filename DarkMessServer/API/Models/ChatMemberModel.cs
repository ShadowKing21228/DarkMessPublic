namespace DarkMessServer.API.Models;

public record ChatMemberModel(int UserId, string Username, DateTime JoinedAt);