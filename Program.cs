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
        decimal balance = salary - expenses;

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
                    insertCmd.Parameters.AddWithValue("@Balance", balance);

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

            string choice;
            do
            {
                Console.WriteLine("\n--- Services ---");
                Console.WriteLine("1. Transfer money to another user");
                Console.WriteLine("2. Delete my account");
                Console.WriteLine("3. Exit");
                Console.Write("Enter your choice: ");
                choice = Console.ReadLine();

                if (choice == "1")
                {
                    // Transfer Service
                    Console.Write("Enter the username of the recipient: ");
                    string recipientUsername = Console.ReadLine();

                    Console.Write("Enter amount to transfer: $");
                    decimal transferAmount = decimal.Parse(Console.ReadLine());

                    if (transferAmount <= 0)
                    {
                        Console.WriteLine("❌ Invalid transfer amount.");
                        continue;
                    }

                    // Get sender balance
                    string getSenderBalanceQuery = "SELECT Balance FROM Users WHERE Username = @Username";
                    decimal senderBalance = 0;
                    using (SqlCommand getBalCmd = new SqlCommand(getSenderBalanceQuery, conn))
                    {
                        getBalCmd.Parameters.AddWithValue("@Username", username);
                        senderBalance = (decimal)getBalCmd.ExecuteScalar();
                    }

                    if (senderBalance < transferAmount)
                    {
                        Console.WriteLine("❌ You don't have enough balance to transfer.");
                        continue;
                    }

                    // Check if recipient exists
                    string getRecipientQuery = "SELECT Balance FROM Users WHERE Username = @Recipient";
                    object recipientBalanceObj;
                    using (SqlCommand checkCmd = new SqlCommand(getRecipientQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Recipient", recipientUsername);
                        recipientBalanceObj = checkCmd.ExecuteScalar();
                    }

                    if (recipientBalanceObj == null)
                    {
                        Console.WriteLine("❌ Recipient user does not exist.");
                        continue;
                    }

                    // Perform the transfer
                    decimal recipientBalance = (decimal)recipientBalanceObj;

                    string updateSender = "UPDATE Users SET Balance = Balance - @Amount WHERE Username = @Username";
                    using (SqlCommand cmd = new SqlCommand(updateSender, conn))
                    {
                        cmd.Parameters.AddWithValue("@Amount", transferAmount);
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.ExecuteNonQuery();
                    }

                    string updateRecipient = "UPDATE Users SET Balance = Balance + @Amount WHERE Username = @Recipient";
                    using (SqlCommand cmd = new SqlCommand(updateRecipient, conn))
                    {
                        cmd.Parameters.AddWithValue("@Amount", transferAmount);
                        cmd.Parameters.AddWithValue("@Recipient", recipientUsername);
                        cmd.ExecuteNonQuery();
                    }

                    Console.WriteLine($"✅ Transferred ${transferAmount:F2} to {recipientUsername}.");
                }
                else if (choice == "2")
                {
                    // Delete account
                    Console.Write("Are you sure you want to delete your account? (y/n): ");
                    string confirm = Console.ReadLine().ToLower();

                    if (confirm == "y")
                    {
                        // Optional: delete user goals too
                        string deleteGoals = "DELETE FROM Goals WHERE UserId = (SELECT Id FROM Users WHERE Username = @Username)";
                        using (SqlCommand cmd = new SqlCommand(deleteGoals, conn))
                        {
                            cmd.Parameters.AddWithValue("@Username", username);
                            cmd.ExecuteNonQuery();
                        }

                        // Delete user
                        string deleteUser = "DELETE FROM Users WHERE Username = @Username";
                        using (SqlCommand cmd = new SqlCommand(deleteUser, conn))
                        {
                            cmd.Parameters.AddWithValue("@Username", username);
                            cmd.ExecuteNonQuery();
                        }

                        Console.WriteLine("✅ Your account has been deleted.");
                        break; // Exit the menu
                    }
                }

            } while (choice != "3");
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

