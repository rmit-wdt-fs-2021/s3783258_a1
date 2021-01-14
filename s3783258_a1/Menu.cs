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
        public void MainMenu()
        {
            string mainMenu = @"
==========
1. Deposit Funds
2. Withdraw Funds
3. Transfer Funds

Enter and option: ";

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
                        Console.Clear();
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
        public void Login()
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

    }
}
