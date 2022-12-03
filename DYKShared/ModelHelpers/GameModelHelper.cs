using System;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace DYKShared.ModelHelpers
{
    public class GameModelHelper
    {
        public int ID { get; set; }
        public DateTime CreatedAt { get; set; }

        public GameModelHelper()
        {
        }

        public GameModelHelper(int id, DateTime date)
        {
            this.ID = id;
            this.CreatedAt = date;
        }

        public static ObservableCollection<GameModelHelper> JsonListToObservableCollection(string json)
        {
            var jsonData = JsonSerializer.Deserialize<ObservableCollection<GameModelHelper>>(json);
            return jsonData;
        }
    }
}
