using DYKShared.Model;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
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
                connection.Close();
            }
            return null;
        }

        public void CreateNewAccount(string userName, string userPassword, string userEmail)
        {
            UserModel usr = new UserModel();
            var connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
            string queryString = $"INSERT INTO users ([username],[email],[password]) VALUES ('{userName}', '{userEmail}', '{userPassword}')";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString.ToString(), connection);
                connection.Open();
                var result = command.ExecuteNonQuery();

                if (result < 0)
                    Console.WriteLine("Error inserting data into Database!");
                connection.Close();
            }
        }

        public string HashPassword(string password)
        {
            byte[] salt = new byte[128 / 8];
            /*            using (var rngCsp = new RNGCryptoServiceProvider())
                        {
                            rngCsp.GetNonZeroBytes(salt);
                        }*/
            for (int i = 0; i < salt.Length; i++)
            {
                salt[i] = 0;
            }
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));
            return hashed;
        }
    }
}
