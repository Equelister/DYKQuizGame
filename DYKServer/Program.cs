using DYKServer.Database.MenuCommands;
using DYKServer.Database.GameCommands;
using DYKServer.Net;
using DYKServer.Net.IO;
using DYKShared.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text;
using DYKShared.Enums;

namespace DYKServer
{
    public enum OpCodes
    {
        LoginResult = 2,
        LobbiesList = 20,
        AddToLobbyRequest = 21,
        CategoryListRequest = 22,
        SendUpdatedLobbyInfo = 23,
        GetUserDisconnectedHub = 24,
        SendUpdatedPlayersList = 25,
        SendHalfQuestions = 26,
        SendQuestions = 27,
        SendSummary = 30,
        SendgamesHistoriesList = 31,
        SendGameHistoryDetails = 32,
        SendUsersPartialSummary = 35,
        SendMidGameEnhancements = 34,
        SendStartEnhancedGameRoundTwo = 33,
        SendSecondHalfQuestions = 36
    }

    public class Program
    {
        public static List<Client> _users = new List<Client>();
        public static List<Hub> _hubs = new List<Hub>();
        public static List<CategoryModel> _categories = new List<CategoryModel>();
        static TcpListener _listener;
        private readonly static int _numberOfQuesions = 6; //Only even
                

        static void Main(string[] args)
        {
            InitializeCategories();
            InitializeDefaultHubs(15);
            OutPutInitializeToConsole();

            Task.Run(() => {
            while(true)
                {
                    System.Threading.Thread.Sleep(5000);
                    Console.WriteLine("***"); 
                    Console.WriteLine();
                    Console.WriteLine("HUB USER COUNT: " + _hubs.ElementAt(9).Users.Count);
                    Console.WriteLine("HUBMODEL USER COUNT: "+_hubs.ElementAt(9).HubModel.Users.Count);
                    Console.WriteLine("TOTAL HUB COUNT: "+_hubs.Count);
                    Console.WriteLine();
                    foreach (var user in _hubs.ElementAt(9).Users)
                    {
                        Console.WriteLine(user?.UserModel.Username);
                    }
                    Console.WriteLine();
                    foreach (var user in _hubs.ElementAt(9).HubModel.Users)
                    {
                        Console.WriteLine(user?.Username);
                    }
                    Console.WriteLine();
                    Console.WriteLine("***");
                }
            });

            _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 7710);
            _listener.Start();

            Console.WriteLine("******\r\nStarted Listening for clients...");
            while (true)
            {
                _users.Add(new Client(_listener.AcceptTcpClient()));
            }
        }
        static void OutPutInitializeToConsole()
        {
            foreach (Hub hub in _hubs)
            {
                Console.WriteLine($"Created new Hub [{hub.GUID}] - [{hub.HubModel.Name}] - JoinCode [{hub.HubModel.JoinCode}]");
            }
            foreach (CategoryModel category in _categories)
            {
                Console.WriteLine($"Created new Hub [{category.ID}] - [{category.Name}]");
            }
        }






        /// <summary>
        /// HUBS
        /// </summary>
        static void InitializeDefaultHubs(int hubCount)
        {

            for(int i = 0; i<hubCount; i++)
            {
                Hub newHub = new Hub(i + 1 + ". Hub", GetFirstCategoryFromList());
                newHub.IsDefault = true;
                _hubs.Add(newHub);
            }
            CheckHubUniqueJoinCodeAll();
        }

        static void CheckHubUniqueJoinCodeAll()
        {
            for(int i = 0; i<_hubs.Count; i++)
            {
                foreach (Hub nextHub in _hubs)
                {
                    if(_hubs.ElementAt(i).HubModel.JoinCode == nextHub.HubModel.JoinCode && _hubs.ElementAt(i).GUID != nextHub.GUID)
                    {
                        _hubs.ElementAt(i).GenerateJoinCode();
                        i--;
                        break;
                    }
                }
            }
        }

        static void CheckHubUniqueJoinCodeSingle(Hub hub)
        {
            bool unique;
            do
            {
                unique = true;
                foreach (Hub nextHub in _hubs)
                {
                    if (hub.HubModel.JoinCode == nextHub.HubModel.JoinCode)
                    {
                        hub.GenerateJoinCode();
                        unique = false;
                        break;
                    }
                }
            } while (unique == false);
        }

