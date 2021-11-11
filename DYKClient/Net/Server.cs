using DYKClient.Net.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DYKClient.Net
{
    class Server
    {
        TcpClient _client;
        public PacketReader PacketReader;

        public event Action connectedEvent;
        public event Action messageEvent;
        public event Action userDisconnectedEvent;
        public event Action LoginCredentialsEvent;

        public Server()
        {
            _client = new TcpClient();
        }

        public void ConnectToServer(string username)
        {
            if (_client.Connected == false)
            {
                _client.Connect("127.0.0.1", 7710);
                PacketReader = new PacketReader(_client.GetStream());

                if (string.IsNullOrEmpty(username) == false)
                {
                    var connectPacket = new PacketBuilder();
                    connectPacket.WriteOpCode(0);
                    connectPacket.WriteString(username);
                    _client.Client.Send(connectPacket.GetPacketBytes());
                }
                ReadPacket();
                /*if (_client.Connected)
                {
                    ReadPacket();
                }else
                {
                    PacketReader = null;
                }*/
            }
        }

        public void DisconnectFromServer()
        {
            _client.Close();
        }

        private void ReadPacket()
        {
            Task.Run(() => {
                while (_client.Connected)
                {
                    var opcode = 0;
                    try
                    {
                        opcode = PacketReader.ReadByte();
                    }catch(System.IO.IOException IOE)
                    {
                        Console.WriteLine(IOE.ToString());
                        return;
                    }catch(System.InvalidOperationException invalidOperationE)
                    {
                        Console.WriteLine("Propably user was clicking to fast at login phase \r\n" + invalidOperationE.ToString());
                        return;
                    }
                    switch (opcode)
                    {
                        case 1:
                            connectedEvent?.Invoke();
                            break;
                        case 2:
                            /*if(GetLoginCredentialsResult() == false)
                            {
                                return;
                            }*/
                            GetLoginCredentialsResult();
                            break;
                        case 5:
                            messageEvent?.Invoke();
                            break;
                        case 10:
                            userDisconnectedEvent?.Invoke();
                            break;
                        default:
                            Console.WriteLine("Server.ReadPacket = default");
                            break;
                    }
                }
            });
        }

        public void SendMessageToServer(string message)
        {
            var messagePacket = new PacketBuilder();
            messagePacket.WriteOpCode(5);
            messagePacket.WriteString(message);
            _client.Client.Send(messagePacket.GetPacketBytes());
        }

        public void SendLoginCredentialsToServer(string userEmail, string hashedPassword)
        {
            if (_client.Connected == false)
            {
                //_client = new TcpClient();
                _client.Connect("127.0.0.1", 7710);
                PacketReader = new PacketReader(_client.GetStream());

                if (string.IsNullOrEmpty(userEmail) == false && string.IsNullOrEmpty(hashedPassword) == false)
                {

                    string message = String.Concat(userEmail, "%%^^&&", hashedPassword);
                    var messagePacket = new PacketBuilder();
                    messagePacket.WriteOpCode(2);
                    messagePacket.WriteString(message);
                    _client.Client.Send(messagePacket.GetPacketBytes());
                }
                ReadPacket();
               
            }
        }

        public bool GetLoginCredentialsResult()
        {
            /*            UserModel user = new UserModel
                        {
                            Username = _server.PacketReader.ReadMessage(),
                            UID = _server.PacketReader.ReadMessage()
                        };

                        if (Users.Any(x => x.UID == user.UID) == false)
                        {
                            Application.Current.Dispatcher.Invoke(() => Users.Add(user));
                        }*/

            var message = this.PacketReader.ReadMessage().Trim('\0');

            if (message.Equals("credsLegit"))
            {
                return true;
            }
            else
            {
                //DisconnectFromServer();
                return false;
            }

            //return true;
        }
    }
}
