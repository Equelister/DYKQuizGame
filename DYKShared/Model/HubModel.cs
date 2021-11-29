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
        public bool IsPrivate { get; set; }

        public HubModel(string name, int joinCode, CategoryModel category)
        {
            Name = name;
            JoinCode = joinCode;
            Users = new List<UserModel>();
            Category = category;
        }
        
        public HubModel(int joinCode, int maxSize, string name, CategoryModel category, bool isPrivate)
        {
            Name = name;
            JoinCode = joinCode;
            Users = new List<UserModel>();
            Category = category;
            MaxSize = maxSize;
            IsPrivate = isPrivate;
        }

        public HubModel(CategoryModel category)
        {
            Users = new List<UserModel>();
            Category = category;
        }

        public HubModel()
        {
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
