using System;
using System.Collections.Generic;
using System.Data.SqlClient;

class PeriodicExpense
{
    public string Name { get; set; }
    public double Amount { get; set; }
    public int EveryXMonths { get; set; }
    public double MonthlyEquivalent => Amount / EveryXMonths;
}

class Goal
{
    public string Name { get; set; }
    public double Cost { get; set; }
    public int MonthsToSave { get; set; }
    public double RequiredMonthlySaving => Cost / MonthsToSave;
}

class Program
{
    static void Main()
    {
        string connectionString = "Server=LAPTOP-S064M7H1;Database=FinanceAppDB;Trusted_Connection=True;";

        Console.WriteLine("Welcome to the Personal Finance Portfolio App!");

        // === Get username ===
        Console.Write("Enter your username: ");
        string username = Console.ReadLine();

        decimal salary = 0;
        decimal expenses = 0;
        bool userExists = false;

        // === Step 5: Try to fetch existing user info from DB ===
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();

            string selectQuery = "SELECT MonthlySalary, MonthlyExpenses FROM Users WHERE Username = @Username";

            using (SqlCommand cmd = new SqlCommand(selectQuery, conn))
            {
                cmd.Parameters.AddWithValue("@Username", username);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        userExists = true;
                        salary = reader.GetDecimal(0);
                        expenses = reader.GetDecimal(1);

                        Console.WriteLine($"\nWelcome back, {username}!");
                        Console.WriteLine($"Your saved salary: ${salary}, expenses: ${expenses}");
                    }
                }
            }

            // === If user does not exist, ask for salary/expenses and insert ===
            if (!userExists)
            {
                Console.WriteLine("No user found. Let's create a new one.");
                Console.Write("Enter your monthly salary: $");
                salary = decimal.Parse(Console.ReadLine());

                Console.Write("Enter your total monthly expenses: $");
                expenses = decimal.Parse(Console.ReadLine());

                string insertQuery = @"
                    INSERT INTO Users (Username, MonthlySalary, MonthlyExpenses)
                    VALUES (@Username, @Salary, @Expenses);";

                using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                {
                    insertCmd.Parameters.AddWithValue("@Username", username);
                    insertCmd.Parameters.AddWithValue("@Salary", salary);
                    insertCmd.Parameters.AddWithValue("@Expenses", expenses);

                    insertCmd.ExecuteNonQuery();
                    Console.WriteLine("✅ New user saved to database.");
                }
            }
        }

        // === Step 6: Periodic Expenses ===
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

            Console.Write("Add another periodic expense? (y/n): ");
            addMorePeriodic = Console.ReadLine().ToLower();

        } while (addMorePeriodic == "y");

        Console.WriteLine("✅ Periodic expenses recorded successfully.");
    }
}
