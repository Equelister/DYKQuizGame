using DYKClient.Core;
using DYKShared.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DYKClient.MVVM.ViewModel.GameViewModels
{
    class GameViewModel : ObservableObject
    {
        private ObservableCollection<QuestionModel> _questions = new ObservableCollection<QuestionModel>();
        public ObservableCollection<QuestionModel> Questions
        {
            get { return _questions; }
            set
            {
                _questions = value;
                onPropertyChanged();
            }
        }
        
        private QuestionModel _currentQuestion = new QuestionModel();
        public QuestionModel CurrentQuestion
        {
            get { return _currentQuestion; }
            set
            {
                _currentQuestion = value;
                onPropertyChanged("CurrentQuestion");
            }
        }

        private string _answerA = "";
        public string AnswerA
        {
            get { return _answerA; }
            set
            {
                _answerA = value;
                onPropertyChanged();
            }
        }

        private string _answerB = "";
        public string AnswerB
        {
            get { return _answerB; }
            set
            {
                _answerB = value;
                onPropertyChanged();
            }
        }

        private string _answerC = "";
        public string AnswerC
        {
            get { return _answerC; }
            set
            {
                _answerC = value;
                onPropertyChanged("AnswerC");
            }
        }

        private string _answerD = "";
        public string AnswerD
        {
            get { return _answerD; }
            set
            {
                _answerD = value;
                onPropertyChanged("AnswerD");
            }
        }


        public RelayCommand UserSelectedAnswerCommand { get; set; }
        private MainViewModel mainViewModel;
        private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        public GameViewModel(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
           /* Task.Run(() =>
            {*/
                //System.Threading.Thread.Sleep(5000);
                Run();
           // });
        }

        private async Task Run()
        {
            ReadQuestions();
            UserSelectedAnswerCommand = new RelayCommand(SumQuestion);        
            ShowQuestion(0);
        }

        private void SumQuestion(object param)
        {
            string userChosedAnswer = param as string;
            GetAnsweredTime();
            GetAnsweredResult(userChosedAnswer);

            if (++currentQuestionIndex >= Questions.Count)
            {
                string message = CreateQuestionsSummaryToSend();
                mainViewModel._server.SendMessageToServerOpCode(message, Net.OpCodes.SendIHaveEndedGame);
                GoToSummaryViewAsync();
            }
            else
            {
                ShowQuestion(currentQuestionIndex);
            }
        }

        private string CreateQuestionsSummaryToSend()
        {
            foreach(var question in Questions)
            {
                question.Question = null;
                question.CorrectAnswer = null;
                question.WrongAnswerA = null;
                question.WrongAnswerB = null;
                question.WrongAnswerC = null;
            }
            return JsonSerializer.Serialize(Questions);          
        }

        private SummaryViewModel summaryViewModel;
        private async Task GoToSummaryViewAsync()
        {
            summaryViewModel = null;
            summaryViewModel = new SummaryViewModel(mainViewModel);
            mainViewModel.CurrentView = summaryViewModel;
        }

        private void GetAnsweredTime()
        {
            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            Questions.ElementAt(currentQuestionIndex).AnswerTimeMS = time;
        }

        private void GetAnsweredResult(string userChosedAnswer)
        {
            if (userChosedAnswer.Equals(CurrentQuestion.CorrectAnswer))
            {
                Questions.ElementAt(currentQuestionIndex).IsAnsweredCorrectly = true;
            }
            else
            {
                Questions.ElementAt(currentQuestionIndex).IsAnsweredCorrectly = false;
            }
        }

        private int currentQuestionIndex = 0;

        private void ShowQuestion(int index)
        {
            CurrentQuestion = Questions.ElementAt(index);
            List<string> answers = new List<string>();
            answers.Add(CurrentQuestion.CorrectAnswer);
            answers.Add(CurrentQuestion.WrongAnswerA);
            answers.Add(CurrentQuestion.WrongAnswerB);
            answers.Add(CurrentQuestion.WrongAnswerC);
            RandomizeAnswersList( ref answers);
            AnswerA = answers.ElementAt(0);
            AnswerB = answers.ElementAt(1);
            AnswerC = answers.ElementAt(2);
            AnswerD = answers.ElementAt(3); 
            stopwatch.Restart();
        }

        public void RandomizeAnswersList(ref List<string> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                string value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        private void ReadQuestions()
        {
            var msg = mainViewModel._server.PacketReader.ReadMessage();
            Console.WriteLine("\r\n OTO SA PYTANIA: " + msg + "\r\n");
            Questions = QuestionModel.JsonListToQuestionModelObservableCollection(msg);
            onPropertyChanged("Questions");
        }
    }
}
