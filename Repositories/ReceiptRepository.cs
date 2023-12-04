namespace WillysReceiptParser.Repositories;

using Dapper;
using WillysReceiptParser;
using WillysReceiptParser.Helpers;

public interface IReceiptRepository
{
    Task<IEnumerable<Receipt>> GetAll();
    Task<Receipt> GetById(int id);
    Task<int> Create(Receipt Receipt);
    Task Update(Receipt Receipt);
    Task Delete(int id);
}

public class ReceiptRepository : IReceiptRepository
{
    private readonly DataContext _context;

    public ReceiptRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Receipt>> GetAll()
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            SELECT * FROM Receipts
        ";
        return await connection.QueryAsync<Receipt>(sql);
    }

    public async Task<Receipt> GetById(int id)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            SELECT * FROM Receipts 
            WHERE Id = @id
        ";
        return await connection.QuerySingleOrDefaultAsync<Receipt>(sql, new { id });
    }

    public async Task<int> Create(Receipt Receipt)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            INSERT INTO Receipts(
	        Store, Date, TotalAmount, StoreId, FileOrigin)
            VALUES (@Store, @Date, @TotalAmount, @StoreId, @FileOrigin)
            RETURNING Id
        ";
        return await connection.QuerySingleAsync<int>(sql, Receipt);
    }

    public async Task Update(Receipt Receipt)
    {
        throw new NotImplementedException();
        using var connection = _context.CreateConnection();
        const string sql = @"
            UPDATE Receipts 
            SET Title = @Title,
                FirstName = @FirstName,
                LastName = @LastName, 
                Email = @Email, 
                Role = @Role, 
                PasswordHash = @PasswordHash
            WHERE Id = @Id
        ";
        await connection.ExecuteAsync(sql, Receipt);
    }

    public async Task Delete(int id)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            DELETE FROM Receipts 
            WHERE Id = @id
        ";
        await connection.ExecuteAsync(sql, new { id });
    }
}