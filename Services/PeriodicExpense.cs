public class PeriodicExpense
{
    public string Name { get; set; }
    public double Amount { get; set; }
    public int EveryXMonths { get; set; }
    public double MonthlyEquivalent => Amount / EveryXMonths;
}

