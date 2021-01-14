﻿using System;
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

    }
}