        internal static void StartEnhancedQuizRoundTwo(string uid, string message)
        {
            Client user = _users.Where(x => x.GUID.ToString() == uid).FirstOrDefault();
            Hub hub = _hubs.Where(x => x.Users.Contains(user)).FirstOrDefault();
            List<InGameActions> actionsFromUser = InGameActions.JsonToList(message);
            hub.HubModel.PlayersThatEndedGame++;

            foreach (var actions in actionsFromUser)
            {
                if (actions.UserNickname is null)
                {
                    continue;
                }
                foreach (var currentUser in hub.Users)
                {                    
                    if(currentUser.UserModel.Username.Equals(actions.UserNickname))
                    {
                        currentUser.UserModel.AppliedEnhancementsIDs.Add(actions.ID);
                        break;
                    }
                }
            }

            if(hub.HubModel.PlayersThatEndedGame >= hub.Users.Count)
            {
                foreach(var userToSend in hub.Users)
                {
                    BroadcastMessageToSpecificClient(
                        user.ClientSocket,
                        JsonSerializer.Serialize(userToSend.UserModel.AppliedEnhancementsIDs),
                        OpCodes.SendStartEnhancedGameRoundTwo);
                }
                hub.HubModel.PlayersThatEndedGame = 0;
                StartQuizGame(hub.Users.ElementAt(0).GUID.ToString(), GameTypes.EnhancedQuizGame);
            }
        }

        internal static void GetUserGameHistoryDetails(string uid, string message)
        {
            Client user = _users.Where(x => x.GUID.ToString() == uid).FirstOrDefault();
            bool success = Int32.TryParse(message, out int gameId);
            if(success)
            {
                SummaryQueries sq = new SummaryQueries();
                string msg = JsonSerializer.Serialize(sq.GetAllQuestionsFromGameHistoryWhereGameID(gameId));
                BroadcastMessageToSpecificClient(user.ClientSocket, msg, OpCodes.SendGameHistoryDetails);
            }else
            {
                Console.WriteLine($"Something went wrong with sending GameHistoryDetails to user [{user.GUID}]");
            }
        }

        internal static void GetQuestionsForANewGame(string uid)
        {
            QuestionsReceiver qr = new QuestionsReceiver();
            Client user = _users.Where(x => x.GUID.ToString() == uid).FirstOrDefault();
            Hub hub = _hubs.Where(x => x.Users.Contains(user)).FirstOrDefault();
            hub.Questions = qr.GetRandomQuestions(hub.HubModel.Category.ID, _numberOfQuesions);
        }

        internal static void GetAllUserGameHistories(string uid)
        {
            Client user = _users.Where(x => x.GUID.ToString() == uid).FirstOrDefault();
            SummaryQueries sq = new SummaryQueries();
            string msg = JsonSerializer.Serialize(sq.GetAllGamesHistoriesWhereUserID(user.UserModel.ID));
            BroadcastMessageToSpecificClient(user.ClientSocket, msg, OpCodes.SendgamesHistoriesList);
        }

        internal static void CreateSummaryForTheGame(string uid, List<QuestionModel> questionsFromUser)
        {
            Client user = _users.Where(x => x.GUID.ToString() == uid).FirstOrDefault();
            Hub hub = _hubs.Where(x => x.Users.Contains(user)).FirstOrDefault();
            user.UserModel.IsReady = true;

            if(hub.HubModel.GameRound.Equals((int)GameTypes.NormalQuizGame))
            {
                CreateFullSummaryForTheGame(questionsFromUser, user, hub);
            }else
            {
                CreatePartialSummaryForTheGame(questionsFromUser, user, hub);
            }

        }

        internal static void CreateFullSummaryForTheGame(List<QuestionModel> questionsFromUser, Client user, Hub hub)
            {
            hub.HubModel.PlayersThatEndedGame++;

            if (hub.Summary.Count <= 0)
            {
                hub.Summary = new List<SummaryModel>();
                for (int i = 0; i < hub.Questions.Count; i++)
                {
                    hub.Summary.Add(new SummaryModel(
                        hub.Questions.ElementAt(i).Question,
                        hub.Questions.ElementAt(i).CorrectAnswer,
                        "",
                        0
                    ));
                }
            }

            for(int i = 0; i< hub.Questions.Count; i++)
            {
                if (questionsFromUser.ElementAt(i).IsAnsweredCorrectly)
                {
                    if (hub.Summary.ElementAt(i).FastestAnswerLong > questionsFromUser.ElementAt(i).AnswerTimeMS || hub.Summary.ElementAt(i).FastestAnswerLong == 0)
                    {
                        hub.Summary.ElementAt(i).FastestAnswerName = user.UserModel.Username;
                        hub.Summary.ElementAt(i).FastestAnswerLong = questionsFromUser.ElementAt(i).AnswerTimeMS;
                    }
                    hub.Summary.ElementAt(i).UsersNicknames.Add(user.UserModel.Username);
                    user.UserModel.GameScore += 2;
                }
            }

            Task.Run(() =>
            {
                GiveSummaryIfGameEnded(hub);
            });
        }


