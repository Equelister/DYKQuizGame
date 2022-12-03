using DYKShared.Model;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace DYKServer.Net
{
    public class Hub
    {
        public Guid GUID { get; set; }
        public HubModel HubModel { get; set; }
        public List<Client> Users { get; set; }
        public List<QuestionModel> Questions { get; set; }
        public List<SummaryModel> Summary { get; set; }
        public bool IsDefault = false;

        public Hub(HubModel hubModel)
        {
            GUID = Guid.NewGuid();
            HubModel = hubModel;
            Summary = new List<SummaryModel>();
        }

        public Hub(string name, CategoryModel category)
        {
            GUID = Guid.NewGuid();
            Users = new List<Client>();
            HubModel = new HubModel(
                new Random().Next(9000) + 1000,
            8,
                name,
                category,
                false
            );
            Summary = new List<SummaryModel>();
        }

        public Hub(string name, int maxSize, bool isPrivate, CategoryModel category)
        {
            GUID = Guid.NewGuid();
            Users = new List<Client>();
            HubModel = new HubModel(
                new Random().Next(9000) + 1000,
                maxSize,
                name,
                category,
                isPrivate
            );
            Summary = new List<SummaryModel>();
        }

        public void GenerateJoinCode()
        {
            this.HubModel.JoinCode = new Random().Next(9000) + 1000;
        }

        public bool AddClient(Client client)
        {
            if (Users.Count < HubModel.MaxSize && HubModel.IsGameStarted == false)
            {
                Users.Add(client);
                return true;
            }
            else
            {
                Console.WriteLine($"Client [{client.GUID}] tried to connect into hub [{this.GUID}] but it was full or during game.");
                return false;
            }
        }

        public static string PublicLobbiesToSendCreateJson(List<Hub> lobbies)
        {
            var toJsonList = new List<HubModel>();
            foreach (var lobby in lobbies)
            {
                if (lobby.HubModel.IsPrivate == false)
                {
                    toJsonList.Add(new HubModel(
                            lobby.HubModel.Name,
                            lobby.HubModel.JoinCode,
                            lobby.HubModel.Category,
                            lobby.HubModel.MaxSize,
                            lobby.HubModel.Users.Count,
                            lobby.HubModel.IsGameStarted
                        )
                    );
                }
            }
            return JsonSerializer.Serialize(toJsonList);
        }

        public bool CheckNotDefaultHubIsEmpty()
        {
            if (this.Users.Count < 1 && this.IsDefault == false)
            {
                return true;

            }
            else
            {
                return false;
            }
        }
    }
}
