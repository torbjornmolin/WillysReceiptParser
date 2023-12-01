namespace WillysReceiptParser.Repositories;

using Dapper;
using WillysReceiptParser.Helpers;

public interface ILineItemRepository
{
    Task<IEnumerable<LineItem>> GetAll();
    Task<LineItem> GetById(int id);
    Task<LineItem> GetByEmail(string email);
    Task Create(LineItem LineItem);
    Task Update(LineItem LineItem);
    Task Delete(int id);
}

public class LineItemRepository : ILineItemRepository
{
    private DataContext _context;

    public LineItemRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LineItem>> GetAll()
    {
        using var connection = _context.CreateConnection();
        var sql = @"
            SELECT * FROM LineItems
        ";
        return await connection.QueryAsync<LineItem>(sql);
    }

    public async Task<LineItem> GetById(int id)
    {
        using var connection = _context.CreateConnection();
        var sql = @"
            SELECT * FROM LineItems 
            WHERE Id = @id
        ";
        return await connection.QuerySingleOrDefaultAsync<LineItem>(sql, new { id });
    }

    public async Task<LineItem> GetByEmail(string email)
    {
        using var connection = _context.CreateConnection();
        var sql = @"
            SELECT * FROM LineItems 
            WHERE Email = @email
        ";
        return await connection.QuerySingleOrDefaultAsync<LineItem>(sql, new { email });
    }

    public async Task Create(LineItem LineItem)
    {
        using var connection = _context.CreateConnection();
        var sql = @"
            INSERT INTO LineItems (name, quantity, unitprice, totalprice)
            VALUES (@Name, @Quantity, @UnitPrice, @TotalPrice)
        ";
        await connection.ExecuteAsync(sql, LineItem);
    }

    public async Task Update(LineItem LineItem)
    {
        using var connection = _context.CreateConnection();
        var sql = @"
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
        var sql = @"
            DELETE FROM LineItems 
            WHERE Id = @id
        ";
        await connection.ExecuteAsync(sql, new { id });
    }
}