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
        //public event Action LoginCredentialsEvent;

        enum OpCodes{
            SendLogin = 2,
            SendMessage = 5
        }

        public Server()
        {
            _client = new TcpClient();
        }

        public bool ConnectToServer()
        {
            if (_client.Connected == false)
            {
                _client.Connect("127.0.0.1", 7710);
                PacketReader = new PacketReader(_client.GetStream());
                return true;
            }
            return false;
        }

        public void DisconnectFromServer()
        {
            _client.Close();
            _client.Dispose();
            PacketReader = null;
        }

        private void ReadPacket()
        {
            //bool flagusia = true;
            Task.Run(() => {
                Console.WriteLine("Server.cs -> ReadPacket() Dzien Dobry");
                while (_client.Connected)
                {
                    byte opcode = 0;
                    try
                    {
                        Console.WriteLine("Server.cs -> ReadPacket() try. CanRead?: " + PacketReader._ns.CanRead);

                        //opcode =  PacketReader.Read();
                        opcode =  PacketReader.ReadByte();
                        Console.WriteLine("OPCODE: "+opcode);
                    }catch(System.IO.IOException IOE)
                    {
                        Console.WriteLine(IOE.ToString());
                        return;
                    }catch(System.InvalidOperationException invalidOperationE)
                    {
                        Console.WriteLine("Propably user was clicking to fast at login phase \r\n" + invalidOperationE.ToString());
                        return;
                    }catch(Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    switch (opcode)
                    {
                        case 1:
                            connectedEvent?.Invoke();
                            break;
                        case 2:
                            if(GetLoginCredentialsResult() == false)
                            {
                                //flagusia
                                return;
                            }
                            //GetLoginCredentialsResult();
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
                    Task.Delay(1000);
                    System.Threading.Thread.Sleep(1000);
                }
            });
        }

        public void SendMessageToServer(string message)
        {
            var messagePacket = new PacketBuilder();
            messagePacket.WriteOpCode(Convert.ToByte(OpCodes.SendMessage));
            messagePacket.WriteString(message);
            _client.Client.Send(messagePacket.GetPacketBytes());
        }

        public void SendMessageToServerOpCode(string message, byte opcode)
        {
            var messagePacket = new PacketBuilder();
            messagePacket.WriteOpCode(opcode);
            messagePacket.WriteString(message);
            _client.Client.Send(messagePacket.GetPacketBytes());
        }

        public void SendLoginCredentialsToServer(string userEmail, string hashedPassword)
        {
            if (string.IsNullOrEmpty(userEmail) == false && string.IsNullOrEmpty(hashedPassword) == false)
            {
                if (ConnectToServer())
                {
                    string message = String.Concat(userEmail, "%%^^&&", hashedPassword);
                    /*var messagePacket = new PacketBuilder();
                    messagePacket.WriteOpCode(2);
                    messagePacket.WriteString(message);
                    _client.Client.Send(messagePacket.GetPacketBytes());*/
                    SendMessageToServerOpCode(message, Convert.ToByte(OpCodes.SendLogin));
                    ReadPacket();
                }
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
            var message = this.PacketReader.ReadMessage();
            if (string.IsNullOrEmpty(message) == false)
            {
                if (message.Contains("\0"))
                {
                    message = message.Trim('\0');
                }
                if (message.Equals("credsLegit"))
                {
                    return true;
                }
                else
                {
                    //DisconnectFromServer();
                    return false;
                }
            }else
            {
                return false;
            }

            //return true;
        }
    }
}
