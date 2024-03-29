﻿using DYKClient.Controller;
using DYKClient.Core;
using DYKShared.Enums;
using DYKShared.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        private int _answerHitCounter = 1;
        public int AnswerHitCounter
        {
            get { return _answerHitCounter; }
            set
            {
                _answerHitCounter = value;
                onPropertyChanged("AnswerHitCounter");
            }
        }

        public System.Windows.Visibility IsAnswerHitCounterVisible
        {
            get
            {
                return AnswerHitCounter > 1 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
        }

        public RelayCommand UserSelectedAnswerCommand { get; set; }
        private MainViewModel mainViewModel;
        private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        private int _gameRound;
        private bool _isHitCounterActive = false;
        private GameTypes _gameType;
        private List<int> Enhancements = new List<int>();
        private SummaryViewModel summaryViewModel;
        private ActionChooserViewModel actionChooserViewModel;
        private int currentQuestionIndex = 0;

        public GameViewModel(MainViewModel mainViewModel, int gameRound, GameTypes gameType)
        {
            this.mainViewModel = mainViewModel;
            _gameRound = gameRound;
            _gameType = gameType;
            Run();
        }

        public GameViewModel(MainViewModel mainViewModel, int gameRound, GameTypes gameType, List<int> inGameActions)
        {
            this.mainViewModel = mainViewModel;
            _gameRound = gameRound;
            _gameType = gameType;
            Enhancements = inGameActions;
            Run();
        }

        private async Task Run()
        {
            ReadQuestions();
            if (_gameType.Equals(GameTypes.EnhancedQuizGame))
            {
                EnhanceQuestions();
            }
            UserSelectedAnswerCommand = new RelayCommand(SumQuestion);
            ShowQuestion(0);
        }

        private void EnhanceQuestions()
        {
            if (_gameRound == 1)
            {
                Enhancements = Enhancements.Distinct().ToList();
                foreach (var enhancement in Enhancements)
                {
                    switch (enhancement)
                    {
                        case (int)InGameActionTypes.DeleteSomeLettersAnswers:
                            Questions = QuestionEnhancer.DeleteLettersAnswers(Questions);
                            break;
                        case (int)InGameActionTypes.DeleteSomeLettersQuestions:
                            Questions = QuestionEnhancer.DeleteLettersQuestions(Questions);
                            break;
                        case (int)InGameActionTypes.SwitchFirstWithLastLetterAnswers:
                            Questions = QuestionEnhancer.SwitchLettersAnswers(Questions);
                            break;
                        case (int)InGameActionTypes.SwitchFirstWithLastLetterQuestions:
                            Questions = QuestionEnhancer.SwitchLettersQuestions(Questions);
                            break;
                        case (int)InGameActionTypes.DisplayOnlyOnHoverAnswers:
                            break;
                        case (int)InGameActionTypes.HitIt5TimesAnswers:
                            _isHitCounterActive = true;
                            AnswerHitCounter = 5;
                            break;
                        case (int)InGameActionTypes.FloatingAnswers:
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void SumQuestion(object param)
        {
            if (_isHitCounterActive == false || AnswerHitCounter <= 0)
            {
                string userChosedAnswer = param as string;
                GetAnsweredTime();
                GetAnsweredResult(userChosedAnswer);
                if (_isHitCounterActive)
                {
                    AnswerHitCounter = 5;
                }
                if (++currentQuestionIndex >= Questions.Count)
                {
                    if (GameTypes.EnhancedQuizGame.Equals(_gameType))
                    {
                        if (_gameRound == 0)
                        {
                            string message = CreateQuestionsSummaryToSend();
                            mainViewModel._server.SendMessageToServerOpCode(message, Net.OpCodes.SendIHaveEndedGame);
                            GoToActionChooserView();
                        }
                        else if (_gameRound == 1)
                        {
                            string message = CreateQuestionsSummaryToSend();
                            mainViewModel._server.SendMessageToServerOpCode(message, Net.OpCodes.SendIHaveEndedGame);
                            GoToSummaryViewAsync();
                        }
                    }
                    else
                    {
                        string message = CreateQuestionsSummaryToSend();
                        mainViewModel._server.SendMessageToServerOpCode(message, Net.OpCodes.SendIHaveEndedGame);
                        GoToSummaryViewAsync();
                    }
                }
                else
                {
                    ShowQuestion(currentQuestionIndex);
                }
            }
            else
            {
                AnswerHitCounter--;
            }
        }

        private void GoToActionChooserView()
        {
            actionChooserViewModel = null;
            actionChooserViewModel = new ActionChooserViewModel(mainViewModel);
            mainViewModel.CurrentView = actionChooserViewModel;
        }

        private string CreateQuestionsSummaryToSend()
        {
            foreach (var question in Questions)
            {
                question.Question = null;
                question.CorrectAnswer = null;
                question.WrongAnswerA = null;
                question.WrongAnswerB = null;
                question.WrongAnswerC = null;
            }
            return JsonSerializer.Serialize(Questions);
        }

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

        private void ShowQuestion(int index)
        {
            CurrentQuestion = Questions.ElementAt(index);
            List<string> answers = new List<string>();
            answers.Add(CurrentQuestion.CorrectAnswer);
            answers.Add(CurrentQuestion.WrongAnswerA);
            answers.Add(CurrentQuestion.WrongAnswerB);
            answers.Add(CurrentQuestion.WrongAnswerC);
            RandomizeAnswersList(ref answers);
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
