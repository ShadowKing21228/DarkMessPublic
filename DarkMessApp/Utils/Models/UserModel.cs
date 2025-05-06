namespace DarkMessApp.Utils.Models;

public class UserModel(string username, string email, string password)
{
    public string username { get; set; } = username;
    public string password { get; set; } = password;
    public string email { get; set; } = email;
}