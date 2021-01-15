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
Main Menu
==========
1. Deposit Funds
2. Withdraw Funds
3. Transfer Funds
4. My Statements
5. Logout

Enter an option: ";
            Console.WriteLine("Welcome " + db.GetName(currentLogin.CustomerID));
            Console.Write(mainMenu);

            bool valid = false;
            while (!valid)
            {
                var input = Console.ReadLine();
                if (!int.TryParse(input, out var option) || option < 1 || option > 4)
                {
                    Console.Clear();
                    Console.WriteLine("Invalid Input. Please Try Again");
                    Console.WriteLine(mainMenu);
                }

                switch (option)
                {
                    case 1:
                        valid = true;
                        DepositWithdrawMenu("Deposit");
                        break;
                    case 2:
                        valid = true;
                        DepositWithdrawMenu("Withdraw");
                        break;
                    case 3:
                        valid = true;
                        TransferMenu();
                        break;
                    case 4:
                        valid = true;
                        MyStatements();
                        break;
                    case 5:
                        valid = true;
                        Console.Clear();
                        currentLogin = null;
                        Console.WriteLine("Successfully Logged Out");
                        WelcomeMenu();
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

        private void DepositWithdrawMenu(string type)
        {
            Console.Clear();
            List<Account> custAccounts = db.GetLoginAccounts(currentLogin);
            if (custAccounts.Count != 0)
            {
                int accountChoice = ChooseAccount(custAccounts);
            
                bool validAmount = false;
                while (!validAmount)
                {
                    Console.Write("Enter Dollar Amount to " + type + ": ");
                    var amount = Console.ReadLine();

                    if (!double.TryParse(amount, out var option) || option < 0)
                    {
                        Console.Clear();
                        Console.WriteLine("Invalid Input. Please Try Again");
                    }
                    else
                    {
                        if (type == "Deposit")
                        {
                            db.Deposit(option, currentLogin, custAccounts[accountChoice - 1].AccountNumber);
                            validAmount = true;
                        }
                        else
                        {
                            if (!db.Withdraw(option, currentLogin, custAccounts[accountChoice - 1].AccountNumber))
                            {
                                Console.WriteLine();
                                Console.WriteLine("Error: Not enough funds available");
                            }
                            else
                            {
                                validAmount = true;
                            }
                        }

                    }
                }
                Console.Clear();
            }
            else
            {
                Console.WriteLine("No Accounts Available For This User");
                Console.WriteLine();
            }
            MainMenu();
        }

        private void TransferMenu()
        {
            Console.Clear();
            List<Account> custAccounts = db.GetLoginAccounts(currentLogin);
            int startAccount = 0;
            int destAccount = 0;
            int amount = 0;
            if (custAccounts.Count != 0)
            {
                bool valid = false;
                int accountChoice = ChooseAccount(custAccounts);
                startAccount = custAccounts[accountChoice - 1].AccountNumber;
                while (!valid)
                {
                    Console.WriteLine();
                    Console.Write("Enter Destination Account Number: ");
                    var input = Console.ReadLine();
                    if (!int.TryParse(input, out destAccount) || input.Length != 4 || !db.CheckAccountNumber(destAccount))
                    {
                        Console.WriteLine();
                        Console.WriteLine("Invalid Input. Please Try Again");
                    } else if (startAccount == destAccount)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Cant Transfer to Same Account Number. Please Try Again");
                    }
                    else
                    {
                        valid = true;
                    }
                }

                valid = false;
                while (!valid)
                {
                    Console.WriteLine();
                    Console.Write("Enter Transfer Amount Value: ");
                    var input = Console.ReadLine();
                    if (!int.TryParse(input, out amount) || amount <=0 || (db.CheckBalance(startAccount)-amount)<0)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Invalid Input. Please Try Again");
                    }
                    else
                    {
                        valid = true;
                    }
                }

                Console.Write("Enter Transaction Comment (Optional): ");
                string comment = Console.ReadLine();

                db.Transfer(amount, comment, startAccount, destAccount);
                Console.Clear();

                MainMenu();
            }
        }

        public void MyStatements()
        {
            Console.Clear();
            string myStatements = @"
My Statements
=============
";
            List<Account> custAccounts = db.GetLoginAccounts(currentLogin);

            Console.WriteLine(myStatements);
            int accountChoice = ChooseAccount(custAccounts);

            List<Transaction> transactions = db.GetTransactions(custAccounts[accountChoice-1].AccountNumber);

            if (transactions.Count != 0)
            {
                Console.WriteLine("Transaction ID   Type     AccNumber   Destination    Amount      Transaction Date               Comment");
                foreach(var transaction in transactions)
                {
                    Console.WriteLine(String.Format("{0,-17}{1,-9}{2,-12}{3,-15}{4,-12:0.00}{5,-23}\t{6}", transaction.TransactionID, transaction.TransactionType,
                        transaction.AccountNumber, transaction.DestinationAccountNumber, transaction.Amount, transaction.TransactionTimeUtc, transaction.Comment));
                }
            }
            
        }

        //Prints Accounts in a structured format
        private int ChooseAccount(List<Account> custAccounts)
        {
            Console.WriteLine("Accounts for " + db.GetName(currentLogin.CustomerID) + ":");
            Console.WriteLine("    Account Number     Account Type        Balance");
            int num = 1;
            foreach (var account in custAccounts)
            {
                Console.WriteLine(String.Format("{0}.  {1,-19}{2,-20}{3,-20}", num, account.AccountNumber, account.AccountType, account.Balance));
                num++;
            }
            int accountChoice = 0;

            bool valid = false;
            while (!valid)
            {
                Console.WriteLine();
                Console.Write("Which account?: ");
                var input = Console.ReadLine();
                if (!int.TryParse(input, out accountChoice) || accountChoice < 1 || accountChoice > custAccounts.Count)
                {
                    Console.WriteLine("Invalid Input. Please Try Again");
                }
                else
                {
                    valid = true;
                }
            }
            return accountChoice;
       
        }

    }
}
