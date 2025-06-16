using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using PersonalPortfolioFinanceApp;





class Program
{
    static void Main()
    {
        string connectionString = "Server=LAPTOP-S064M7H1;Database=FinanceAppDB;Trusted_Connection=True;";
        FinanceApp app = new FinanceApp(connectionString);
        app.Run();
    }
}
