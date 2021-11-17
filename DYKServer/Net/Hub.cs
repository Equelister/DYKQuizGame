using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DYKServer.Net
{
    class Hub
    {

        public Guid GUID { get; set; }
        public string Name { get; set; }
        public List<Client> Users { get; set; }
        public int MaxSize { get; set; }
        public int JoinCode { get; set; }
        public bool IsPrivate {get;set;}

        public Hub(string name)
        {
            GUID = Guid.NewGuid();
            Name = name;
            Users = new List<Client>();
            MaxSize = 8;
            JoinCode = GenerateJoinCode();
            IsPrivate = false;
        }

        public Hub(string name, int maxSize, bool isPrivate)
        {
            GUID = Guid.NewGuid();
            Name = name;
            Users = new List<Client>();
            MaxSize = maxSize;
            JoinCode = GenerateJoinCode();
            IsPrivate = isPrivate;
        }

        public int GenerateJoinCode()
        {
            return new Random().Next(9000)+1000;
        }

        public bool AddClient(Client client)
        {
            if(Users.Count <= MaxSize)
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
            var toJsonList = new List<DYKShared.Model.HubModel>();
            foreach (var lobby in lobbies)
            {
                if (lobby.IsPrivate == false)
                {
                    toJsonList.Add(new DYKShared.Model.HubModel(lobby.Name, lobby.JoinCode));
                }
            }
            return JsonSerializer.Serialize(toJsonList);
        }


    }
}
