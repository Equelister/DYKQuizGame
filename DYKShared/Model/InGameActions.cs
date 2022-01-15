using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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

        public InGameActions(int  id, string name, string desc, string username) : this(id, name, desc)
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
            list.Add(new InGameActions(0, "Question Switcher", "Switches first letter with last in every word in a question."));
            list.Add(new InGameActions(1, "Answer Switcher", "Switches first letter with last in every word in each answer."));
            list.Add(new InGameActions(2, "Question Remover", "Deletes 15% of letters in a question."));
            list.Add(new InGameActions(3, "Answer Remover", "Deletes 15% of letters in each answer."));
            list.Add(new InGameActions(4, "Multi-Clicker", "You need to click 5 times in order to select your answer."));
            //list.Add(new InGameActions(5, "Flashlight", "Shows answer only if you have your mouse cursor on it."));
            //list.Add(new InGameActions(6, "It Floats!", "Makes Answers float"));
            return list;
        }

        public static List<InGameActions> GetRandomAmoutOfActions(List<InGameActions> actionList ,int userCount)
        {
            List<InGameActions> resultList = new List<InGameActions>();
            Random rand = new Random();
            int numberOfActionsPerUser = (int)Math.Ceiling((userCount * (0.24)) > 1 ? (userCount * (0.24) + 0.1) : (userCount * (0.24))); // 1 action for 2-4 users, 2 for 5-7, 3 for 8
            for(int i = 0; i<numberOfActionsPerUser; i++)
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
