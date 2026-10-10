using RiderProfit.Models;

namespace RiderProfit.Services;

// Manages a rider's expenses. Every method teakes the userId so riders only see their own data.
public interface IExpenseService
{
    // All of this rider's expenses in a data rnage (both ends optional), newest first
    Task<List<Expense>> GetExpensesAsync(
        string userId,
        DateTime? from = null,
        DateTime? to = null,
        ExpenseCategory? category = null
    );
    Task AddExpenseAsync(Expense expense);

    // For the dashboard's "total costs" card and category breakdown
    Task<decimal> GetTotalAsync(string userId, DateTime? from = null, DateTime? to = null);
    Task<Dictionary<ExpenseCategory, decimal>> GetTotalsByCategoryAsync(
        string userId,
        DateTime? from = null,
        DateTime? to = null
    );
    Task<bool> DeleteExpenseAsync(int id, string userId); // false if not found or not theirs

    Task<bool> UpdateExpenseAsync(Expense expense); // false if not found or not theirs
}
