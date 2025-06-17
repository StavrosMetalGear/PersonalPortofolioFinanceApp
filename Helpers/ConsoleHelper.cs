namespace PersonalPortfolioFinanceApp.Helpers
{
    public static class ConsoleHelper
    {
        public static string Prompt(string message)
        {
            Console.Write(message);
            return Console.ReadLine();
        }

        public static void PrintSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✅ " + message);
            Console.ResetColor();
        }

        public static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ " + message);
            Console.ResetColor();
        }

        public static void PrintHeader(string header)
        {
            Console.WriteLine($"\n--- {header} ---");
        }
    }
}

