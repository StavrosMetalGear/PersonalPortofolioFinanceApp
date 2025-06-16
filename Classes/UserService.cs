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
                        Console.WriteLine($"\nWelcome back, {username}!");
                        Console.WriteLine($"Your saved salary: ${salary}, expenses: ${expenses}");
                        return (salary, expenses, userId);
                    }
                }
            }

            Console.WriteLine("No user found. Let's create a new one.");
            Console.Write("Enter your monthly salary: $");
            salary = decimal.Parse(Console.ReadLine());
            Console.Write("Enter your total monthly expenses: $");
            expenses = decimal.Parse(Console.ReadLine());

            string insertQuery = "INSERT INTO Users (Username, MonthlySalary, MonthlyExpenses, Balance) OUTPUT INSERTED.Id VALUES (@Username, @Salary, @Expenses, @Balance)";
            using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
            {
                insertCmd.Parameters.AddWithValue("@Username", username);
                insertCmd.Parameters.AddWithValue("@Salary", salary);
                insertCmd.Parameters.AddWithValue("@Expenses", expenses);
                insertCmd.Parameters.AddWithValue("@Balance", salary - expenses);
                userId = (int)insertCmd.ExecuteScalar();
            }

            Console.WriteLine("✅ New user saved to database.");
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

            Console.WriteLine("✅ Your account has been deleted.");
        }
    }
}
