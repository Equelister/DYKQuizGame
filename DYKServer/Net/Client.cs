﻿using DYKServer.Database.ConnectionCommands;
using DYKServer.Net.IO;
using DYKShared.Model;
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
        public Guid GUID { get; set; }
        public UserModel UserModel { get; set; }
        public TcpClient ClientSocket { get; set; }

        PacketReader _packetReader;
        public Client(TcpClient client)
        {
            ClientSocket = client;
            GUID = Guid.NewGuid();
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
                    Console.WriteLine($"[{ DateTime.Now}]: Client with username: [{UserModel.Username}] is already connected!");
                    return;
                }               
                Console.WriteLine($"[{ DateTime.Now}]: Client connected with the username: [{UserModel.Username}]");
            }
            else
            {
                GetClientsLoginCredentials(succesLogin);
                ClientSocket = null;
                GUID = Guid.Empty;
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
                       /* case 5:
                            GetAndBroadcastClientMessage();
                            break;*/
                        case 20:
                            SendHubListAsJson();
                            break;
                        case 21:
                            AddToLobby();
                            break;
                        case 22:
                            SendCategoriesList();
                            break;
                        default:
                            break;
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.WriteLine($"{GUID.ToString()} - Disconnected!");
                    ClientSocket.Close();
                    Program.RemoveUserFromList(GUID.ToString());
                    return;
                }
            }
        }

        private void AddToLobby()
        {
            int receivedJoinCode;
            bool result = Int32.TryParse(_packetReader.ReadMessage(), out receivedJoinCode);
            if(result)
            {
                HubModel hub = Program.AddUserToHub(receivedJoinCode, GUID.ToString());
                if (hub is not null)
                {
                    var message = hub.ConvertToJson();
                    Program.BroadcastMessageToSpecificUser(GUID.ToString(), message, OpCodes.AddToLobbyRequest);
                    //add user to hub and send hubmodel
                }
                else
                {
                    var message = "lobbyDoesntExists";
                    Program.BroadcastMessageToSpecificUser(GUID.ToString(), message, OpCodes.AddToLobbyRequest);
                    // lobby doesnt exist return to user
                }                
            }else
            {
                var message = "wrongJoinCode";
                Program.BroadcastMessageToSpecificUser(GUID.ToString(), message, OpCodes.AddToLobbyRequest);
            }
        }

        private void SendHubListAsJson()
        {
            var lobbiesString = Program.GetHubListAsJson(this.GUID.ToString());
            Program.BroadcastMessageToSpecificUser(this.GUID.ToString(),lobbiesString, OpCodes.LobbiesList);
        }

        private void SendCategoriesList()
        {
            var categoriesString = Program.GetCategoriesAsJson();
            Program.BroadcastMessageToSpecificUser(this.GUID.ToString(), categoriesString, OpCodes.CategoryListRequest);
        }

        private bool GetClientsLoginCredentials(bool status)
        {
            if (status)
            {
                return Program.BroadcastLoginResult(GUID.ToString(), "credsLegit");
            }
            else
            {
                return Program.BroadcastLoginResult(GUID.ToString(), "credsNotLegit");
                
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
