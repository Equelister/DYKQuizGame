using System;
using System.Configuration;
using System.Data.SqlClient;

namespace DYKServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ReadProducts();
        }

        static void ReadProducts()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
            string queryString = "SELECT Id, username FROM users;";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader[0]}. {reader[1]}");
                    }
                }
            }
        }
    }
}
