using Microsoft.EntityFrameworkCore;
using RiderProfit.Data;
using RiderProfit.Models;

namespace RiderProfit.Services;

// Reads and writes a rider's expenses. Every method checks the userId
// so a rider won't see or change another rider's data.
public class ExpenseService(IDbContextFactory<ApplicationDbContext> dbFactory) : IExpenseService
{
    // Returns the rider's expenses, latest first. The date range and category are optional;
    // leavning them out means "no limit". Includes the linked shift (null if there is none).
    public async Task<List<Expense>> GetExpensesAsync(
        string userId,
        DateTime? from = null,
        DateTime? to = null,
        ExpenseCategory? category = null
    )
    {
        // a short-lived context per call: Blazor server compoentns live a long time.
        await using var db = await dbFactory.CreateDbContextAsync();
        var query = ApplyOptionalFilters(
            db.Expenses.Where(e => e.UserId == userId),
            from,
            to,
            category
        ); //LINQ + lambda expression

        return await query
            .Include(e => e.Shift) // load linked shift (null if there is none)
            .OrderByDescending(e => e.Date)
            .ToListAsync();
    }

    // Adds and saves a new expense
    public async Task AddExpenseAsync(Expense expense)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.Expenses.Add(expense); // only marks it as new; not written yet
        await db.SaveChangesAsync(); // sends the INSERT to the database
    }

    // Total of the rider's expenses in the date range, or all of them if no range.
    public async Task<decimal> GetTotalAsync(
        string userId,
        DateTime? from = null,
        DateTime? to = null
    )
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var amounts = await ApplyOptionalFilters(
                db.Expenses.Where(e => e.UserId == userId),
                from,
                to,
                null
            ) //LINQ + lambda expression
            .Select(e => e.Amount) // only the amounts are needed
            .ToListAsync();
        // summed in memory as SQLite cannot SUM decimal columns in SQL
        return amounts.Sum();
    }

    //  Total per category. Used fo the dashboard's category chart.
    public async Task<Dictionary<ExpenseCategory, decimal>> GetTotalsByCategoryAsync(
        string userId,
        DateTime? from = null,
        DateTime? to = null
    )
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var rows = await ApplyOptionalFilters(
                db.Expenses.Where(e => e.UserId == userId),
                from,
                to,
                null
            )
            .Select(e => new { e.Category, e.Amount }) // anonymous type
            .ToListAsync();
        // group and summed in memory (SQLite decimal limit)
        return rows.GroupBy(r => r.Category).ToDictionary(g => g.Key, g => g.Sum(r => r.Amount));
    }

    // Deletes one expense. Return false if not found or belongs to another rider
    public async Task<bool> DeleteExpenseAsync(int id, string userId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        // only find the expense if it belongs to this rider
        var expense = await db.Expenses.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        if (expense == null)
        {
            return false;
        }
        db.Expenses.Remove(expense);
        await db.SaveChangesAsync();
        return true;
    }

    // Update an existing expense. Return false if not found or belongs to another rider.
    public async Task<bool> UpdateExpenseAsync(Expense expense)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var foundExpense = await db.Expenses.FirstOrDefaultAsync(e =>
            e.Id == expense.Id && e.UserId == expense.UserId
        );
        if (foundExpense == null)
        {
            return false;
        }
        // fields that the rider can edit
        foundExpense.Category = expense.Category;
        foundExpense.Amount = expense.Amount;
        foundExpense.Date = expense.Date;
        foundExpense.Description = expense.Description;
        foundExpense.ShiftId = expense.ShiftId;

        await db.SaveChangesAsync();
        return true;
    }

    // helper function for optional filters to a query; used by list and total methods
    private static IQueryable<Expense> ApplyOptionalFilters(
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
            var endExcluding = to.Value.Date.AddDays(1); // include the whole 'to' day (dates are stored with a time)
            query = query.Where(e => e.Date < endExcluding);
        }
        return query;
    }
}
