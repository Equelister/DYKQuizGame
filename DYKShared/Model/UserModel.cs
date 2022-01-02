using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DYKShared.Model
{
    public class UserModel
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public int Total_games { get; set; }
        public bool IsReady { get; set; }
        public int GameScore { get; set; }
        public List<InGameActions> AppliedEnhancements { get; set; }

        public UserModel()
        {
            IsReady = false;
            GameScore = 0;
            AppliedEnhancements = new List<InGameActions>();
        }

        public UserModel(string username) : this()
        {
            Username = username;
        }

        public static List<UserModel> JsonListToUserModelList(string json)
        {
            var jsonData = JsonSerializer.Deserialize<List<UserModel>>(json);
            return jsonData;
        }

        public static ObservableCollection<UserModel> JsonListToUserModelObservableCollection(string json)
        {
            var jsonData = JsonSerializer.Deserialize<ObservableCollection<UserModel>>(json);
            return jsonData;
        }

        public string ConvertToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
