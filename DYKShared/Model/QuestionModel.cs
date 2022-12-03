using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace DYKShared.Model
{
    public class QuestionModel
    {
        public int ID { get; set; }
        public string Question { get; set; }
        public string CorrectAnswer { get; set; }
        public string WrongAnswerA { get; set; }
        public string WrongAnswerB { get; set; }
        public string WrongAnswerC { get; set; }
        public long AnswerTimeMS { get; set; }
        public bool IsAnsweredCorrectly { get; set; }

        public QuestionModel()
        {
        }

        public QuestionModel(
            string question,
            string correctAnswer,
            string wrongAnswerA,
            string wrongAnswerB,
            string wrongAnswerC,
            int id
            )
        {
            Question = question;
            CorrectAnswer = correctAnswer;
            WrongAnswerA = wrongAnswerA;
            WrongAnswerB = wrongAnswerB;
            WrongAnswerC = wrongAnswerC;
            ID = id;
        }

        public QuestionModel(string question, string answer)
        {
            this.Question = question;
            this.CorrectAnswer = answer;
        }

        public static List<QuestionModel> JsonListToQuestionModelList(string json)
        {
            var jsonData = JsonSerializer.Deserialize<List<QuestionModel>>(json);
            return jsonData;
        }

        public static QuestionModel JsonToSingleQuestion(string json)
        {
            Console.WriteLine(json);
            var jsonData = JsonSerializer.Deserialize<QuestionModel>(json);
            return jsonData;
        }

        public static ObservableCollection<QuestionModel> JsonListToQuestionModelObservableCollection(string json)
        {
            var jsonData = JsonSerializer.Deserialize<ObservableCollection<QuestionModel>>(json);
            return jsonData;
        }

        public string ConvertToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
