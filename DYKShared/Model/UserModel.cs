using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;

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
        public List<int> AppliedEnhancementsIDs { get; set; }

        public UserModel()
        {
            IsReady = false;
            GameScore = 0;
            AppliedEnhancementsIDs = new List<int>();
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
