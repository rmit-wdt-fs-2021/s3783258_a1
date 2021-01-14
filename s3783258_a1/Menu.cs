using s3783258_a1.model;
using s3783258_a1.repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace s3783258_a1
{
    class Menu
    {
        

        Login currentLogin;
        DatabaseAccess db;

        public Menu()
        {
            db = new DatabaseAccess();
        }

        //Welcome menu for login
        public void WelcomeMenu()
        {
            string welcomeMenu = @"
  Welcome
==========
1. Login
2. Quit

Enter an option: ";
            Console.Write(welcomeMenu);
            db.CreateTables();
            db.PopulateDatabase();


            bool valid = false;
            while (!valid)
            {
                var input = Console.ReadLine();
                if (!int.TryParse(input, out var option) || option < 1 || option > 2)
                {
                    Console.Clear();
                    Console.WriteLine("Invalid Input. Please Try again");
                    Console.WriteLine(welcomeMenu);

                }

                switch (option)
                {
                    case 1:
                        valid = true;
                        Console.Clear();
                        Login();
                        break;
                    case 2:
                        valid = true;
                        Console.WriteLine("\nQuitting.....");
                        return;
                }
            }

        }
        //Main Menu after user logs in
        private void MainMenu()
        {
            string mainMenu = @"
==========
1. Deposit Funds
2. Withdraw Funds
3. Transfer Funds

Enter and option: ";

            Console.WriteLine(mainMenu);

            bool valid = false;
            while (!valid)
            {
                var input = Console.ReadLine();
                if (!int.TryParse(input, out var option) || option < 1 || option > 3)
                {
                    Console.Clear();
                    Console.WriteLine("Invalid Input. Please Try Again");
                    Console.WriteLine(mainMenu);
                }

                switch (option)
                {
                    case 1:
                        valid = true;
                        DepositMenu();
                        break;
                    case 2:
                        valid = true;
                        Console.Clear();
                        break;
                    case 3:
                        valid = true;
                        Console.Clear();
                        break;
                }
            }

            Console.WriteLine(mainMenu);
        }

        //Login with hidden input and comparing login with db
        private void Login()
        {
            bool valid = false;
            while (!valid)
            {
                Console.Write("Login ID: ");
                var loginID = Console.ReadLine();
                Console.Write("Password: ");
                string password = PasswordBuilder().ToString();
                Console.WriteLine();

                currentLogin = db.CheckLogin(loginID, password);
                //See if login exist, if not try again
                if (currentLogin != null)
                {
                    valid = true;
                    Console.Clear();
                    Console.WriteLine("Welcome " + db.GetName(currentLogin.CustomerID));
                    MainMenu();
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("Invalid Login. Please try again...");
                }
            }
        }

        //Password string builder as Console.ReadLine doesnt allow for hidden input
        private StringBuilder PasswordBuilder()
        {
            StringBuilder password = new StringBuilder();
            bool finished = false;
            char finishChar = '\r';
            //Repeat inputs until return key
            while (!finished)
            {
                ConsoleKeyInfo consoleKeyInfo = Console.ReadKey(true);
                char passwordChar = consoleKeyInfo.KeyChar;
                if (passwordChar == finishChar)
                {
                    finished = true;
                }
                else
                {
                    password.Append(passwordChar.ToString());
                }
            }

            return password;
        }

        private void DepositMenu()
        {
            Console.Clear();
            List<Account> custAccounts = db.GetLoginAccounts(currentLogin);
            int accountChoice = 0;
            if (custAccounts != null)
            {
                Console.WriteLine("Accounts for " + db.GetName(currentLogin.CustomerID) + ":");
                Console.WriteLine("    Account Number     Account Type        Balance");
                int num = 1;
                foreach (var account in custAccounts)
                { 
                    Console.WriteLine(String.Format("{0}.  {1,-19}{2,-20}{3,-20}", num, account.AccountNumber, account.AccountType, account.Balance));
                    num++;
                }

                bool valid = false;
                while (!valid)
                {
                    Console.WriteLine();
                    Console.Write("Which account?: ");
                    var input = Console.ReadLine();
                    if (!int.TryParse(input, out accountChoice) || accountChoice < 1 || accountChoice > num)
                    {
                        Console.Clear();
                        Console.WriteLine("Invalid Input. Please Try Again");
                    }
                    else
                    {
                        valid = true;
                    }
                }
            }
            bool validDeposit = false;
            while (!validDeposit)
            {
                Console.Write("Enter Dollar Amount to Deposit: ");
                var deposit = Console.ReadLine();

                if (!int.TryParse(deposit, out var option) || option < 0)
                {
                    Console.Clear();
                    Console.WriteLine("Invalid Input. Please Try Again");
                }
                else
                {
                    validDeposit = true;
                    db.Deposit(option, currentLogin, custAccounts[accountChoice-1].AccountNumber);
                    db.AddFundsAccount(option, custAccounts[accountChoice - 1].AccountNumber);
                }
            }
            
            
        }

    }
}
