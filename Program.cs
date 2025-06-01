using System;
using System.Collections.Generic;

// Represents a non-monthly (periodic) expense like car insurance or annual tax
class PeriodicExpense
{
    public string Name { get; set; }
    public double Amount { get; set; } // Total amount of the expense
    public int EveryXMonths { get; set; } // How often the expense occurs (in months)

    // Convert to monthly equivalent to simplify budgeting
    public double MonthlyEquivalent => Amount / EveryXMonths;
}

// Represents a financial goal (e.g., buy a laptop, go on vacation)
class Goal
{
    public string Name { get; set; }
    public double Cost { get; set; } // Total cost of the goal
    public int MonthsToSave { get; set; } // How many months until the goal deadline

    // Calculates the required saving per month to reach the goal in time
    public double RequiredMonthlySaving => Cost / MonthsToSave;
}

class Program
{
    static void Main()
    {
        Console.WriteLine("Welcome to the Personal Finance Portfolio App!");

        // --- Step 1: Get Monthly Salary ---
        Console.Write("Enter your monthly salary: $");
        double salary = double.Parse(Console.ReadLine());

        // --- Step 2: Get Fixed Monthly Expenses (e.g., rent, groceries) ---
        Console.Write("Enter your total monthly expenses: $");
        double expenses = double.Parse(Console.ReadLine());

        // --- Step 3: Get Periodic Expenses (e.g., car insurance every 6 months) ---
        List<PeriodicExpense> periodicExpenses = new List<PeriodicExpense>();
        Console.WriteLine("\n--- Periodic Expenses ---");
        string addMorePeriodic;

        do
        {
            Console.Write("Enter expense name (or press Enter to skip): ");
            string name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name)) break;

            Console.Write($"Enter the total amount for {name}: $");
            double amount = double.Parse(Console.ReadLine());

            Console.Write($"This occurs every how many months?: ");
            int months = int.Parse(Console.ReadLine());

            periodicExpenses.Add(new PeriodicExpense
            {
                Name = name,
                Amount = amount,
                EveryXMonths = months
            });
