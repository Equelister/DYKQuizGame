using System.Collections.ObjectModel;
using System.Text.Json;

namespace DYKShared.Model
{
    public class CategoryModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public CategoryModel(int id, string name, string description)
        {
            ID = id;
            Name = name;
            Description = description;
        }

        public CategoryModel()
        {

        }

        public static ObservableCollection<CategoryModel> JsonListToCategoryModelObservableCollection(string json)
        {
            var jsonData = JsonSerializer.Deserialize<ObservableCollection<CategoryModel>>(json);
            return jsonData;
        }

        public string ConvertToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
