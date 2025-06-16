using System.Data.SqlClient;

using System;
using System.Data.SqlClient;

namespace PersonalPortfolioFinanceApp.Services
{
    public class GoalService
    {
        public void AddGoals(SqlConnection conn, int userId, string username, decimal salary, decimal expenses)
        {
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
                    Console.WriteLine("❌ This goal is not feasible.");
                }
                else
                {
                    Console.WriteLine("✅ This goal is feasible.");
                    Console.WriteLine($"General savings during goal period: ${generalMonthlySaving:F2}/month");

                    decimal totalGeneralSavings = generalMonthlySaving * goalMonths;
                    Console.WriteLine($"Total general savings in {goalMonths} months: ${totalGeneralSavings:F2}");

                    int totalMonths = 60;
                    if (goalMonths >= totalMonths)
                    {
                        Console.WriteLine($"Total general savings in 5 years: ${generalMonthlySaving * totalMonths:F2}");
                    }
                    else
                    {
                        decimal afterGoal = (salary - expenses) * (totalMonths - goalMonths);
                        Console.WriteLine($"Total general savings in 5 years: ${(generalMonthlySaving * goalMonths + afterGoal):F2}");
                    }

                    string insertGoalQuery = "INSERT INTO Goals (UserId, GoalAmount, GoalMonths) VALUES (@UserId, @GoalAmount, @GoalMonths)";
                    using (SqlCommand insertCmd = new SqlCommand(insertGoalQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@UserId", userId);
                        insertCmd.Parameters.AddWithValue("@GoalAmount", goalAmount);
                        insertCmd.Parameters.AddWithValue("@GoalMonths", goalMonths);
                        insertCmd.ExecuteNonQuery();
                        Console.WriteLine("✅ Your goal was saved to the database.");
                    }
                }

                Console.Write("Do you want to add another goal? (y/n): ");
                addAnotherGoal = Console.ReadLine().ToLower();

            } while (addAnotherGoal == "y");
        }
    }
}
