﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DYKShared.Model
{
    public class SummaryModel
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        public string FastestAnswer 
        { 
            get
            {
                return $"{this.FastestAnswerName} - [{this.FastestAnswerLong}ms]"; 
            }
        }        //"User001 [132ms]"  
        public string FastestAnswerName { get; set; }        //"User001 [132ms]"  
        public long FastestAnswerLong{ get; set; }        //"User001 [132ms]"  
        public List<string> UsersNicknames { get; set; } //Those with CorrectAnswer

        public SummaryModel()
        {             
        }

        public SummaryModel(string question, string correctanswer, string fastestanswer, long fastestAnswerLong, List<string> nicknames)
        {
            Question = question;
            Answer = correctanswer;
            FastestAnswerName = fastestanswer;
            UsersNicknames = nicknames;
            FastestAnswerLong = fastestAnswerLong;
        }

        public SummaryModel(string question, string correctanswer, string fastestanswer, long fastestAnswerLong)
        {
            Question = question;
            Answer = correctanswer;
            FastestAnswerName = fastestanswer;
            FastestAnswerLong = fastestAnswerLong;
            UsersNicknames = new List<string>();
        }



        public static List<SummaryModel> JsonToList(string json)
        {
            var jsonData = JsonSerializer.Deserialize<List<SummaryModel>>(json);
            return jsonData;
        }

        public static SummaryModel JsonToSingleObject(string json)
        {
            Console.WriteLine(json);
            var jsonData = JsonSerializer.Deserialize<SummaryModel>(json);
            return jsonData;
        }

        public static ObservableCollection<SummaryModel> JsonToObservableCollection(string json)
        {
            var jsonData = JsonSerializer.Deserialize<ObservableCollection<SummaryModel>>(json);
            return jsonData;
        }

        public string ConvertToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
