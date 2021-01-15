using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using s3783258_a1.utilities;
using s3783258_a1.model;
using Microsoft.Extensions.Configuration;
using SimpleHashing;

namespace s3783258_a1.repository
{
    class DatabaseAccess
    {
        private string connectionString;

        public DatabaseAccess() {
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            connectionString = configuration["ConnectionString"];
        }


        //Creates the tables in the database given in the connection string
        public bool CreateTables()
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = File.ReadAllText("sql/CreateTables.sql");

            //Try to execute the query, else the tables already exist
            try
            {
                command.ExecuteNonQuery();
                return true;
            } catch (SqlException e)
            {
                return false;
            }
        }

        //Checks to see if there are any rows in any table.
        private bool DataCount()
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            var command = new SqlCommand("DataCount", connection);
            command.CommandType = CommandType.StoredProcedure;
            int count = (int)command.ExecuteScalar();
            
            if (count > 0)
            {
                return false;
            } else
            {
                return true;
            }
        }

        //Populate the database from webservice if there are no rows
        public void PopulateDatabase()
        {
            if (DataCount())
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();

                RestData rd = new RestData();
                List<Customer> customers = rd.GetCustomers();
                List<Login> logins = rd.GetLogins();

                foreach (var customer in customers)
                {
                    var command = new SqlCommand("CreateCustomer", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@CUSTOMERID", customer.CustomerID);
                    command.Parameters.AddWithValue("@NAME", customer.Name);
                    command.Parameters.AddWithValue("@ADDRESS", customer.Address);
                    command.Parameters.AddWithValue("@CITY", customer.City);
                    command.Parameters.AddWithValue("@POSTCODE", customer.PostCode);

                    command.ExecuteNonQuery();

                    foreach (var account in customer.Accounts)
                    {
                        var accountCommand = new SqlCommand("CreateAccount", connection);
                        accountCommand.CommandType = CommandType.StoredProcedure;

                        accountCommand.Parameters.AddWithValue("@AccountNumber", account.AccountNumber);
                        accountCommand.Parameters.AddWithValue("@AccountType", account.AccountType);
                        accountCommand.Parameters.AddWithValue("@CustomerID", account.CustomerID);
                        accountCommand.Parameters.AddWithValue("@Balance", account.Balance);

                        accountCommand.ExecuteNonQuery();

                        foreach (var transaction in account.Transactions)
                        {
                            var transCommand = new SqlCommand("CreateTransaction", connection);
                            transCommand.CommandType = CommandType.StoredProcedure;

                            transaction.TransactionType = 'D';
                            transaction.AccountNumber = account.AccountNumber;
                            transaction.DestinationAccountNumber = account.AccountNumber;
                            transaction.Amount = account.Balance;
                            
                            transaction.Comment = "Initial Deposit";

                            transCommand.Parameters.AddWithValue("@TransactionType", transaction.TransactionType);
                            transCommand.Parameters.AddWithValue("@AccountNumber", transaction.AccountNumber);
                            transCommand.Parameters.AddWithValue("@DestinationAccountNumber", transaction.DestinationAccountNumber);
                            transCommand.Parameters.AddWithValue("@Amount", transaction.Amount);
                            transCommand.Parameters.AddWithValue("@Comment", transaction.Comment);
                            transCommand.Parameters.AddWithValue("@TransactionTimeUtc", transaction.TransactionTimeUtc);

                            transCommand.ExecuteNonQuery();
                        }
                    }
                }

                foreach (var login in logins)
                {
                    var command = new SqlCommand("CreateLogin", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@LoginID", login.LoginID);
                    command.Parameters.AddWithValue("@CustomerID", login.CustomerID);
                    command.Parameters.AddWithValue("@PasswordHash", login.PasswordHash);

                    command.ExecuteNonQuery();
                }
            }
        }

        public Login CheckLogin(string loginID, string password)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "Select * FROM [dbo].[Login]";

            var table = new DataTable();
            new SqlDataAdapter(command).Fill(table);
            List<Login> logins = table.Select().Select(x => new Login
            {
                LoginID = (string)x["LoginID"],
                CustomerID = (int)x["CustomerID"],
                PasswordHash = (string)x["PasswordHash"]
            }).ToList();


            foreach (var login in logins)
            {
                if (login.LoginID == loginID && PBKDF2.Verify(login.PasswordHash, password))
                {
                    return login;
                }
            }
            return null;

        }

        public string GetName(int customerID)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "Select Name FROM [dbo].[Customer] WHERE CustomerID = @customerID";
            command.Parameters.AddWithValue("customerID", customerID);
            string name = (string) command.ExecuteScalar();

            return name;
        }

        //Creates transaction and adjusts balance
        public void Deposit(double deposit, Login currentLogin, int accountNumber)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            var command = new SqlCommand("CreateTransaction", connection);
            command.CommandType = CommandType.StoredProcedure;


            Transaction transaction = new Transaction();
            transaction.TransactionType = 'D';
            transaction.AccountNumber = accountNumber;
            transaction.DestinationAccountNumber = accountNumber;
            transaction.Amount = deposit;
            transaction.Comment = "Deposit";
            transaction.TransactionTimeUtc = DateTime.UtcNow;

            command.Parameters.AddWithValue("@TransactionType", transaction.TransactionType);
            command.Parameters.AddWithValue("@AccountNumber", transaction.AccountNumber);
            command.Parameters.AddWithValue("@DestinationAccountNumber", transaction.DestinationAccountNumber);
            command.Parameters.AddWithValue("@Amount", transaction.Amount);
            command.Parameters.AddWithValue("@Comment", transaction.Comment);
            command.Parameters.AddWithValue("@TransactionTimeUtc", transaction.TransactionTimeUtc);

            //Executes the transaction creation
            command.ExecuteNonQuery();

            var balanceCMD = connection.CreateCommand();
            balanceCMD.CommandText = "UPDATE [dbo].[Account] SET Balance = Balance + @Funds WHERE AccountNumber = @AccountNumber";
            balanceCMD.Parameters.AddWithValue("@Funds", deposit);
            balanceCMD.Parameters.AddWithValue("@AccountNumber", accountNumber);

            //Executes the balance adjustment
            balanceCMD.ExecuteNonQuery();
        }

        //Returns all the login accounts from the db
        public List<Account> GetLoginAccounts (Login currentLogin)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM [dbo].[Account] WHERE CustomerID = @CustomerID";
            command.Parameters.AddWithValue("@CustomerID", currentLogin.CustomerID);

            var table = new DataTable();
            new SqlDataAdapter(command).Fill(table);
            List<Account> accounts = table.Select().Select(x => new Account
            {
                AccountNumber = (int)x["AccountNumber"],
                AccountType = Convert.ToChar((string)x["AccountType"]),
                CustomerID = (int)x["CustomerID"],
                Balance = Convert.ToDouble((decimal)x["Balance"])
            }).ToList();

            return accounts;
        }

        //Creates transaction and adjusts balance
        public bool Withdraw(double withdraw, Login currentLogin, int accountNumber)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            var command = new SqlCommand("CreateTransaction", connection);
            command.CommandType = CommandType.StoredProcedure;

            //Check balance is correct 
            double balance = CheckBalance(accountNumber);
            if (balance - withdraw >= 0)
            {

                Transaction transaction = new Transaction();
                transaction.TransactionType = 'W';
                transaction.AccountNumber = accountNumber;
                transaction.DestinationAccountNumber = accountNumber;
                transaction.Amount = withdraw;
                transaction.Comment = "Withdraw";
                transaction.TransactionTimeUtc = DateTime.UtcNow;

                command.Parameters.AddWithValue("@TransactionType", transaction.TransactionType);
                command.Parameters.AddWithValue("@AccountNumber", transaction.AccountNumber);
                command.Parameters.AddWithValue("@DestinationAccountNumber", transaction.DestinationAccountNumber);
                command.Parameters.AddWithValue("@Amount", transaction.Amount);
                command.Parameters.AddWithValue("@Comment", transaction.Comment);
                command.Parameters.AddWithValue("@TransactionTimeUtc", transaction.TransactionTimeUtc);

                //Executes the transaction creation
                command.ExecuteNonQuery();

                return true;
            }
            else
            {
                return false;
            }
        }

        public double CheckBalance(int accountNumber)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Balance FROM [dbo].[Account] WHERE AccountNumber = @AccountNumber";
            command.Parameters.AddWithValue("@AccountNumber", accountNumber);

            return Convert.ToDouble((decimal)command.ExecuteScalar());
        }

        public bool CheckAccountNumber(int accountNumber)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM [dbo].Account WHERE AccountNumber = @AccountNumber";
            command.Parameters.AddWithValue("@AccountNumber", accountNumber);

            int boolean = (int)command.ExecuteScalar();

            if (boolean == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void AddFunds(SqlConnection connection, double amount, int accountNumber)
        {
            var balanceCMD = connection.CreateCommand();
            balanceCMD.CommandText = "UPDATE [dbo].[Account] SET Balance = Balance + @Funds WHERE AccountNumber = @AccountNumber";
            balanceCMD.Parameters.AddWithValue("@Funds", amount);
            balanceCMD.Parameters.AddWithValue("@AccountNumber", accountNumber);

            //Executes the balance adjustment
            balanceCMD.ExecuteNonQuery();
        }

        public void RemoveFunds(SqlConnection connection, double amount, int accountNumber)
        {
            var balanceCMD = connection.CreateCommand();
            balanceCMD.CommandText = "UPDATE [dbo].[Account] SET Balance = Balance - @Funds WHERE AccountNumber = @AccountNumber";
            balanceCMD.Parameters.AddWithValue("@Funds", amount);
            balanceCMD.Parameters.AddWithValue("@AccountNumber", accountNumber);

            //Executes the balance adjustment
            balanceCMD.ExecuteNonQuery();
        }

        public void Transfer(double amount, string comment, int startAccount, int destAccount)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            var command = new SqlCommand("CreateTransaction", connection);
            command.CommandType = CommandType.StoredProcedure;


            Transaction transaction = new Transaction();
            transaction.TransactionType = 'T';
            transaction.AccountNumber = startAccount;
            transaction.DestinationAccountNumber = destAccount;
            transaction.Amount = amount;
            transaction.Comment = comment;
            transaction.TransactionTimeUtc = DateTime.UtcNow;

            command.Parameters.AddWithValue("@TransactionType", transaction.TransactionType);
            command.Parameters.AddWithValue("@AccountNumber", transaction.AccountNumber);
            command.Parameters.AddWithValue("@DestinationAccountNumber", transaction.DestinationAccountNumber);
            command.Parameters.AddWithValue("@Amount", transaction.Amount);
            command.Parameters.AddWithValue("@Comment", transaction.Comment);
            command.Parameters.AddWithValue("@TransactionTimeUtc", transaction.TransactionTimeUtc);

            command.ExecuteNonQuery();

            RemoveFunds(connection, amount, startAccount);
            AddFunds(connection, amount, destAccount);
        }
    }
}
