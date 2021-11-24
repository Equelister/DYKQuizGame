using DYKServer.Database.MenuCommands;
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
                Console.WriteLine($"Created new Hub [{hub.GUID}] - [{hub.Name}] - JoinCode [{hub.JoinCode}]");
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

            for(int i = 0; i<20;i++)
            {
                Hub newHub = new Hub(i + 1 + ". Hub", GetFirstCategoryFromList());
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
                    if(_hubs.ElementAt(i).JoinCode == nextHub.JoinCode && _hubs.ElementAt(i).GUID != nextHub.GUID)
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
                    if (hub.JoinCode == nextHub.JoinCode)
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

        public static string GetHubListAsJson(string UID)
        {
            Console.WriteLine($"User [{UID}] requested a lobby list.");
            return Hub.PublicLobbiesToSendCreateJson(_hubs);
        }

        public static HubModel AddUserToHub(int receivedJoinCode, string uid)
        {
            Hub hub = _hubs.Where(x => x.JoinCode == receivedJoinCode).FirstOrDefault();
            if (hub is not null)
            {
                hub.AddClient(_users.Where(x => x.GUID.ToString() == uid).FirstOrDefault());
                HubModel hubmodel = new HubModel(
                    hub.JoinCode,
                    hub.MaxSize,
                    hub.Name,
                    hub.Category                    
                );
                foreach(var user in hub.Users)
                {
                    hubmodel.Users.Add(user.UserModel);
                }
                return hubmodel;
            }
            return null;
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
