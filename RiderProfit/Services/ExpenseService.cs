using Microsoft.EntityFrameworkCore;
using RiderProfit.Data;
using RiderProfit.Models;

namespace RiderProfit.Services;

public class ExpenseService(IDbContextFactory<ApplicationDbContext> dbFactory) : IExpenseService
{
    public async Task<List<Expense>> GetExpensesAsync(
        string userId,
        DateTime? from = null,
        DateTime? to = null,
        ExpenseCategory? category = null
    )
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var query = OptionalFilters(db.Expenses.Where(e => e.UserId == userId), from, to, category); //LINQ + lambda expression

        return await query
            .Include(e => e.Shift) // load linked shift (null if there is none)
            .OrderByDescending(e => e.Date)
            .ToListAsync();
    }

    public async Task AddExpenseAsync(Expense expense)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.Expenses.Add(expense);
        await db.SaveChangesAsync();
    }

    public async Task<decimal> GetTotalAsync(
        string userId,
        DateTime? from = null,
        DateTime? to = null
    )
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var amounts = await OptionalFilters(
                db.Expenses.Where(e => e.UserId == userId),
                from,
                to,
                null
            ) //LINQ + lambda expression
            .Select(e => e.Amount) // only the amounts are needed
            .ToListAsync();
        return amounts.Sum();
    }

    // TODO (Ellie): implement business logic: update, delete and totals by category. Placeholder for project builds; throw exception if called.
    public Task<Dictionary<ExpenseCategory, decimal>> GetTotalsByCategoryAsync(
        string userId,
        DateTime? from = null,
        DateTime? to = null
    ) => throw new NotImplementedException();

    public Task<bool> DeleteExpenseAsync(int id, string userId) =>
        throw new NotImplementedException();

    public Task<bool> UpdateExpenseAsync(Expense expense) => throw new NotImplementedException();

    //helper function for optional filters to a query; used by list and total methods
    private static IQueryable<Expense> OptionalFilters(
        IQueryable<Expense> query,
        DateTime? from,
        DateTime? to,
        ExpenseCategory? category
    )
    {
        if (category is not null)
        {
            query = query.Where(e => e.Category == category.Value);
        }
        if (from is not null)
        {
            query = query.Where(e => e.Date >= from.Value.Date);
        }
        if (to is not null)
        {
            var endExcluding = to.Value.Date.AddDays(1); // include the whole 'to' day
            query = query.Where(e => e.Date < endExcluding);
        }
        return query;
    }
}
