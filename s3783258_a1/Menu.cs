using s3783258_a1.model;
using s3783258_a1.repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace s3783258_a1
{
    class Menu
    {
        string welcomeMenu = @"
  Welcome
==========
1. Login
2. Quit

Enter an option: ";

        Login currentLogin;
        DatabaseAccess db;

        public Menu()
        {
            db = new DatabaseAccess();
        }

        public void WelcomeMenu()
        {
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

        public void Login()
        {
            bool valid = false;
            while (!valid)
            {
                Console.Write("Login ID: ");
                var loginID = Console.ReadLine();
                Console.Write("Password: ");
                var password = Console.ReadLine();

                currentLogin = db.CheckLogin(loginID, password);
                if (currentLogin != null)
                {
                    valid = true;
                    Console.WriteLine("Welcome " + db.GetName(currentLogin.CustomerID));
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("Invalid Login. Please try again...");
                }
            }
        }

    }
}
