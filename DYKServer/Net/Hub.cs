using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using DYKShared.Model;

namespace DYKServer.Net
{
    class Hub
    {

        public Guid GUID { get; set; }
        public HubModel HubModel { get; set; }
        public List<Client> Users { get; set; }
        public bool IsDefault = false;

        public Hub(HubModel hubModel)
        {
            GUID = Guid.NewGuid();
            HubModel = hubModel;
        }

        public Hub(string name, CategoryModel category)
        {
            GUID = Guid.NewGuid();
            Users = new List<Client>();
            HubModel = new HubModel(
                GenerateJoinCode(),
                8,
                name,
                category,
                false
            );
        }

        public Hub(string name, int maxSize, bool isPrivate, CategoryModel category)
        {
            GUID = Guid.NewGuid();
            Users = new List<Client>();
            HubModel = new HubModel(
                GenerateJoinCode(),
                maxSize,
                name,
                category,
                isPrivate
            );
        }

        public int GenerateJoinCode()
        {
            return new Random().Next(9000)+1000;
        }

        public bool AddClient(Client client)
        {
            if(Users.Count < HubModel.MaxSize)
            {
                Users.Add(client);
                return true;
            }
            else
            {
                Console.WriteLine($"Client [{client.GUID}] tried to connect into hub [{this.GUID}] but it was full.");
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
                    toJsonList.Add(new HubModel(lobby.HubModel.Name, lobby.HubModel.JoinCode, lobby.HubModel.Category));
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
