using PersonalPortfolioFinanceApp.Services;
using PersonalPortofolioFinanceApp.Classes;
using System;
using System.Data.SqlClient;

namespace PersonalPortfolioFinanceApp
{
    public class FinanceApp
    {
        private readonly string _connectionString;

        public FinanceApp(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Run()
        {
            Console.WriteLine("Welcome to the Personal Finance Portfolio App!");

            // Create service instances
            var userService = new UserService();
            var goalService = new GoalService();
            var transferService = new TransferService();
            var expenseService = new ExpenseService();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Step 1: Login or Register
                string username = PromptUsername();
                var (salary, expenses, userId) = userService.LoginOrRegisterUser(conn, username);

                // Step 2: Add goals
                goalService.AddGoals(conn, userId, username, salary, expenses);

                // Step 3: Show service menu (transfer, delete)
                transferService.ShowServiceMenu(conn, username, userService);

                // Step 4: Periodic expenses
                expenseService.HandlePeriodicExpenses();
            }
        }

        private string PromptUsername()
        {
            Console.Write("Enter your username: ");
            return Console.ReadLine();
        }
    }
}

