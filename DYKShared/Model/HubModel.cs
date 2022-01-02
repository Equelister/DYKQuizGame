using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DYKShared.Model
{
    public class HubModel : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public int JoinCode { get; set; }
        public List<UserModel> Users { get; set; }
        public CategoryModel Category { get; set; }
        public int MaxSize { get; set; }
        public int PlayerCount { get; set; }
        public int PlayersThatEndedGame { get; set; }
        public int GameRound { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsGameStarted { get; set; }
        public string LobbySlotsStr
        {
            get
            {
                return $"{this.PlayerCount}/{this.MaxSize}";
            }
        }

        public HubModel(string name, int joinCode, CategoryModel category)
        {
            Name = name;
            JoinCode = joinCode;
            Category = category;
            Users = new List<UserModel>();
            IsGameStarted = false;
            PlayersThatEndedGame = 0;
            GameRound = 0;
        }
        
        public HubModel(int joinCode, int maxSize, string name, CategoryModel category, bool isPrivate)
        {
            Name = name;
            JoinCode = joinCode;
            Users = new List<UserModel>();
            Category = category;
            MaxSize = maxSize;
            IsPrivate = isPrivate;
            IsGameStarted = false;
            PlayersThatEndedGame = 0;
            GameRound = 0;
        }

        public HubModel(CategoryModel category)
        {
            Users = new List<UserModel>();
            Category = category;
            IsGameStarted = false;
            PlayersThatEndedGame = 0; 
            GameRound = 0;
        }

        public HubModel()
        {
        }

        public HubModel(string name, int joinCode, CategoryModel category, int maxSize, int playerCount, bool isGameStarted) : this(name, joinCode, category)
        {
            MaxSize = maxSize;
            PlayerCount = playerCount;
            IsGameStarted = isGameStarted;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static List<HubModel> JsonListToHubModelList(string json)
        {
            //List<HubModel> lobbies = new List<HubModel>();
            var jsonData = JsonSerializer.Deserialize<List<HubModel>>(json);
            return jsonData;
        }

        public static HubModel JsonToSingleLobby(string json)
        {
            Console.WriteLine(json);
            var jsonData = JsonSerializer.Deserialize<HubModel>(json);
            return jsonData;
        }

        public static ObservableCollection<HubModel> JsonListToHubModelObservableCollection(string json)
        {
            //List<HubModel> lobbies = new List<HubModel>();
            var jsonData = JsonSerializer.Deserialize<ObservableCollection<HubModel>>(json);
            return jsonData;
        }

        public string ConvertToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
