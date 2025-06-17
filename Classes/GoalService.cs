using System;
using System.Data.SqlClient;
using PersonalPortfolioFinanceApp.Helpers;

namespace PersonalPortfolioFinanceApp.Services
{
    public class GoalService
    {
        public void AddGoals(SqlConnection conn, int userId, string username, decimal salary, decimal expenses)
        {
            string addAnotherGoal;
            do
            {
                ConsoleHelper.PrintHeader("Add a New Financial Goal");

                decimal goalAmount = decimal.Parse(ConsoleHelper.Prompt("Enter your savings goal amount: $"));
                int goalMonths = int.Parse(ConsoleHelper.Prompt("In how many months do you want to reach this goal?: "));

                decimal monthlyGoalSaving = goalAmount / goalMonths;
                decimal generalMonthlySaving = salary - expenses - monthlyGoalSaving;

                Console.WriteLine($"\nYou need to save ${monthlyGoalSaving:F2}/month to reach this goal.");

                if (monthlyGoalSaving + expenses > salary)
                {
                    ConsoleHelper.PrintError("❌ This goal is not feasible.");
                }
                else
                {
                    ConsoleHelper.PrintSuccess("✅ This goal is feasible.");
                    Console.WriteLine($"General savings during goal period: ${generalMonthlySaving:F2}/month");

                    decimal totalGeneralSavings = generalMonthlySaving * goalMonths;
                    Console.WriteLine($"Total general savings in {goalMonths} months: ${totalGeneralSavings:F2}");

                    int totalMonths = 60;
                    decimal fiveYearSavings;

                    if (goalMonths >= totalMonths)
                    {
                        fiveYearSavings = generalMonthlySaving * totalMonths;
                    }
                    else
                    {
                        decimal afterGoal = (salary - expenses) * (totalMonths - goalMonths);
                        fiveYearSavings = (generalMonthlySaving * goalMonths) + afterGoal;
                    }

                    Console.WriteLine($"Total general savings in 5 years: ${fiveYearSavings:F2}");

                    string insertGoalQuery = "INSERT INTO Goals (UserId, GoalAmount, GoalMonths) VALUES (@UserId, @GoalAmount, @GoalMonths)";
                    using (SqlCommand insertCmd = new SqlCommand(insertGoalQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@UserId", userId);
                        insertCmd.Parameters.AddWithValue("@GoalAmount", goalAmount);
                        insertCmd.Parameters.AddWithValue("@GoalMonths", goalMonths);
                        insertCmd.ExecuteNonQuery();
                        ConsoleHelper.PrintSuccess("✅ Your goal was saved to the database.");
                    }
                }

                addAnotherGoal = ConsoleHelper.Prompt("Do you want to add another goal? (y/n): ").ToLower();

            } while (addAnotherGoal == "y");
        }
    }
}

