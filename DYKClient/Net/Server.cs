using DYKClient.Net.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DYKClient.Net
{   public enum OpCodes
    {
        SendLogin = 2,
        SendMessage = 5,
        SendLobbiesList = 20,
        SendLobbyJoinCode = 21,
        SendCategoriesList = 22,
        SendNewLobbyInfo = 23,
        SendDisconnectFromLobby = 24,
        ReceivedUpdatedPlayersList = 25,
        SendUserReady = 26,
        SendNewEnhancedGame = 27,
        SendNewNormalGame = 28,
    }
    class Server
    {
        TcpClient _client;
        public PacketReader PacketReader;

        public event Action connectedEvent;
        public event Action messageEvent;
        public event Action userDisconnectedEvent;
        public event Action receivedPublicLobbiesListEvent;
        public event Action receivedCategoryListEvent;
        public event Action receivedNewLobbyInfoEvent;
        public event Action receivedNewPlayersInfoEvent;
        public event Action connectToLobbyViewEvent;
        public event Action unlockLoginButtonEvent;
        public event Action startEnhancedGameEvent;
        public event Action startNormalGameEvent;
        public event Func<bool> receivedLoginResultEvent;

        

        public Server()
        {
            _client = new TcpClient();
        }

        public bool ConnectToServer()
        {
            if (_client.Connected == false)
            {
                try
                {
                    _client.Connect("127.0.0.1", 7710);
                    PacketReader = new PacketReader(_client.GetStream());
                    return true;
                }catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    unlockLoginButtonEvent?.Invoke();
                    return false;
                }
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
            Task.Run(() => {
                Console.WriteLine("Server.cs -> ReadPacket() Dzien Dobry");
                while (_client.Connected)
                {
                    byte opcode = 0;
                    try
                    {
                        Console.WriteLine("Server.cs -> ReadPacket() try. CanRead?: " + PacketReader._ns.CanRead);
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
                            bool? xdddd = receivedLoginResultEvent?.Invoke();
                            if(xdddd.HasValue == false)
                            {
                                return;
                            }
                            if(xdddd == false)
                            {
                                return;
                            }
                            break;
                        case 5:
                            messageEvent?.Invoke();
                            break;
                        case 10:
                            userDisconnectedEvent?.Invoke();
                            break;
                        case 20:
                            receivedPublicLobbiesListEvent.Invoke();
                            break;
                        case 21:
                            connectToLobbyViewEvent.Invoke();
                            break;
                        case 22:
                            receivedCategoryListEvent.Invoke();
                            break;
                        case 23:
                            receivedNewLobbyInfoEvent.Invoke();
                            break;
                        case 25:
                            receivedNewPlayersInfoEvent.Invoke();
                            break;
                        case 26:
                            startEnhancedGameEvent.Invoke();
                            break;
                        case 27:
                            startNormalGameEvent.Invoke();
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
            messagePacket.WriteOpCode(Convert.ToByte(OpCodes.SendMessage));
            messagePacket.WriteString(message);
            _client.Client.Send(messagePacket.GetPacketBytes());
        }

        public void SendMessageToServerOpCode(string message, OpCodes opcode)
        {
            var messagePacket = new PacketBuilder();
            messagePacket.WriteOpCode(Convert.ToByte(opcode));
            messagePacket.WriteString(message);
            _client.Client.Send(messagePacket.GetPacketBytes());
        }
        public void SendOpCodeToServer(OpCodes opcode)
        {
            var messagePacket = new PacketBuilder();
            messagePacket.WriteOpCode(Convert.ToByte(opcode));
            _client.Client.Send(messagePacket.GetPacketBytes());
        }

        public void SendLoginCredentialsToServer(string userEmail, string hashedPassword)
        {
            if (string.IsNullOrEmpty(userEmail) == false && string.IsNullOrEmpty(hashedPassword) == false)
            {
                if (ConnectToServer())
                {
                    string message = String.Concat(userEmail, "%%^^&&", hashedPassword);
                    SendMessageToServerOpCode(message, OpCodes.SendLogin);
                    ReadPacket();
                }
            }
        }

        public bool GetLoginCredentialsResult()
        {
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
                    //DisconnectFromServer
                    return false;
                }
            }else
            {
                return false;
            }
        }
    }
}
