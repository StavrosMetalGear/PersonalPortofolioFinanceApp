using PersonalPortfolioFinanceApp.Helpers;
using System;
using System.Data.SqlClient;

namespace PersonalPortfolioFinanceApp.Services
{
    public class UserService
    {
        public (decimal, decimal, int) LoginOrRegisterUser(SqlConnection conn, string username)
        {
            decimal salary = 0, expenses = 0;
            int userId = 0;

            // Attempt to find existing user
            string selectQuery = "SELECT Id, MonthlySalary, MonthlyExpenses FROM Users WHERE Username = @Username";
            using (SqlCommand cmd = new SqlCommand(selectQuery, conn))
            {
                cmd.Parameters.AddWithValue("@Username", username);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        userId = reader.GetInt32(0);
                        salary = reader.GetDecimal(1);
                        expenses = reader.GetDecimal(2);

                        ConsoleHelper.PrintSuccess($"\nWelcome back, {username}!");
                        Console.WriteLine($"Your saved salary: ${salary}, expenses: ${expenses}");
                        return (salary, expenses, userId);
                    }
                }
            }

            // If not found, register new user
            ConsoleHelper.PrintHeader("No user found. Let's create a new one.");

            while (true)
            {
                if (decimal.TryParse(ConsoleHelper.Prompt("Enter your monthly salary: $"), out salary) && salary >= 0) break;
                ConsoleHelper.PrintError("Invalid input. Please enter a valid salary.");
            }

            while (true)
            {
                if (decimal.TryParse(ConsoleHelper.Prompt("Enter your total monthly expenses: $"), out expenses) && expenses >= 0) break;
                ConsoleHelper.PrintError("Invalid input. Please enter valid expenses.");
            }

            decimal balance = salary - expenses;

            string insertQuery = @"
                INSERT INTO Users (Username, MonthlySalary, MonthlyExpenses, Balance)
                OUTPUT INSERTED.Id
                VALUES (@Username, @Salary, @Expenses, @Balance)";

            using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
            {
                insertCmd.Parameters.AddWithValue("@Username", username);
                insertCmd.Parameters.AddWithValue("@Salary", salary);
                insertCmd.Parameters.AddWithValue("@Expenses", expenses);
                insertCmd.Parameters.AddWithValue("@Balance", balance);

                userId = (int)insertCmd.ExecuteScalar();
            }

            ConsoleHelper.PrintSuccess("✅ New user saved to the database.");
            return (salary, expenses, userId);
        }

        public void DeleteUser(SqlConnection conn, string username)
        {
            string deleteGoals = "DELETE FROM Goals WHERE UserId = (SELECT Id FROM Users WHERE Username = @Username)";
            using (SqlCommand cmd = new SqlCommand(deleteGoals, conn))
            {
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.ExecuteNonQuery();
            }

            string deleteUser = "DELETE FROM Users WHERE Username = @Username";
            using (SqlCommand cmd = new SqlCommand(deleteUser, conn))
            {
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.ExecuteNonQuery();
            }

            ConsoleHelper.PrintSuccess("✅ Your account has been deleted.");
        }
        public void UpdateUserInfo(SqlConnection conn, string username)
        {
            Console.WriteLine("\n--- Update Your Financial Info ---");

            Console.Write("Enter your new monthly salary: $");
            decimal newSalary = decimal.Parse(Console.ReadLine());

            Console.Write("Enter your new monthly expenses: $");
            decimal newExpenses = decimal.Parse(Console.ReadLine());

            decimal newBalance = newSalary - newExpenses;

            string updateQuery = @"
        UPDATE Users
        SET MonthlySalary = @Salary, MonthlyExpenses = @Expenses, Balance = @Balance
        WHERE Username = @Username";

            using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
            {
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Salary", newSalary);
                cmd.Parameters.AddWithValue("@Expenses", newExpenses);
                cmd.Parameters.AddWithValue("@Balance", newBalance);

                cmd.ExecuteNonQuery();
            }

            Console.WriteLine("✅ Your financial information has been updated.");
        }
    }

}