        internal static void CreatePartialSummaryForTheGame(List<QuestionModel> questionsFromUser, Client user, Hub hub)
        {
            hub.HubModel.PlayersThatEndedGame++;

            if (hub.Summary.Count <= 0)
            {
                ///hub.Summary = new List<SummaryModel>();
                for (int j = 0; j < hub.Questions.Count; j++)
                {
                    hub.Summary.Add(new SummaryModel(
                        hub.Questions.ElementAt(j).Question,
                        hub.Questions.ElementAt(j).CorrectAnswer,
                        "",
                        0
                    ));
                }
            }
            int i = 0;
            int k = 0;
            if (hub.HubModel.GameRound.Equals((int)GameTypes.EnhancedQuizGameRoundTwo))
            {
                k += (int)Math.Ceiling((double)(_numberOfQuesions / 2));
            }
            else
            {
                hub.HubModel.GameRound = (int)GameTypes.EnhancedQuizGameRoundOne;
            }
            for (; i < questionsFromUser.Count; i++)
            {
                if (questionsFromUser.ElementAt(i).IsAnsweredCorrectly)
                {
                    if (hub.Summary.ElementAt(k).FastestAnswerLong > questionsFromUser.ElementAt(i).AnswerTimeMS || hub.Summary.ElementAt(k).FastestAnswerLong == 0)
                    {
                        hub.Summary.ElementAt(k).FastestAnswerName = user.UserModel.Username;
                        hub.Summary.ElementAt(k).FastestAnswerLong = questionsFromUser.ElementAt(i).AnswerTimeMS;
                    }
                    hub.Summary.ElementAt(k).UsersNicknames.Add(user.UserModel.Username);
                    user.UserModel.GameScore += 2;
                }
                k++;
            }

            Task.Run(() =>
            {
                GiveSummaryIfGameEnded(hub);
            });
        }




        private static void GiveUserExtraPointsForTime(Hub hub)
        {
            foreach (var elem in hub.Summary)
            {
                foreach (var user in hub.Users)
                {
                    if (elem.FastestAnswerName.Equals(user.UserModel.Username))
                    {
                        user.UserModel.GameScore++;
                        break;
                    }
                }
            }
        }

        public static bool IsEverybodyInHubEndedGame(Hub hub)
        {
            foreach(var user in hub.Users)
            {
                if(user.UserModel.IsReady == false)
                {
                    return false;
                }
            }
            return true;
        }

