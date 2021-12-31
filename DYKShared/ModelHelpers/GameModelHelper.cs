using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DYKShared.ModelHelpers
{
    public class GameModelHelper
    {
        public int ID { get; set; }
        public DateTime CreatedAt { get; set; }

        public GameModelHelper(int id, DateTime date)
        {
            this.ID = id;
            this.CreatedAt = date;
        }

        public GameModelHelper()
        {

        }

        public static ObservableCollection<GameModelHelper> JsonListToObservableCollection(string json)
        {
            var jsonData = JsonSerializer.Deserialize<ObservableCollection<GameModelHelper>>(json);
            return jsonData;
        }
    }
}
