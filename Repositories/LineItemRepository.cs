namespace WillysReceiptParser.Repositories;
using Dapper;
using WillysReceiptParser;
using WillysReceiptParser.Helpers;

public interface ILineItemRepository
{
    Task<IEnumerable<LineItem>> GetAll();
    Task<LineItem> GetById(int id);
    Task Create(LineItem LineItem);
    Task Update(LineItem LineItem);
    Task Delete(int id);
}

public class LineItemRepository : ILineItemRepository
{
    private readonly DataContext _context;

    public LineItemRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LineItem>> GetAll()
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            SELECT * FROM LineItems
        ";
        return await connection.QueryAsync<LineItem>(sql);
    }

    public async Task<LineItem> GetById(int id)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            SELECT * FROM LineItems 
            WHERE Id = @id
        ";
        return await connection.QuerySingleOrDefaultAsync<LineItem>(sql, new { id });
    }

    public async Task Create(LineItem LineItem)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            INSERT INTO LineItems (name, quantity, unitprice, totalprice, receiptId)
            VALUES (@Name, @Quantity, @UnitPrice, @TotalPrice, @ReceiptId)
        ";
        await connection.ExecuteAsync(sql, LineItem);
    }

    public async Task Update(LineItem LineItem)
    {
        throw new NotImplementedException();
        using var connection = _context.CreateConnection();
        const string sql = @"
            UPDATE LineItems 
            SET Title = @Title,
                FirstName = @FirstName,
                LastName = @LastName, 
                Email = @Email, 
                Role = @Role, 
                PasswordHash = @PasswordHash
            WHERE Id = @Id
        ";
        await connection.ExecuteAsync(sql, LineItem);
    }

    public async Task Delete(int id)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            DELETE FROM LineItems 
            WHERE Id = @id
        ";
        await connection.ExecuteAsync(sql, new { id });
    }
}