        internal static void GiveSummaryIfGameEnded(Hub hub)
        {
            if (hub.Users.Count > 0)
            {
                if (hub.HubModel.PlayersThatEndedGame >= hub.Users.Count)
                {
                    if (IsEverybodyInHubEndedGame(hub) == false)
                    {
                        Console.WriteLine($"[In {hub.GUID}] there're still some players playing!");
                        return;
                    }

                    hub.HubModel.PlayersThatEndedGame = 0;
                    GiveUserExtraPointsForTime(hub);

                    if (hub.HubModel.GameRound.Equals((int)GameTypes.EnhancedQuizGameRoundOne))
                    {
                        List<UserModel> usersSummary = new List<UserModel>();
                        foreach (var user in hub.Users)
                        {
                            UserModel usertoSumm = new UserModel();
                            usertoSumm.Username = user.UserModel.Username;
                            usertoSumm.GameScore = user.UserModel.GameScore;
                            usersSummary.Add(usertoSumm);
                        }
                        SendToEveryoneInLobby(
                               hub,
                               JsonSerializer.Serialize(usersSummary),
                               OpCodes.SendUsersPartialSummary);

                        List<InGameActions> enhancementsToSend = InGameActions.GetRandomAmoutOfActions(InGameActions.GenerateActions(), hub.Users.Count);

                        SendToEveryoneInLobby(
                                hub,
                                JsonSerializer.Serialize(enhancementsToSend),
                                OpCodes.SendMidGameEnhancements);

                        hub.HubModel.GameRound = (int)GameTypes.EnhancedQuizGameRoundTwo; // For QuizEnhancedMode
                        hub.HubModel.PlayersThatEndedGame = 0;
                    }
                    else if (hub.HubModel.GameRound.Equals((int)GameTypes.EnhancedQuizGameRoundTwo) || hub.HubModel.GameRound.Equals((int)GameTypes.NormalQuizGame))
                    {
                        SendToEveryoneInLobby(
                               hub,
                               JsonSerializer.Serialize(hub.Summary),
                               OpCodes.SendSummary);
                        MakeAnUpdateOnALobbyAfterGame(hub);
                        Task.Run(() =>
                        {
                            InsertSummaryToDB(hub);
                            hub.Summary.Clear();
                            hub.Questions.Clear();
                        });
                    }
                }
                else
                {
                    Console.WriteLine($"[In {hub.GUID}] there're still some players playing!");
                    return;
                }
            }else
            {
                hub.HubModel.IsGameStarted = false;
                hub.HubModel.PlayersThatEndedGame = 0;
                hub.HubModel.GameRound = (int)GameTypes.NormalQuizGame;
            }
        }

        private static void InsertSummaryToDB(Hub hub)
        {
            SummaryQueries sq = new SummaryQueries();
            decimal gameID = sq.InsertNewGameToTable();
            if(gameID > 0)
            {
                List<int> usersID = new List<int>();
                foreach(var user in hub.Users)
                {
                    usersID.Add(user.UserModel.ID);
                }
                int result = sq.InsertUsersToGameID(usersID, gameID);
                if(result > 0)
                {
                    result = sq.InsertQuestionSummariesToGame(hub, gameID);
                    if(result > 0)
                    {
                        return;
                    }
                }
            }
            Console.WriteLine($"Something went wrong with inserting summary to DB in Hub [{hub.GUID}]!");
        }

        private static void MakeAnUpdateOnALobbyAfterGame(Hub hub)
        {
            hub.HubModel.IsGameStarted = false;
            hub.HubModel.PlayersThatEndedGame = 0;
            hub.HubModel.GameRound = (int)GameTypes.NormalQuizGame;
            foreach (var user in hub.Users)
            {
                user.UserModel.IsReady = false;
                user.UserModel.Total_games++;
                user.UserModel.AppliedEnhancementsIDs.Clear();
                UpdateDBWithUserScoresAsync(hub);
            }
            SendToEveryoneInLobby(
                    hub,
                    hub.HubModel.ConvertToJson(),
                    OpCodes.SendUpdatedLobbyInfo);
        }

        private static void UpdateDBWithUserScoresAsync(Hub hub)
        {
            Console.WriteLine("\r\n UpdateDBWithUserScoresAsync => Please implement me!");
            Console.WriteLine("throw new NotImplementedException();\r\n");
        }

        internal static void StartQuizGame(string uid, GameTypes gameType)
        {
            Client user = _users.Where(x => x.GUID.ToString() == uid).FirstOrDefault();
            Hub hub = _hubs.Where(x => x.Users.Contains(user)).FirstOrDefault();
            hub.HubModel.IsGameStarted = true;
            foreach (var usr in hub.Users)
            {
                usr.UserModel.IsReady = false;
            }

            if (gameType.Equals(GameTypes.EnhancedQuizGame))
            {
                int i = 0;
                OpCodes opcode;
                List<QuestionModel> halfQuestions = new List<QuestionModel>();
                if (hub.HubModel.GameRound.Equals((int)GameTypes.EnhancedQuizGameRoundTwo))
                {
                    opcode = OpCodes.SendSecondHalfQuestions;
                    i += (int)Math.Ceiling((double)(_numberOfQuesions / 2));
                    for (; i < _numberOfQuesions; i++)
                    {
                        halfQuestions.Add(hub.Questions.ElementAt(i));
                    }
                }
                else
                {
                    hub.HubModel.GameRound = (int)GameTypes.EnhancedQuizGameRoundOne;
                    opcode = OpCodes.SendHalfQuestions;
                    for (; i < _numberOfQuesions / 2; i++)
                    {
                        halfQuestions.Add(hub.Questions.ElementAt(i));
                    }
                }
                SendToEveryoneInLobby(
                    hub,
                    JsonSerializer.Serialize(halfQuestions),
                    opcode);                
            }
            else if (gameType.Equals(GameTypes.NormalQuizGame))
            {
                hub.HubModel.GameRound = (int)GameTypes.NormalQuizGame;
                SendToEveryoneInLobby(
                                    hub,
                                    JsonSerializer.Serialize(hub.Questions),
                                    OpCodes.SendQuestions);
            }
        }




