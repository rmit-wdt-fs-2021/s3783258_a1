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

namespace s3783258_a1.repository
{
    class DatabaseAccess
    {
        //Creates the tables in the database given in the connection string
        public bool CreateTables(string connectionString)
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
        public bool DataCount(string connectionString)
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
        public void PopulateDatabase(string connectionString)
        {
            if (DataCount(connectionString))
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
