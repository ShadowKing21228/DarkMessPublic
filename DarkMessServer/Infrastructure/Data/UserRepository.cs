using System.Text;
using DarkMessServer.API.Models;
using DarkMessServer.Core.Security;
using Microsoft.AspNetCore.Identity;
using Npgsql;

namespace DarkMessServer.Infrastructure.Data;

public static class UserRepository
{
    public static async Task<bool> ValidateCredentialsAsync(string email, string password)
    {
        await using var connection = DbConnectionFactory.CreateConnection();
        await connection.OpenAsync();

        var selectCommand = new NpgsqlCommand("SELECT password_hash FROM user_account WHERE email = @email", connection);
        selectCommand.Parameters.AddWithValue("email", email);
        var result = await selectCommand.ExecuteScalarAsync();
        
        if (result == null) return false;
        var hashPassword = result.ToString();
        return PasswordUtils.VerifyPassword(password, hashPassword);
    }

    public static async Task CreateNewUser(string username, string password, string email)
    {
        await using var connection = DbConnectionFactory.CreateConnection();
        await connection.OpenAsync();
        
        var selectCommand = new NpgsqlCommand("INSERT INTO user_account(username, password_hash, email) VALUES (@username, @password, @email)", connection);
        selectCommand.Parameters.AddWithValue("username", username);
        selectCommand.Parameters.AddWithValue("password", PasswordUtils.HashPassword(password));
        selectCommand.Parameters.AddWithValue("email", email);
        await selectCommand.ExecuteNonQueryAsync();
    }
    
    public static async Task UpdateEmail(int userId, string newEmail)
    {
        await using var connection = DbConnectionFactory.CreateConnection();
        await connection.OpenAsync();

        var command = new NpgsqlCommand("UPDATE user_account SET email = @newEmail, dataversion = dataversion + 1 WHERE id = @userId", connection);
        command.Parameters.AddWithValue("userId", userId);
        command.Parameters.AddWithValue("newEmail", newEmail);

        await command.ExecuteNonQueryAsync();
    }

    public static async Task UpdatePassword(int userId, string newPassword)
    {
        await using var connection = DbConnectionFactory.CreateConnection();
        await connection.OpenAsync();

        var hashedPassword = PasswordUtils.HashPassword(newPassword);

        var command = new NpgsqlCommand("UPDATE user_account SET password_hash = @newPassword, dataversion = dataversion + 1 WHERE id = @userId", connection);
        command.Parameters.AddWithValue("userId", userId);
        command.Parameters.AddWithValue("newPassword", hashedPassword);

        await command.ExecuteNonQueryAsync();
    }

    public static async Task UpdateUsername(int userId, string newUsername)
    {
        await using var connection = DbConnectionFactory.CreateConnection();
        await connection.OpenAsync();

        var command = new NpgsqlCommand("UPDATE user_account SET username = @newUsername, dataversion = dataversion + 1 WHERE id = @userId", connection);
        command.Parameters.AddWithValue("userId", userId);
        command.Parameters.AddWithValue("newUsername", newUsername);

        await command.ExecuteNonQueryAsync();
    }

    public static async Task<string> GetId(UserLoginModel model)
    {
        await using var connection = DbConnectionFactory.CreateConnection();
        await connection.OpenAsync();
        
        var selectCommand = new NpgsqlCommand("SELECT id FROM user_account WHERE email = @email", connection);
        selectCommand.Parameters.AddWithValue("email", model.Email);
        
        var result = await selectCommand.ExecuteScalarAsync();
        return result.ToString();
    }

    public static async Task<UserProfileModel> GetUserProfile(int id)
    {
        await using var connection = DbConnectionFactory.CreateConnection();
        await connection.OpenAsync();
        
        var selectCommand = new NpgsqlCommand("SELECT * FROM user_account WHERE id = @id", connection);
        selectCommand.Parameters.AddWithValue("id", id);
        
        var reader = await selectCommand.ExecuteReaderAsync();

        var result = new UserProfileModel();
        while (await reader.ReadAsync()) {
            result = new UserProfileModel {
                UserId = reader.GetInt32(0),
                Username = reader.GetString(1),
                Email = reader.GetString(4),
                CreatedAt = reader.GetDateTime(3),
                Password = string.Empty
            };
        }
        return result;
    }
    public static async Task<int> GetIdByUsername(string username) {
        await using var connection = DbConnectionFactory.CreateConnection();
        await connection.OpenAsync();
        
        var selectCommand = new NpgsqlCommand("SELECT id FROM user_account WHERE username = @username", connection);
        selectCommand.Parameters.AddWithValue("email", username);
        
        var result = await selectCommand.ExecuteReaderAsync();
        return result.GetInt32(0);
    }
}