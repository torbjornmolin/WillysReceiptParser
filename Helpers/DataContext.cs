namespace WillysReceiptParser.Helpers;

using System.Data;
using Dapper;
using Npgsql;
using Npgsql.Replication;

public class DataContext
{
    private DbSettings _dbSettings;

    public DataContext(DbSettings dbSettings)
    {
        _dbSettings = dbSettings;
    }

    public IDbConnection CreateConnection()
    {
        string connectionString = GetConnectionString();
        return new NpgsqlConnection(connectionString);
    }

    private string GetConnectionString()
    {
        return $"Host={_dbSettings.Server}:{_dbSettings.Port}; Database={_dbSettings.Database}; Username={_dbSettings.UserId}; Password={_dbSettings.Password};";
    }

    public async Task Init()
    {
        await InitDatabase();
        await InitTables();
    }

    private async Task InitDatabase()
    {
        // create database if it doesn't exist
        var connectionString = $"Host={_dbSettings.Server}:{_dbSettings.Port}; Database=postgres; Username={_dbSettings.UserId}; Password={_dbSettings.Password};";
        using var connection = new NpgsqlConnection(connectionString);
        var sqlDbCount = $"SELECT COUNT(*) FROM pg_database WHERE datname = '{_dbSettings.Database}';";
        var dbCount = await connection.ExecuteScalarAsync<int>(sqlDbCount);
        if (dbCount == 0)
        {
            var sql = $"CREATE DATABASE \"{_dbSettings.Database}\"";
            await connection.ExecuteAsync(sql);
        }
    }

    private async Task InitTables()
    {
        // create tables if they don't exist
        using var connection = CreateConnection();
        await _dropUsers();
        await _initLineItems();

        async Task _initLineItems()
        {
            const string sql = @"
                CREATE TABLE IF NOT EXISTS LineItems (
                    Id SERIAL PRIMARY KEY,
                    Name VARCHAR,
                    Quantity SMALLINT,
                    UnitPrice money,
                    TotalPrice money
                );
            ";
            await connection.ExecuteAsync(sql);
        }
        async Task _dropUsers()
        {
            const string sql = @"
                DROP TABLE IF EXISTS Users;
            ";
            await connection.ExecuteAsync(sql);
        }
    }
}