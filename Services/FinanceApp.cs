using System.Data.SqlClient;

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

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            conn.Open();

            string username = PromptUsername();
            var (salary, expenses, userId) = LoginOrRegisterUser(conn, username);

            AddGoals(conn, userId, username, salary, expenses);
            ShowServiceMenu(conn, username);
            HandlePeriodicExpenses();
        }
    }

    private string PromptUsername()
    {
        Console.Write("Enter your username: ");
        return Console.ReadLine();
    }

    private (decimal salary, decimal expenses, int userId) LoginOrRegisterUser(SqlConnection conn, string username)
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

    private void AddGoals(SqlConnection conn, int userId, string username, decimal salary, decimal expenses)
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
                Console.WriteLine("❌ This goal is not feasible. It exceeds your disposable income.");
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
                    Console.WriteLine($"Post-goal savings = ${(salary - expenses):F2}/month");
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

    private void ShowServiceMenu(SqlConnection conn, string username)
    {
        string choice;
        do
        {
            Console.WriteLine("\n--- Services ---");
            Console.WriteLine("1. Transfer money to another user");
            Console.WriteLine("2. Delete my account");
            Console.WriteLine("3. Exit");
            Console.Write("Enter your choice: ");
            choice = Console.ReadLine();

            if (choice == "1") TransferMoney(conn, username);
            else if (choice == "2")
            {
                Console.Write("Are you sure you want to delete your account? (y/n): ");
                string confirm = Console.ReadLine().ToLower();
                if (confirm == "y")
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
                    break;
                }
            }
        } while (choice != "3");
    }

    private void TransferMoney(SqlConnection conn, string senderUsername)
    {
        Console.Write("Enter the username of the recipient: ");
        string recipientUsername = Console.ReadLine();

        Console.Write("Enter amount to transfer: $");
        decimal amount = decimal.Parse(Console.ReadLine());

        string getSenderBalanceQuery = "SELECT Balance FROM Users WHERE Username = @Username";
        decimal senderBalance = 0;
        using (SqlCommand cmd = new SqlCommand(getSenderBalanceQuery, conn))
        {
            cmd.Parameters.AddWithValue("@Username", senderUsername);
            senderBalance = (decimal)cmd.ExecuteScalar();
        }

        if (senderBalance < amount)
        {
            Console.WriteLine("❌ Insufficient balance.");
            return;
        }

        string getRecipientQuery = "SELECT Balance FROM Users WHERE Username = @Recipient";
        object recipientBalanceObj;
        using (SqlCommand cmd = new SqlCommand(getRecipientQuery, conn))
        {
            cmd.Parameters.AddWithValue("@Recipient", recipientUsername);
            recipientBalanceObj = cmd.ExecuteScalar();
        }

        if (recipientBalanceObj == null)
        {
            Console.WriteLine("❌ Recipient does not exist.");
            return;
        }

        string updateSender = "UPDATE Users SET Balance = Balance - @Amount WHERE Username = @Username";
        using (SqlCommand cmd = new SqlCommand(updateSender, conn))
        {
            cmd.Parameters.AddWithValue("@Amount", amount);
            cmd.Parameters.AddWithValue("@Username", senderUsername);
            cmd.ExecuteNonQuery();
        }

        string updateRecipient = "UPDATE Users SET Balance = Balance + @Amount WHERE Username = @Recipient";
        using (SqlCommand cmd = new SqlCommand(updateRecipient, conn))
        {
            cmd.Parameters.AddWithValue("@Amount", amount);
            cmd.Parameters.AddWithValue("@Recipient", recipientUsername);
            cmd.ExecuteNonQuery();
        }

        Console.WriteLine($"✅ Transferred ${amount:F2} to {recipientUsername}.");
    }

    private void HandlePeriodicExpenses()
    {
        List<PeriodicExpense> expenses = new List<PeriodicExpense>();
        Console.WriteLine("\n--- Periodic Expenses ---");
        string addMore;
        do
        {
            Console.Write("Enter expense name (or press Enter to skip): ");
            string name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name)) break;

            Console.Write($"Enter total amount for {name}: $");
            double amount = double.Parse(Console.ReadLine());

            Console.Write($"Occurs every how many months?: ");
            int months = int.Parse(Console.ReadLine());

            expenses.Add(new PeriodicExpense { Name = name, Amount = amount, EveryXMonths = months });

            Console.Write("Add another? (y/n): ");
            addMore = Console.ReadLine().ToLower();
        } while (addMore == "y");

        Console.WriteLine("✅ Periodic expenses recorded (not saved to DB).");
    }
}
