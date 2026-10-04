namespace RiderProfit.Models;
public class Expense {
    public int Id {set; get;}
    public string UserId {set; get;} = string.Empty;
    public ExpenseCategory Category {set; get;} = ExpenseCategory.Other;
    public decimal Amount {set; get;} = 0;
    public DateTime Date {set; get;} = DateTime.MinValue;
    public string Description {set; get;} = string.Empty;

    // link back to shift model, if this expense is associated with a shift
    public Shift? Shift {set; get;} = null;
    public int? ShiftId {set; get;} = null;
}