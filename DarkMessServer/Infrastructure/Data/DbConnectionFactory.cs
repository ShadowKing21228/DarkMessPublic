using Npgsql;

namespace DarkMessServer.Infrastructure.Data;

public static class DbConnectionFactory
{
    private const string _ipAdress = "localhost";
    private const string _port = "5432";
    private const string _dbName = ""; //your db name
    private const string _username = ""; //your user data
    private const string _password = ""; //your user pass
    
    private const string _connectionString = $"Host={_ipAdress};Port={_port};Database={_dbName};Username={_username};Password={_password}";

    public static NpgsqlConnection CreateConnection() {
        return new NpgsqlConnection(_connectionString);
    }
}