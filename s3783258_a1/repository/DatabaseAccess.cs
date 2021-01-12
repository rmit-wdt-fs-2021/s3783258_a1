using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace s3783258_a1.repository
{
    class DatabaseAccess
    {
        //Creates the tables in the database given in the connection string
        public void CreateTables(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = File.ReadAllText("sql/CreateTables.sql");

            //Try to execute the query, else the tables already exist
            try
            {
                command.ExecuteNonQuery();
            } catch (SqlException e)
            {
                Console.WriteLine("Error: Tables Already Exist");
            }
        }
    }
}
