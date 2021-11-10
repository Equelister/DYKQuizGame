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

        public static void BroadcastLoginResult(string UID, string message)
        {            
            var msgPacket = new PacketBuilder();
            msgPacket.WriteOpCode(2);
            msgPacket.WriteMessage(message);
            _users.Where(x => x.UID.ToString() == UID).FirstOrDefault()
                .ClientSocket.Client.Send(msgPacket.GetPacketBytes());
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
