using DYKShared.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DYKServer.Database.MenuCommands
{
    class HubCommands
    {
        public List<CategoryModel> GetCategoriesList()
        {
            List<CategoryModel> categories = new List<CategoryModel>();
            var connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
            string queryString = $"SELECT id, name, description FROM categories";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CategoryModel category = new CategoryModel
                        (
                            reader.GetInt32(reader.GetOrdinal("id")),
                            (string)reader[1],
                            (string)reader[2]
                        );
                        categories.Add(category);
                    }
                }
                connection.Close();
            }
            return categories;
        }

        public List<CategoryModel> GetCategoriesListNotArchived()
        {
            List<CategoryModel> categories = new List<CategoryModel>();
            var connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
            string queryString = $"SELECT id, name, description FROM categories WHERE archive = 0";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CategoryModel category = new CategoryModel
                        (
                            reader.GetInt32(reader.GetOrdinal("id")),
                            (string)reader[1],
                            (string)reader[2]
                        );
                        categories.Add(category);
                    }
                }
                connection.Close();
            }
            return categories;
        }
    }
}
