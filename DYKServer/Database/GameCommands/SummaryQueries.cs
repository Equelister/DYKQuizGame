using DYKShared.Model;
using DYKShared.ModelHelpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DYKServer.Database.GameCommands
{
    class SummaryQueries
    {        
        public List<SummaryModel> GetAllSummariesIDWhereUserID(int userID)
        {
            List<SummaryModel> summaryList = new List<SummaryModel>();
            SummaryModel summary;
            var connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
            string queryString = $"SELECT ID, created_at FROM summaries WHERE User_ID = {userID}";
            //string queryString = $"SELECT * FROM users ORDER BY id OFFSET 1 ROWS";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        summary = new SummaryModel();
                        summary.ID = (int)reader[0];
                        summary.CreatedAt = (DateTime)reader[1];
                        summaryList.Add(summary);
                    }
                }
            }
            return summaryList;
        }


        public decimal InsertNewGameToTable()
        {
            decimal newGameId = -1;
            var connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
            string queryString = $"Insert into games_summaries values (GETDATE()) SELECT SCOPE_IDENTITY();";
            //string queryString = $"SELECT * FROM users ORDER BY id OFFSET 1 ROWS";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        newGameId = (decimal)reader[0];                        
                    }
                }
            }
            return newGameId;
        }


        public int InsertUsersToGameID(List<int> usersID, decimal gameID)
        {
            int result = -1;
            var connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
            StringBuilder queryString = new StringBuilder("Insert into summaries_users (userId, gameId) values ", 400);
            foreach (var id in usersID)
            {
                queryString.Append($" ({id}, {gameID}),");
            }
            queryString = queryString.Remove(queryString.Length - 1, 1);
            queryString.Append(";");

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString.ToString(), connection);
                connection.Open();
                result = command.ExecuteNonQuery();

                // Check Error
                if (result < 0)
                    Console.WriteLine("Error inserting data into Database!");
            }
            return result;
        }


        public int InsertQuestionSummariesToGame(Net.Hub hub, decimal gameID)
        {
            int result = -1;
            var connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
            StringBuilder queryString = new StringBuilder("Insert into summaries_questions (questionId, fastestAnswerName, fastestAnswerLong, gameId, userNicknames) values ", 1000);
            for (int i = 0; i < hub.Summary.Count; i++)
            {
                queryString.Append($" ({hub.Questions.ElementAt(i).ID}, '{hub.Summary.ElementAt(i).FastestAnswerName}', {hub.Summary.ElementAt(i).FastestAnswerLong}, {gameID}, '");
                foreach (var nickname in hub.Summary.ElementAt(i).UsersNicknames)
                {
                    queryString.Append($"{nickname},");
                }
                if (hub.Summary.ElementAt(i).UsersNicknames.Count > 0)
                {
                    queryString = queryString.Remove(queryString.Length - 1, 1);
                }
                queryString.Append("'),");
            }
            queryString = queryString.Remove(queryString.Length - 1, 1);
            queryString.Append(';');

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString.ToString(), connection);
                connection.Open();
                result = command.ExecuteNonQuery();

                // Check Error
                if (result < 0)
                    Console.WriteLine("Error inserting data into Database!");
            }
            return result;
        }


        public List<GameModelHelper> GetAllGamesHistoriesWhereUserID(int userID)
        {
            List<GameModelHelper> gameHistoryList = new List<GameModelHelper>();
            var connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
            string queryString = $"SELECT id, CreatedDateTime FROM games_summaries WHERE id IN (Select gameId FROM summaries_users where userId = {userID})";
            //string queryString = $"SELECT * FROM users ORDER BY id OFFSET 1 ROWS";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        gameHistoryList.Add(new GameModelHelper((int)reader[0], (DateTime)reader[1]));
                    }
                }
            }
            return gameHistoryList;
        }

        public List<SummaryModel> GetAllQuestionsFromGameHistoryWhereGameID(int gameID)
        {
            List<SummaryModel> summaryList = new List<SummaryModel>();
            var connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
            string queryString = $"SELECT questionId ,fastestAnswerName, fastestAnswerLong, userNicknames FROM summaries_questions WHERE gameId = {gameID}";
            //string queryString = $"SELECT * FROM users ORDER BY id OFFSET 1 ROWS";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    QuestionsReceiver qr = new QuestionsReceiver();
                    QuestionModel question = new QuestionModel();
                    while (reader.Read())
                    {
                        question = qr.GetQuestionAndAnswerWhereId((int)reader[0]);
                        string nicknames = (string)reader[3];
                        summaryList.Add(new SummaryModel(
                                question.Question,
                                question.CorrectAnswer,
                                (string)reader[1],
                                (long)reader[2],
                                nicknames.Split(',').ToList()
                                ));
                    }
                }
            }
            return summaryList;
        }

        /// <summary>
        /// Inserts userID with gameID and re
        /// </summary>
        /// <param name="iD"></param>
        /// <returns></returns>
        internal static int InsertSummaryForUser(int iD)
        {
            throw new NotImplementedException();
        }

        internal static void InsertQuestionsForGame(int iD, int summaryID)
        {
            throw new NotImplementedException();
        }
    }
}
