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

        public void WelcomeMenu()
        {
            Console.Write(welcomeMenu);

            DatabaseAccess db = new DatabaseAccess();
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
            Console.Clear();
            Console.WriteLine("Login ID: ");
            var loginID = Console.ReadLine();
            
        }

    }
}
