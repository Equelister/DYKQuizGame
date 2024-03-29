﻿using DYKServer.Database.ConnectionCommands;
using DYKServer.Net.IO;
using DYKShared.Model;
using DYKShared.ModelHelpers;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DYKServer.Net
{
    public class Client
    {
        public Guid GUID { get; set; }
        public UserModel UserModel { get; set; }
        public TcpClient ClientSocket { get; set; }
        private bool RegisterRequest = false;

        PacketReader _packetReader;
        public Client(TcpClient client)
        {
            ClientSocket = client;
            GUID = Guid.NewGuid();
            _packetReader = new PacketReader(ClientSocket.GetStream());

            LoginOrRegister();

            Task.Run(() => Process());
        }

        void LoginOrRegister()
        {
            var opCode = _packetReader.ReadByte();
            string jsonMsg = _packetReader.ReadMessage();
            LoginCredentialsModelHelper lc = LoginCredentialsModelHelper.JsonToModelHelper(jsonMsg);
            LoginValidator lg = new LoginValidator();
            if (opCode == 2)
            {
                UserModel = lg.ValidateLoginCredentials(lc.Login, lg.HashPassword(lc.Password));
            }
            else
            {
                RegisterRequest = true;
                ClientSocket = null;
                _packetReader = null;
                lg.CreateNewAccount(lc.Login, lg.HashPassword(lc.Password), lc.Email);
            }
        }

        void Process()
        {
            if (RegisterRequest == false)
            {
                bool succesLogin = false;

                if (UserModel is not null)
                {
                    succesLogin = true;
                    bool isUniqueUser = GetClientsLoginCredentials(succesLogin);
                    if (isUniqueUser == false)
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
                            case 20:
                                SendHubListAsJson();
                                break;
                            case 21:
                                AddToLobby();
                                break;
                            case 22:
                                SendCategoriesList();
                                break;
                            case 23:
                                UpdateLobbyInfo();
                                break;
                            case 24:
                                RemoveUserFromHub();
                                break;
                            case 26:
                                SetThisPlayerReady();
                                break;
                            case 27:
                                StartEnhancedGame();
                                break;
                            case 28:
                                StartNormalGame();
                                break;
                            case 30:
                                GiveSummaryIfGameEnded();
                                break;
                            case 31:
                                GiveGamesHistoriesList();
                                break;
                            case 32:
                                GiveGameHistoryDetails();
                                break;
                            case 33:
                                StartRoundTwoIfEveryoneGaveEnhancementList();
                                break;
                            default:
                                Console.WriteLine("Client.ReadPacket = default");
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.WriteLine($"{GUID.ToString()} - Disconnected!");
                        ClientSocket.Close();
                        Program.RemoveUserFromHub(GUID.ToString(), UserModel);
                        Program.RemoveUserFromList(GUID.ToString(), UserModel.ID);
                        return;
                    }
                }
            }
            else
            {
                Program.RemoveUserFromList(GUID.ToString(), 0);
            }
        }

        private void StartRoundTwoIfEveryoneGaveEnhancementList()
        {
            var message = _packetReader.ReadMessage();
            Program.StartEnhancedQuizRoundTwo(GUID.ToString(), message);
        }

        private void StartEnhancedGame()
        {
            Program.GetQuestionsForANewGame(GUID.ToString());
            Program.StartQuizGame(GUID.ToString(), DYKShared.Enums.GameTypes.EnhancedQuizGame);
        }

        private void GiveGameHistoryDetails()
        {
            var message = _packetReader.ReadMessage();
            Program.GetUserGameHistoryDetails(GUID.ToString(), message);
        }

        private void GiveGamesHistoriesList()
        {
            Program.GetAllUserGameHistories(GUID.ToString());
        }

        private void GiveSummaryIfGameEnded()
        {
            var message = _packetReader.ReadMessage();
            List<QuestionModel> questionsFromUser = QuestionModel.JsonListToQuestionModelList(message);
            Program.CreateSummaryForTheGame(GUID.ToString(), questionsFromUser);
        }

        private void StartNormalGame()
        {
            Program.GetQuestionsForANewGame(GUID.ToString());
            Program.StartQuizGame(GUID.ToString(), DYKShared.Enums.GameTypes.NormalQuizGame);
        }

        private void SetThisPlayerReady()
        {
            Hub hub = null;
            bool result = Program.SetThisPlayerReady(GUID.ToString(), out hub);
            if (result)
            {
                Program.SendToEveryoneInLobby(
                        hub,
                        System.Text.Json.JsonSerializer.Serialize(hub.HubModel.Users),
                        OpCodes.SendUpdatedPlayersList);
            }
            else
            {
                Console.WriteLine($"User [{this.GUID}] can't be or already is ready");
            }
        }

        private void RemoveUserFromHub()
        {
            Program.RemoveUserFromHub(GUID.ToString(), UserModel);
        }

        private void UpdateLobbyInfo()
        {
            var message = _packetReader.ReadMessage();
            HubModel newHub = HubModel.JsonToSingleLobby(message);
            Hub hub = Program.UpdateReceivedLobbyInfo(GUID.ToString(), newHub);
            if (hub is not null)
            {
                Program.SendToEveryoneInLobby(
                    hub,
                    hub.HubModel.ConvertToJson(),
                    OpCodes.SendUpdatedLobbyInfo);
            }
        }

        private void AddToLobby()
        {
            int receivedJoinCode;
            bool result = Int32.TryParse(_packetReader.ReadMessage(), out receivedJoinCode);
            if (result)
            {
                HubModel hub = Program.AddUserToHub(receivedJoinCode, GUID.ToString());
                if (hub is not null)
                {
                    var message = hub.ConvertToJson();
                    Program.BroadcastMessageToSpecificClient(this.ClientSocket, message, OpCodes.AddToLobbyRequest);
                }
                else
                {
                    var message = "lobbyDoesntExists";
                    Program.BroadcastMessageToSpecificClient(this.ClientSocket, message, OpCodes.AddToLobbyRequest);
                }
            }
            else
            {
                var message = "wrongJoinCode";
                Program.BroadcastMessageToSpecificClient(this.ClientSocket, message, OpCodes.AddToLobbyRequest);
            }
        }

        private void SendHubListAsJson()
        {
            var lobbiesString = Program.GetHubListAsJson(this.GUID.ToString());
            Program.BroadcastMessageToSpecificClient(this.ClientSocket, lobbiesString, OpCodes.LobbiesList);
        }

        private void SendCategoriesList()
        {
            var categoriesString = Program.GetCategoriesAsJson();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Wusyłam kategorie!");
            Console.WriteLine(categoriesString);
            Console.WriteLine();
            Console.WriteLine();
            Program.BroadcastMessageToSpecificClient(this.ClientSocket, categoriesString, OpCodes.CategoryListRequest);
        }

        private bool GetClientsLoginCredentials(bool status)
        {
            try
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
            catch (Exception e)
            { return false; }
        }

        public Client()
        {
        }
    }
}
