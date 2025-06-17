using System;
using System.Collections.Generic;
using PersonalPortofolioFinanceApp.Classes;
using PersonalPortfolioFinanceApp.Helpers;

namespace PersonalPortfolioFinanceApp.Services
{
    public class ExpenseService
    {
        public void HandlePeriodicExpenses()
        {
            List<PeriodicExpense> expenses = new List<PeriodicExpense>();

            ConsoleHelper.PrintHeader("Periodic Expenses");
            string addMore;

            do
            {
                string name = ConsoleHelper.Prompt("Enter expense name (or press Enter to skip): ");
                if (string.IsNullOrWhiteSpace(name)) break;

                double amount = double.Parse(ConsoleHelper.Prompt($"Enter the total amount for {name}: $"));
                int months = int.Parse(ConsoleHelper.Prompt("Occurs every how many months?: "));

                expenses.Add(new PeriodicExpense
                {
                    Name = name,
                    Amount = amount,
                    EveryXMonths = months
                });

                addMore = ConsoleHelper.Prompt("Add another? (y/n): ").ToLower();

            } while (addMore == "y");

            ConsoleHelper.PrintSuccess("Periodic expenses recorded (not saved to DB).");
        }
    }
}
