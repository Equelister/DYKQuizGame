using DYKShared.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DYKServer.Database.GameCommands
{
    class QuestionsReceiver
    {
        public List<QuestionModel> GetRandomQuestions(int categoryID, int numberOfQuestions)
        {
            List<QuestionModel> questionsList = new List<QuestionModel>();
            QuestionModel question;
            var connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
            string queryString = $"SELECT TOP {numberOfQuestions} question, correct_answer, wrong_answer_1, wrong_answer_2, wrong_answer_3 FROM questions WHERE category_id = {categoryID} ORDER BY NEWID()";
            //string queryString = $"SELECT * FROM users ORDER BY id OFFSET 1 ROWS";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        question = new QuestionModel(
                            (string)reader[0],
                            (string)reader[1],
                            (string)reader[2],
                            (string)reader[3],
                            (string)reader[4]);
                        questionsList.Add(question);
                    }
                }
            }
            return questionsList;
        }
    }
}
