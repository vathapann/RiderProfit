
public interface IShiftPredictor
{
    // Predict earnings every trip per hour (not including idle time) 
    float Predict(DayOfWeek day, int hour, string suburb);
    // Existing restaurant suburb in training data for dropdown selection
    IReadOnlyList<string> KnownSuburbs {get;}
}