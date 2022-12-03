using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;

namespace DYKShared.Model
{
    public class InGameActions
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? UserNickname { get; set; }

        public InGameActions()
        {
        }

        private InGameActions(int id, string name, string desc)
        {
            ID = id;
            Name = name;
            Description = desc;
        }

        public InGameActions(int id, string name, string desc, string username) : this(id, name, desc)
        {
            UserNickname = username;
        }

        public InGameActions(InGameActions enhancement, string username)
        {
            ID = enhancement.ID;
            Name = enhancement.Name;
            Description = enhancement.Description;
            UserNickname = username;
        }

        public static List<InGameActions> GenerateActions()
        {
            List<InGameActions> list = new List<InGameActions>();
            list.Add(new InGameActions(0, "Zmieniacz pytań", "Zamienia pierwszą literę z ostatnią w każdym słowie pytania."));
            list.Add(new InGameActions(1, "Zmieniacz odpowiedzi", "Zamienia pierwszą literę z ostatnią w każdym słowie odpowiedzi."));
            list.Add(new InGameActions(2, "Usuwacz pytań", "Usuwa 15% liter z pytania."));
            list.Add(new InGameActions(3, "Usuwacz odpowiedzi", "Usuwa 15% liter z odpowiedzi."));
            list.Add(new InGameActions(4, "Klikacz", "Musisz kliknąć 5 razy, by wybrać prawidłową odpowiedź."));
            return list;
        }

        public static List<InGameActions> GetRandomAmoutOfActions(List<InGameActions> actionList, int userCount)
        {
            List<InGameActions> resultList = new List<InGameActions>();
            Random rand = new Random();
            int numberOfActionsPerUser = (int)Math.Ceiling((userCount * (0.24)) > 1 ? (userCount * (0.24) + 0.1) : (userCount * (0.24)));
            for (int i = 0; i < numberOfActionsPerUser; i++)
            {
                int random = rand.Next(actionList.Count);
                if (resultList.Contains(actionList.ElementAt(random)))
                {
                    i--;
                }
                else
                {
                    resultList.Add(actionList.ElementAt(random));
                }
            }
            return resultList;
        }

        public static List<InGameActions> JsonToList(string json)
        {
            var jsonData = JsonSerializer.Deserialize<List<InGameActions>>(json);
            return jsonData;
        }

        public static ObservableCollection<InGameActions> JsonToObservableCollection(string json)
        {
            var jsonData = JsonSerializer.Deserialize<ObservableCollection<InGameActions>>(json);
            return jsonData;
        }
    }
}
