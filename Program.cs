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

class Program
{
    static void Main()
    {
        string connectionString = "Server=LAPTOP-S064M7H1;Database=FinanceAppDB;Trusted_Connection=True;";
        Console.WriteLine("Welcome to the Personal Finance Portfolio App!");

        Console.Write("Enter your username: ");
        string username = Console.ReadLine();

        decimal salary = 0;
        decimal expenses = 0;
        bool userExists = false;

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();

            // === Check if user exists ===
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

            
            // === Financial Goal Logic (multiple goals supported) ===
            string addAnotherGoal;
            do
            {
                Console.WriteLine("\n--- Add a New Financial Goal ---");
                Console.Write("Enter your savings goal amount: $");
                decimal goalAmount = decimal.Parse(Console.ReadLine());

                Console.Write("In how many months do you want to reach this goal?: ");
                int goalMonths = int.Parse(Console.ReadLine());

                decimal monthlyGoalSaving = goalAmount / goalMonths;
                decimal generalMonthlySaving = salary - expenses - monthlyGoalSaving;

                Console.WriteLine($"\nYou need to save ${monthlyGoalSaving:F2}/month to reach this goal.");

                if (monthlyGoalSaving + expenses > salary)
                {
                    Console.WriteLine("❌ This goal is not feasible. It exceeds your disposable income.");
                }
                else
                {
                    Console.WriteLine("✅ This goal is feasible.");
                    Console.WriteLine($"General savings during goal period: ${generalMonthlySaving:F2}/month");

                    decimal totalGeneralSavings = generalMonthlySaving * goalMonths;
                    Console.WriteLine($"Total general savings in {goalMonths} months (excluding goal): ${totalGeneralSavings:F2}");

                    // Long-term calculation (5 years = 60 months)
                    int totalMonths = 60;
                    if (goalMonths >= totalMonths)
                    {
                        decimal fiveYearSavings = generalMonthlySaving * totalMonths;
                        Console.WriteLine($"Total general savings in 5 years: ${fiveYearSavings:F2}");
                    }
                    else
                    {
                        decimal withGoal = generalMonthlySaving * goalMonths;
                        decimal postGoalMonthly = salary - expenses;
                        decimal afterGoal = postGoalMonthly * (totalMonths - goalMonths);
                        decimal totalFiveYear = withGoal + afterGoal;
                        Console.WriteLine($"Post-goal savings = ${postGoalMonthly:F2}/month");
                        Console.WriteLine($"Total general savings in 5 years: ${totalFiveYear:F2}");
                    }

                    // === Save goal to DB ===
                    

                        string getUserIdQuery = "SELECT Id FROM Users WHERE Username = @Username";
                        int userId = 0;
                        using (SqlCommand getIdCmd = new SqlCommand(getUserIdQuery, conn))
                        {
                            getIdCmd.Parameters.AddWithValue("@Username", username);
                            userId = (int)getIdCmd.ExecuteScalar();
                        //}

                        string insertGoalQuery = @"
                INSERT INTO Goals (UserId, GoalAmount, GoalMonths)
                VALUES (@UserId, @GoalAmount, @GoalMonths);";

                        using (SqlCommand insertGoalCmd = new SqlCommand(insertGoalQuery, conn))
                        {
                            insertGoalCmd.Parameters.AddWithValue("@UserId", userId);
                            insertGoalCmd.Parameters.AddWithValue("@GoalAmount", goalAmount);
                            insertGoalCmd.Parameters.AddWithValue("@GoalMonths", goalMonths);
                            insertGoalCmd.ExecuteNonQuery();
                            Console.WriteLine("✅ Your goal was saved to the database.");
                        }
                    }
                }

                Console.Write("\nDo you want to add another goal? (y/n): ");
                addAnotherGoal = Console.ReadLine().ToLower();

            } while (addAnotherGoal == "y");


            // === Periodic Expenses (you already had this part) ===
            List<PeriodicExpense> periodicExpenses = new List<PeriodicExpense>();
            Console.WriteLine("\n--- Periodic Expenses ---");
            string addMore;

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
                addMore = Console.ReadLine().ToLower();

            } while (addMore == "y");

            Console.WriteLine("✅ Periodic expenses recorded (not saved to DB yet).");
        }
    }
}

