﻿using DYKServer.Database.MenuCommands;
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
        SendQuestions = 27,
        SendSummary = 30
    }

    class Program
    {
        static List<Client> _users;
        static List<Hub> _hubs;
        static List<CategoryModel> _categories;
        static TcpListener _listener;
                

        static void Main(string[] args)
        {
            _users = new List<Client>();
            _hubs = new List<Hub>();
            _categories = new List<CategoryModel>();
            InitializeCategories();
            InitializeDefaultHubs();
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
        static void InitializeDefaultHubs()
        {

            for(int i = 0; i<15;i++)
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
            bool unique = false;
            do
            {
                bool flag = true;
                foreach (Hub nextHub in _hubs)
                {
                    if (hub.HubModel.JoinCode == nextHub.HubModel.JoinCode)
                    {
                        hub.GenerateJoinCode();
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    unique = true;
                }
            } while (unique == false);
        }

        internal static void CreateSummaryForTheGame(string uid, List<QuestionModel> questionsFromUser)
        {
            Client user = _users.Where(x => x.GUID.ToString() == uid).FirstOrDefault();         //
            Hub hub = _hubs.Where(x => x.Users.Contains(user)).FirstOrDefault();             // To separate method that collects from users questionsList => compares best times and adds usersnickanmes to summary.nickanmes list
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
                    if (hub.Summary.ElementAt(i).FastestAnswerLong > questionsFromUser.ElementAt(i).AnswerTimeMS)
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

        internal static void GiveSummaryIfGameEnded(Hub hub)
        {
        //    Client user = _users.Where(x => x.GUID.ToString() == uid).FirstOrDefault();         //
        //    Hub hub = _hubs.Where(x => x.Users.Contains(user)).FirstOrDefault();             // To separate method that collects from users questionsList => compares best times and adds usersnickanmes to summary.nickanmes list
        //    hub.HubModel.PlayersThatEndedGame++;                                                              //
            if(hub.HubModel.PlayersThatEndedGame >= hub.Users.Count)
            {
                GiveUserExtraPointsForTime(hub);
                SendToEveryoneInLobby(
                                hub,
                                JsonSerializer.Serialize(hub.Summary),       //Send Questions To Everyone in lobby
                                OpCodes.SendSummary);
            }
            else
            {
                Console.WriteLine($"[In {hub.GUID}] there're still some players playing!");
                return;
            }

        }

        internal static void StartNormalGame(string uid)
        {
            Client user = _users.Where(x => x.GUID.ToString() == uid).FirstOrDefault();
            Hub hub = _hubs.Where(x => x.Users.Contains(user)).FirstOrDefault();

            QuestionsReceiver qr = new QuestionsReceiver();
            hub.Questions = qr.GetRandomQuestions(hub.HubModel.Category.ID, 3);

            SendToEveryoneInLobby(
                                hub,
                                JsonSerializer.Serialize(hub.Questions),       //Send Questions To Everyone in lobby
                                OpCodes.SendQuestions);
        }

        public static string GetHubListAsJson(string UID)
        {
            Console.WriteLine($"User [{UID}] requested a lobby list.");
            return Hub.PublicLobbiesToSendCreateJson(_hubs);
        }

        internal static void SetThisPlayerReady(string uid)
        {
            Client user = _users.Where(x => x.GUID.ToString() == uid).FirstOrDefault();
            Hub hub = _hubs.Where(x => x.Users.Contains(user)).FirstOrDefault();
            if (hub is not null && user.UserModel.IsReady == false)
            {
                user.UserModel.IsReady = true;
                SendToEveryoneInLobby(
                                hub,
                                JsonSerializer.Serialize(hub.HubModel.Users),       //Send Updated UserList To Everyone in lobby
                                OpCodes.SendUpdatedPlayersList);
            }
            Console.WriteLine($"User [{uid}] can't be or already is ready");
        }

        public static void RemoveUserFromHub(string uid, UserModel userModel)
        {
            Client user = _users.Where(x => x.GUID.ToString() == uid).FirstOrDefault();
            Hub hub = _hubs.Where(x => x.Users.Contains(user)).FirstOrDefault();
            if (hub is not null)
            {
                hub.HubModel.Users.Remove(userModel);
                hub.Users.Remove(user);
                Console.WriteLine($"User [{uid}] has been disconnected from hub [{hub.GUID}]");
                if (hub.CheckNotDefaultHubIsEmpty())
                {
                    _hubs.Remove(hub);
                    Console.WriteLine($"Hub [{hub.GUID}] removed, because it was empty.");
                }
                else
                {
                    SendToEveryoneInLobby(
                                hub,
                                JsonSerializer.Serialize(hub.HubModel.Users),    //Send Updated UserList To Everyone in lobby
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
            Hub hub = _hubs.Where(x => x.HubModel.JoinCode == receivedJoinCode).FirstOrDefault();
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

        public static void UpdateReceivedLobbyInfo(string UID, HubModel newHub)
        {
            Hub hub = new Hub(newHub);
            if(hub.HubModel.JoinCode == 0)
            {
                hub.HubModel.JoinCode = hub.GenerateJoinCode();
                Client user = _users.Where(x => x.GUID.ToString() == UID).FirstOrDefault();
                hub.Users = new List<Client>();
                hub.HubModel.Users = new List<UserModel>();
                hub.AddClient(user);
                hub.HubModel.Users.Add(user.UserModel);
                _hubs.Add(hub);
            }else
            {
                hub = _hubs.Where(x => x.HubModel.JoinCode == newHub.JoinCode).FirstOrDefault();
                var usersList = hub.HubModel.Users;
                hub.HubModel = newHub;
                hub.HubModel.Users = usersList;
            }

            var message = hub.HubModel.ConvertToJson();
            foreach (var user in hub.Users)
            {
                Program.BroadcastMessageToSpecificUser(user.GUID.ToString(), message, OpCodes.SendUpdatedLobbyInfo);
            }
        }

        private static void SendToEveryoneInLobby(Hub hub, string message, OpCodes opcode)
        {            
            foreach (var user in hub.Users)
            {
                Program.BroadcastMessageToSpecificUser(user.GUID.ToString(), message, opcode);
            }
        }

        private static void SendToEveryoneInLobbyExceptThisClient(Hub hub, Client newClient, string message, OpCodes opcode)
        {
            foreach (var user in hub.Users)
            {
                if (user.Equals(newClient) == false)
                {
                    Program.BroadcastMessageToSpecificUser(user.GUID.ToString(), message, opcode);
                }
            }                
        }



        static void InitializeCategories()
        {
            HubCommands hc = new HubCommands();
            _categories = hc.GetCategoriesList(); 
            if(IsListEmpty(_categories))
            {
                throw new ArgumentException(
                     "*** Categories do not exists in database ***\r\n" +
                            "*** Please insert at least 1 categiry into DB ***"
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
                ChecKUniqueUser(ref message, user);
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

        public static void ChecKUniqueUser(ref string message, Client user)
        {
            int count = _users.Where(x => x.UserModel.ID == user.UserModel.ID).Count();
            if (count > 1)
            {
                message = "credsNotLegit";
            }
            //user = _users.Where(x => x.UID.ToString() == UID).FirstOrDefault();
        }

        public static void RemoveUserFromList(string UID)
        {
            var user = _users.Where(x => x.GUID.ToString() == UID).FirstOrDefault();
            if (user is not null)
            {
                _users.Remove(user);
                user = null;
                Console.WriteLine($"User [{UID}] removed from list");
            }        
            else
            {
                Console.WriteLine($"Error while finding user to remove from list [{UID}].");
            }
        }









        /// <summary>
        /// BROADCASTS
        /// </summary>
        static void BroadcastConnection()
        {
            /*            foreach (var user in _users)
                        {
                            foreach (var usr in _users)
                            {
                                var broadcastPacket = new PacketBuilder();
                                broadcastPacket.WriteOpCode(1);
                                broadcastPacket.WriteMessage(usr.Username);
                                broadcastPacket.WriteMessage(usr.UID.ToString());
                                user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
                            }
                        }*/
        }

        public static void BroadcastMessage(string message)
        {
/*            foreach (var user in _users)
            {
                var msgPacket = new PacketBuilder();
                msgPacket.WriteOpCode(5);
                msgPacket.WriteMessage(message);
                user.ClientSocket.Client.Send(msgPacket.GetPacketBytes());
            }*/
        }
        public static void BroadcastMessageToSpecificUser(string UID, string message, OpCodes opcode)
        {
            Client user = _users.Where(x => x.GUID.ToString() == UID).FirstOrDefault();
            var msgPacket = new PacketBuilder();
            msgPacket.WriteOpCode(Convert.ToByte(opcode));
            msgPacket.WriteMessage(message);
            user.ClientSocket.Client.Send(msgPacket.GetPacketBytes());
        }

        public static void BroadcastDisconnect(string uid)
        {
            var disconnectedUser = _users.Where(x => x.GUID.ToString() == uid).FirstOrDefault();
            _users.Remove(disconnectedUser);
            var broadcastPacket = new PacketBuilder();
            BroadcastMessage($"{disconnectedUser.UserModel.Username} has disconnected from the server!");            
        }




    }
}
