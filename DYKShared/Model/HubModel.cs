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
        public int MaxSize { get; set; }

        public HubModel(string name, int joinCode)
        {
            Name = name;
            JoinCode = joinCode;
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
            var jsonData = JsonSerializer.Deserialize<HubModel>(json);
            return jsonData;
        }

        public static ObservableCollection<HubModel> JsonListToHubModelObservableCollection(string json)
        {
            //List<HubModel> lobbies = new List<HubModel>();
            var jsonData = JsonSerializer.Deserialize<ObservableCollection<HubModel>>(json);
            return jsonData;
        }
    }
}
