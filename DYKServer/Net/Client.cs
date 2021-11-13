using DYKServer.Database.ConnectionCommands;
using DYKServer.Model;
using DYKServer.Net.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DYKServer.Net
{
    class Client
    {
        public string Username { get; set; }
        public Guid UID { get; set; }
        public UserModel UserModel { get; set; }
        public TcpClient ClientSocket { get; set; }

        PacketReader _packetReader;
        public Client(TcpClient client)
        {
            ClientSocket = client;
            UID = Guid.NewGuid();
            _packetReader = new PacketReader(ClientSocket.GetStream());

            var opCode = _packetReader.ReadByte();
            string[] connectionRequestMessage = _packetReader.ReadMessage().Split("%%^^&&");


            LoginValidator lg = new LoginValidator();
            UserModel = lg.ValidateLoginCredentials(connectionRequestMessage.ElementAt(0), connectionRequestMessage.ElementAt(1));


            Task.Run(() => Process());
        }

        void Process()
        {
            bool succesLogin = false;

            if (UserModel is not null)
            {
                succesLogin = true;
                bool isUniqueUser = GetClientsLoginCredentials(succesLogin);
                if(isUniqueUser == false)
                {
                    Console.WriteLine($"[{ DateTime.Now}]: Client with username: [{Username}] is already connected!");
                    return;
                }
                Username = UserModel.Username;
                Console.WriteLine($"[{ DateTime.Now}]: Client connected with the username: [{Username}]");
            }
            else
            {
                GetClientsLoginCredentials(succesLogin);
                ClientSocket = null;
                UID = Guid.Empty;
                _packetReader = null;
            }


            while (succesLogin)
            {
                try
                {
                    var opcode = _packetReader.ReadByte();
                    switch (opcode)
                    {
                       /* case 2:
                            //GetClientsLoginCredentials();
                            break;*/
                        case 5:
                            GetAndBroadcastClientMessage();
                            break;
                        default:
                            break;
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.WriteLine($"{UID.ToString()} - Disconnected!");
                    //Program.BroadcastDisconnect(UID.ToString());
                    ClientSocket.Close();
                    Program.RemoveUserFromList(UID.ToString());
                    return;
                }
            }
        }

        private bool GetClientsLoginCredentials(bool status)
        {
            if (status)
            {
                return Program.BroadcastLoginResult(UID.ToString(), "credsLegit");
            }
            else
            {
                return Program.BroadcastLoginResult(UID.ToString(), "credsNotLegit");
                
            }
        }


        

        private void GetAndBroadcastClientMessage()
        {
/*            var message = _packetReader.ReadMessage();
            Console.WriteLine($"{DateTime.Now} [{Username}]: {message}");
            Program.BroadcastMessage($"{DateTime.Now} [{Username}]: {message}");*/
        }

        
    }
}
