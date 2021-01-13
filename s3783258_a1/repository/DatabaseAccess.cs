using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;

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

            var command = new SqlCommand("DataCount", connection) { CommandType = CommandType.StoredProcedure };
            int count = (int)command.ExecuteScalar();
            
            if (count > 0)
            {
                return false;
            } else
            {
                return true;
            }
        }

        public void PopulateDatabase()
        {
            
        }

    }
}
