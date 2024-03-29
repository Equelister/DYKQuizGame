﻿using DYKShared.Model;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Configuration;
using System.Data.SqlClient;

namespace DYKServer.Database.ConnectionCommands
{
    class LoginValidator
    {
        public UserModel ValidateLoginCredentials(string email, string userPassword)
        {
            UserModel usr = new UserModel();
            var connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
            string queryString = $"SELECT id, username, email, total_games FROM users WHERE email LIKE '{email}' AND password LIKE '{userPassword}'";
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
                try
                {
                    var result = command.ExecuteNonQuery();

                    if (result < 0)
                        Console.WriteLine("Error inserting data into Database!");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e + "\r\n Error inserting data into Database!");
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public string HashPassword(string password)
        {
            byte[] salt = new byte[128 / 8];
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
