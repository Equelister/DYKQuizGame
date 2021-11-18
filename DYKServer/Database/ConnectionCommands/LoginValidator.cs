using DYKShared.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DYKServer.Database.ConnectionCommands
{
    class LoginValidator 
    {
        public UserModel ValidateLoginCredentials(string userName, string userPassword)
        {
            UserModel usr = new UserModel();
            var connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
            string queryString = $"SELECT id, username, email, total_games FROM users WHERE username LIKE '{userName}' AND password LIKE '{userPassword}'";
            //string queryString = $"SELECT * FROM users ORDER BY id OFFSET 1 ROWS";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        usr.ID = reader.GetInt32(reader.GetOrdinal("id"));
                        usr.Username = (string)reader[1];
                        usr.Email = (string)reader[2];
                        usr.Total_games = (int)reader[3];
                        return usr;
                    }
                }
            }
            return null;
        }

    }
}
