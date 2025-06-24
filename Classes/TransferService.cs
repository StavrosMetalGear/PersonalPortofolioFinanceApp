using PersonalPortfolioFinanceApp.Helpers;
using PersonalPortfolioFinanceApp.Services;
using System;
using System.Data.SqlClient;

namespace PersonalPortofolioFinanceApp.Classes
{
    public class TransferService
    {
        public void ShowServiceMenu(SqlConnection conn, string username, UserService userService)
        {
            string choice;
            do
            {
                ConsoleHelper.PrintHeader("--- Services ---");
                Console.WriteLine("1. Transfer money to another user");
                Console.WriteLine("2. Delete my account");
                Console.WriteLine("3. Update my salary/expenses");
                Console.WriteLine("4. Exit");

                choice = ConsoleHelper.Prompt("Enter your choice: ");

                switch (choice)
                {
                    case "1":
                        TransferMoney(conn, username);
                        break;
                    case "2":
                        string confirm = ConsoleHelper.Prompt("Are you sure you want to delete your account? (y/n): ").ToLower();
                        if (confirm == "y")
                        {
                            userService.DeleteUser(conn, username);
                            return; // Exit after deletion
                        }
                        break;
                    case "3":
                        userService.UpdateUserInfo(conn, username);
                        break;
                    case "4":
                        ConsoleHelper.PrintSuccess("Exiting service menu...");
                        break;
                    default:

                        ConsoleHelper.PrintError("Invalid option. Please choose 1, 2, or 3.");
                        break;
                }

            } while (choice != "3");
        }

        private void TransferMoney(SqlConnection conn, string sender)
        {
            try
            {
                string recipient = ConsoleHelper.Prompt("Enter the username of the recipient: ");
                string inputAmount = ConsoleHelper.Prompt("Enter amount to transfer: $");

                if (!decimal.TryParse(inputAmount, out decimal amount) || amount <= 0)
                {
                    ConsoleHelper.PrintError("Invalid transfer amount.");
                    return;
                }

                // Check sender balance
                string getSenderBalanceQuery = "SELECT Balance FROM Users WHERE Username = @Username";
                decimal senderBalance;

                using (SqlCommand cmd = new SqlCommand(getSenderBalanceQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", sender);
                    object result = cmd.ExecuteScalar();
                    senderBalance = result != null ? (decimal)result : 0;
                }

                if (senderBalance < amount)
                {
                    ConsoleHelper.PrintError("❌ Not enough balance to complete the transfer.");
                    return;
                }

                // Check recipient
                string getRecipientQuery = "SELECT Balance FROM Users WHERE Username = @Recipient";
                object recipientResult;

                using (SqlCommand cmd = new SqlCommand(getRecipientQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Recipient", recipient);
                    recipientResult = cmd.ExecuteScalar();
                }

                if (recipientResult == null)
                {
                    ConsoleHelper.PrintError("❌ Recipient does not exist.");
                    return;
                }

                // Transfer money
                using (SqlCommand updateSender = new SqlCommand("UPDATE Users SET Balance = Balance - @Amount WHERE Username = @Username", conn))
                {
                    updateSender.Parameters.AddWithValue("@Amount", amount);
                    updateSender.Parameters.AddWithValue("@Username", sender);
                    updateSender.ExecuteNonQuery();
                }

                using (SqlCommand updateRecipient = new SqlCommand("UPDATE Users SET Balance = Balance + @Amount WHERE Username = @Recipient", conn))
                {
                    updateRecipient.Parameters.AddWithValue("@Amount", amount);
                    updateRecipient.Parameters.AddWithValue("@Recipient", recipient);
                    updateRecipient.ExecuteNonQuery();
                }

                ConsoleHelper.PrintSuccess($"✅ Transferred ${amount:F2} to {recipient}.");
            }
            catch (SqlException ex)
            {
                ConsoleHelper.PrintError("A database error occurred: " + ex.Message);
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError("An unexpected error occurred: " + ex.Message);
            }
        }
    }
}
