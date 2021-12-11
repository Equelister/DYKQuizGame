using DYKClient.Core;
using DYKShared.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
                onPropertyChanged();
            }
        }
        
        public string AnswerA
        {
            get { return AnswerA; }
            set
            {
                AnswerA = value;
                onPropertyChanged();
            }
        }
        public string AnswerB
        {
            get { return AnswerB; }
            set
            {
                AnswerB = value;
                onPropertyChanged();
            }
        }
        public string AnswerC
        {
            get { return AnswerC; }
            set
            {
                AnswerC = value;
                onPropertyChanged();
            }
        }
        public string AnswerD
        {
            get { return AnswerD; }
            set
            {
                AnswerD = value;
                onPropertyChanged();
            }
        }


        public RelayCommand UserSelectedAnswerCommand { get; set; }
        private MainViewModel mainViewModel;
        private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        public GameViewModel(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
            Run();            
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

            if (currentQuestionIndex++ > Questions.Count)
            {
                GoToSummaryViewAsync();
            }
            else
            {
                ShowQuestion(++currentQuestionIndex);
            }
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
            Questions = QuestionModel.JsonListToQuestionModelObservableCollection(msg);
            onPropertyChanged("Questions");
        }
    }
}
