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
        public void CreateTables(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = File.ReadAllText("sql/CreateTables.sql");

            command.ExecuteNonQuery();
        }
    }
}
