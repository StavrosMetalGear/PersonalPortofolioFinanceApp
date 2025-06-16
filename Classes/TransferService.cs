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
                    if (Console.ReadLine().ToLower() == "y")
                    {
                        userService.DeleteUser(conn, username);
                        break;
                    }
                }
            } while (choice != "3");
        }

        private void TransferMoney(SqlConnection conn, string sender)
        {
            Console.Write("Enter the username of the recipient: ");
            string recipient = Console.ReadLine();
            Console.Write("Enter amount to transfer: $");
            decimal amount = decimal.Parse(Console.ReadLine());

            string getSenderBalanceQuery = "SELECT Balance FROM Users WHERE Username = @Username";
            decimal senderBalance = 0;
            using (SqlCommand cmd = new SqlCommand(getSenderBalanceQuery, conn))
            {
                cmd.Parameters.AddWithValue("@Username", sender);
                senderBalance = (decimal)cmd.ExecuteScalar();
            }

            if (senderBalance < amount)
            {
                Console.WriteLine("❌ Not enough balance.");
                return;
            }

            string getRecipientQuery = "SELECT Balance FROM Users WHERE Username = @Recipient";
            object recipientResult;
            using (SqlCommand cmd = new SqlCommand(getRecipientQuery, conn))
            {
                cmd.Parameters.AddWithValue("@Recipient", recipient);
                recipientResult = cmd.ExecuteScalar();
            }

            if (recipientResult == null)
            {
                Console.WriteLine("❌ Recipient does not exist.");
                return;
            }

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

            Console.WriteLine($"✅ Transferred ${amount:F2} to {recipient}.");
        }
    }
}
