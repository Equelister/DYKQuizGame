using DYKShared.Enums;
using DYKShared.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DYKClient.Controller
{
    public static class QuestionEnhancer
    {
        internal static ObservableCollection<QuestionModel> DeleteLettersAnswers(ObservableCollection<QuestionModel> questions)
        {
            foreach(var question in questions)
            {
                question.CorrectAnswer = DeletePercentageOfText(question.CorrectAnswer, 0.15);
                question.WrongAnswerA = DeletePercentageOfText(question.WrongAnswerA, 0.15);
                question.WrongAnswerB = DeletePercentageOfText(question.WrongAnswerB, 0.15);
                question.WrongAnswerC = DeletePercentageOfText(question.WrongAnswerC, 0.15);
            }
            return questions;
        }

        internal static ObservableCollection<QuestionModel> DeleteLettersQuestions(ObservableCollection<QuestionModel> questions)
        {
            foreach(var question in questions)
            {
                question.Question = DeletePercentageOfText(question.Question, 0.15);
            }
            return questions;
        }

        internal static ObservableCollection<QuestionModel> SwitchLettersAnswers(ObservableCollection<QuestionModel> questions)
        {
            foreach (var question in questions)
            {
                question.CorrectAnswer = SwitchLettersForEachWord(question.CorrectAnswer);
                question.WrongAnswerA = SwitchLettersForEachWord(question.WrongAnswerA);
                question.WrongAnswerB = SwitchLettersForEachWord(question.WrongAnswerB);
                question.WrongAnswerC = SwitchLettersForEachWord(question.WrongAnswerC);
            }
            return questions;
        }

        internal static ObservableCollection<QuestionModel> SwitchLettersQuestions(ObservableCollection<QuestionModel> questions)
        {
            foreach (var question in questions)
            {
                question.Question = SwitchLettersForEachWord(question.Question);
            }
            return questions;
        }

        private static string DeletePercentageOfText(string text, double percentage)
        {
            StringBuilder sb = new StringBuilder(text);         
            int length = sb.Length;
            if (length > 2)
            {
                double toDelete = (double)Math.Ceiling(length * (double)(percentage));
                Random rand = new Random();
                for (int i = 0; i < toDelete; i++)
                {
                    int place = rand.Next(sb.Length);
                    sb.Remove(place, 1);
                    sb.Insert(place, '_');
                }
            }
            return sb.ToString();
        }

        private static string SwitchLettersForEachWord(string text)
        {
            StringBuilder sb = new StringBuilder(text.Length);
            string[] stringArray = text.Split(' ');
            foreach(var part in stringArray)
            {
                if (part.Length > 1)
                {
                    sb.Append(part.Substring(part.Length - 1, 1));
                    if (part.Length > 2)
                    {
                        sb.Append(part.Substring(1, part.Length - 2));
                    }
                    sb.Append(part.Substring(0, 1));                    
                }else
                {
                    sb.Append(part);
                }
                sb.Append(' ');
            }
            return sb.ToString();
        }
    }
}
