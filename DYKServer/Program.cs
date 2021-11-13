using DYKServer.Net;
using DYKServer.Net.IO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DYKServer
{
    class Program
    {
        static List<Client> _users;
        static TcpListener _listener;
        static void Main(string[] args)
        {
            _users = new List<Client>();

            _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 7710);
            _listener.Start();


            while (true)
            {
                _users.Add(new Client(_listener.AcceptTcpClient()));
                //_users.Add(client);
                //Console.WriteLine($"Client connected! [{client.UID}]");

                //BroadcastConnection();



            }
        }

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

        public static bool BroadcastLoginResult(string UID, string message)
        {
            RemoveUserFromList(Guid.Empty.ToString());
            Client user = _users.Where(x => x.UID.ToString() == UID).FirstOrDefault();
            if(user is null)
            {
                RemoveUserFromList(UID);
                return false;
            }
            if (message.Equals("credsLegit"))
            {
                int count = _users.Where(x => x.UserModel.ID == user.UserModel.ID).Count();
                if(count > 1)
                {
                    message = "credsNotLegit";
                }
                //user = _users.Where(x => x.UID.ToString() == UID).FirstOrDefault();
            }
            var msgPacket = new PacketBuilder();
            msgPacket.WriteOpCode(2);
            //msgPacket.WriteMessage(message);






            msgPacket.WriteMessage("ABCDEFGHIJKLMNOPQRSTUVWQYZ");











            if (user is not null)
            {
                Console.WriteLine("**********************");
                //Console.WriteLine(msgPacket.WriteMessage()) ;

                foreach (var item in msgPacket.GetPacketBytes())
                {
                    Console.Write(item.ToString()+", ");
                }

                Console.WriteLine("**********************");
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

        public static void RemoveUserFromList(string UID)
        {
            var user = _users.Where(x => x.UID.ToString() == UID).FirstOrDefault();
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

        public static void BroadcastDisconnect(string uid)
        {
            var disconnectedUser = _users.Where(x => x.UID.ToString() == uid).FirstOrDefault();
            _users.Remove(disconnectedUser);
            var broadcastPacket = new PacketBuilder();
            BroadcastMessage($"{disconnectedUser.Username} has disconnected from the server!");            
        }

    }
}
