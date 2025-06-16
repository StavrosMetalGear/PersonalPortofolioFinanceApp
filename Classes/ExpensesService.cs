using System;
using System.Collections.Generic;
using PersonalPortofolioFinanceApp.Classes;

namespace PersonalPortfolioFinanceApp.Services
{
    public class ExpenseService
    {
        public void HandlePeriodicExpenses()
        {
            List<PeriodicExpense> expenses = new List<PeriodicExpense>();
            Console.WriteLine("\n--- Periodic Expenses ---");
            string addMore;

            do
            {
                Console.Write("Enter expense name (or press Enter to skip): ");
                string name = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(name)) break;

                Console.Write($"Enter the total amount for {name}: $");
                double amount = double.Parse(Console.ReadLine());

                Console.Write("Occurs every how many months?: ");
                int months = int.Parse(Console.ReadLine());

                expenses.Add(new PeriodicExpense { Name = name, Amount = amount, EveryXMonths = months });

                Console.Write("Add another? (y/n): ");
                addMore = Console.ReadLine().ToLower();

            } while (addMore == "y");

            Console.WriteLine("✅ Periodic expenses recorded (not saved to DB).");
        }
    }
}

