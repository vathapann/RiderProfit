namespace RiderProfit.Models;
public class Expense {
    public int Id {set; get;}
    public int UserId {set; get;}
    public string Category {set; get;} = string.Empty;
    public decimal Amount {set; get;} = 0;
    public string Date {set; get;} = string.Empty;
    public string description {set; get;} = string.Empty;

    // link back to shift model, if this expense is associated with a shift
    public Shift? Shift {set; get;} = null;
}