        public static string GetHubListAsJson(string UID)
        {
            Console.WriteLine($"User [{UID}] requested a lobby list.");
            return Hub.PublicLobbiesToSendCreateJson(_hubs);
        }

        public static bool SetThisPlayerReady(string uid, out Hub hub)
        {            
            Client user = _users.Where(x => x.GUID.ToString() == uid).FirstOrDefault();
            hub = _hubs.Where(x => x.Users.Contains(user)).FirstOrDefault();
            if (hub is not null && user.UserModel.IsReady == false)
            {
                user.UserModel.IsReady = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void RemoveUserFromHub(string uid, UserModel userModel)
        {
            Client user = _users.Where(x => x.GUID.ToString() == uid).FirstOrDefault();
            Hub hub = _hubs.Where(x => x.Users.Contains(user)).FirstOrDefault();
            if (hub is not null)
            {
                hub.HubModel.Users.Remove(userModel);
                hub.Users.Remove(user);
                user.UserModel.IsReady = false;
                Console.WriteLine($"User [{uid}] has been disconnected from hub [{hub.GUID}]");
                if (hub.CheckNotDefaultHubIsEmpty())
                {
                    _hubs.Remove(hub);
                    Console.WriteLine($"Hub [{hub.GUID}] removed, because it was empty.");
                }
                else
                {
                    if(hub.HubModel.IsGameStarted == true)
                    {
                        GiveSummaryIfGameEnded(hub);
                    }
                    SendToEveryoneInLobby(
                                hub,
                                JsonSerializer.Serialize(hub.HubModel.Users),
                                OpCodes.SendUpdatedPlayersList);
                }
            }
            else
            {
                Console.WriteLine($"User [{uid}] wasn't in hub while disconnecting");
            }
        }

        public static HubModel AddUserToHub(int receivedJoinCode, string uid)
        {
            Hub hub;
            try
            {
                hub = _hubs.Where(x => x.HubModel.JoinCode == receivedJoinCode).FirstOrDefault();
            }catch(Exception e)
            {
                hub = null;
            }
            if (hub is not null)
            {
                Client newClient = _users.Where(x => x.GUID.ToString() == uid).FirstOrDefault();
                if (hub.AddClient(newClient))
                {
                    HubModel hubmodel = new HubModel(
                        hub.HubModel.JoinCode,
                        hub.HubModel.MaxSize,
                        hub.HubModel.Name,
                        hub.HubModel.Category,
                        hub.HubModel.IsPrivate
                    );
                    foreach (var user in hub.Users)
                    {
                        hubmodel.Users.Add(user.UserModel);          
                    }
                    hub.HubModel = hubmodel;
                    Task.Run(() => SendToEveryoneInLobbyExceptThisClient(
                                                            hub,
                                                            newClient,
                                                            JsonSerializer.Serialize(hub.HubModel.Users),
                                                            OpCodes.SendUpdatedPlayersList)
                    );
                    return hubmodel;
                }
            }
            return null;
        }

        public static Hub UpdateReceivedLobbyInfo(string UID, HubModel newHub)
        {
            Hub hub = new Hub(newHub);
            if(hub.HubModel.JoinCode == 0)
            {
                if(_hubs.Count > 8999)
                {
                    Console.WriteLine("Reached Full Server Capacity! Cannot create new Hub.");
                    return null;
                }
                /*hub.HubModel.JoinCode = */hub.GenerateJoinCode();
                CheckHubUniqueJoinCodeSingle(hub);
                Client user = _users.Where(x => x.GUID.ToString() == UID).FirstOrDefault();
                if(user is null)
                {
                    Console.WriteLine("User doesnt exists.");
                    return null;
                }
                hub.Users = new List<Client>();
                hub.HubModel.Users = new List<UserModel>();
                hub.AddClient(user);
                hub.HubModel.Users.Add(user.UserModel);
                _hubs.Add(hub);
            }
            else
            {
                hub = _hubs.Where(x => x.HubModel.JoinCode == newHub.JoinCode).FirstOrDefault();
                var usersList = hub.HubModel.Users;
                hub.HubModel = newHub;
                hub.HubModel.Users = usersList;
            }
            return hub;
        }

        public static void SendToEveryoneInLobby(Hub hub, string message, OpCodes opcode)
        {
            Console.WriteLine(message);
            foreach (var user in hub.Users)
            {
                Program.BroadcastMessageToSpecificClient(user.ClientSocket, message, opcode);
            }
        }

        public static void SendToEveryoneInLobbyExceptThisClient(Hub hub, Client newClient, string message, OpCodes opcode)
        {
            foreach (var user in hub.Users)
            {
                if (user.Equals(newClient) == false)
                {
                    Program.BroadcastMessageToSpecificClient(user.ClientSocket, message, opcode);
                }
            }                
        }



        static void InitializeCategories()
        {
            HubCommands hc = new HubCommands();
            _categories = hc.GetCategoriesListNotArchived(); 
            if(IsListEmpty(_categories))
            {
                throw new ArgumentException(
                     "*** Categories do not exists in database ***\r\n" +
                            "*** Please insert at least 1 category into DB ***"
                );
            }
        }

        public static string GetCategoriesAsJson()
        {
            return JsonSerializer.Serialize(_categories);
        }

        static bool IsListEmpty(List<CategoryModel> list)
        {
            if (list == null)
            {
                return true;
            }

            return list.Count == 0;
        }

        static CategoryModel GetFirstCategoryFromList()
        {
            return _categories.ElementAt(0);
        }





        /// <summary>
        /// LOGIN
        /// </summary>
        /// <param name="UID"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool BroadcastLoginResult(string UID, string message)
        {
            RemoveUserFromList(Guid.Empty.ToString());
            Client user = _users.Where(x => x.GUID.ToString() == UID).FirstOrDefault();
            if(user is null)
            {
                RemoveUserFromList(UID);
                return false;
            }
            if (message.Equals("credsLegit"))
            {
                CheckUniqueUser(ref message, user);
            }
            var msgPacket = new PacketBuilder();
            msgPacket.WriteOpCode(2);
            msgPacket.WriteMessage(message);
            if (user is not null)
            {
/*                Console.WriteLine("**********************");
                //Console.WriteLine(msgPacket.WriteMessage()) ;

                foreach (var item in msgPacket.GetPacketBytes())
                {
                    Console.Write(item.ToString()+", ");
                }

                Console.WriteLine("**********************");*/
                user.ClientSocket.Client.Send(msgPacket.GetPacketBytes());
                if (message.Equals("credsNotLegit"))
                {
                    _users.Remove(user);
                    user = null;
                    return false;
                }
                return true; // successfull login
            }
            else
            {
                Console.WriteLine($"Error while finding user [{UID}].");
                return false;
            }
        }

        public static void CheckUniqueUser(ref string message, Client user)
        {
            int count = _users.Where(x => x.UserModel.ID == user.UserModel.ID).Count();
            if (count > 1)
            {
                message = "credsNotLegit";
            }
        }

        public static string RemoveUserFromList(string UID)
        {
            var user = _users.Where(x => x.GUID.ToString() == UID).FirstOrDefault();
            if (user is not null)
            {
                _users.Remove(user);
                user = null;
                return ($"User [{UID}] removed from list");
            }        
            else
            {
                return ($"Error while finding user to remove from list [{UID}].");
            }
        }









        /// <summary>
        /// BROADCASTS
        /// </summary>
        public static void BroadcastMessageToSpecificClient(string UID, string message, OpCodes opcode)
        {
            Client user = _users.Where(x => x.GUID.ToString() == UID).FirstOrDefault();
            var msgPacket = new PacketBuilder();
            msgPacket.WriteOpCode(Convert.ToByte(opcode));
            msgPacket.WriteMessage(message);
            user.ClientSocket.Client.Send(msgPacket.GetPacketBytes());
        }

        public static void BroadcastMessageToSpecificClient(TcpClient client, string message, OpCodes opcode)
        {
            var msgPacket = new PacketBuilder();
            msgPacket.WriteOpCode(Convert.ToByte(opcode));
            msgPacket.WriteMessage(message);
            client.Client.Send(msgPacket.GetPacketBytes());
        }
    }
}
