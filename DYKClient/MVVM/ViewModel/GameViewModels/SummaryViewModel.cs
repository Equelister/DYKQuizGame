using DYKClient.Core;
using DYKShared.Model;
using System;
using System.Collections.ObjectModel;

namespace DYKClient.MVVM.ViewModel.GameViewModels
{
    class SummaryViewModel : ObservableObject
    {
        private bool _gameInProgress = true;
        public bool GameInProgress
        {
            get
            {
                return _gameInProgress;
            }
            set
            {
                _gameInProgress = value;
                onPropertyChanged("GameInProgress");
                onPropertyChanged("IsUserWaitingVisibility");
                onPropertyChanged("IsGameEndedVisibility");
            }
        }

        private bool _isUserAWinner = false;
        public bool IsUserAWinner
        {
            get
            {
                return _isUserAWinner;
            }
            set
            {
                _isUserAWinner = value;
                onPropertyChanged("IsUserAWinner");
                onPropertyChanged("IsUserAWinnerVisibility");
            }
        }

        public System.Windows.Visibility IsUserWaitingVisibility
        {
            get
            {
                return GameInProgress ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
        }

        public System.Windows.Visibility IsGameEndedVisibility
        {
            get
            {
                return GameInProgress ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            }
        }

        public System.Windows.Visibility IsUserAWinnerVisibility
        {
            get
            {
                return IsUserAWinner ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
        }

        private ObservableCollection<SummaryModel> _summary = new ObservableCollection<SummaryModel>();
        public ObservableCollection<SummaryModel> Summary
        {
            get { return _summary; }
            set
            {
                _summary = value;
                onPropertyChanged("Summary");
            }
        }

        public RelayCommand GoBackToLobbyCommand { get; set; }
        private MainViewModel mainViewModel;

        public SummaryViewModel(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;

            mainViewModel._server.getGameSummaryEvent += DisplaySummary;
            mainViewModel._server.getIsUserAWinnerEvent += ChangeStarVisibility;
            GoBackToLobbyCommand = new RelayCommand(o =>
            {
                if (GameInProgress == false)
                {
                    mainViewModel._server.getGameSummaryEvent -= DisplaySummary;
                    mainViewModel._server.getIsUserAWinnerEvent -= ChangeStarVisibility;
                    mainViewModel.CurrentView = mainViewModel.LobbiesViewModel.LobbyViewModel;
                }
            });
        }

        public SummaryViewModel(MainViewModel mainViewModel, string summary)
        {
            this.mainViewModel = mainViewModel;
            mainViewModel.MenuRadios = false;
            DisplaySummaryFromHistory(summary);
            GoBackToLobbyCommand = new RelayCommand(o =>
            {
                mainViewModel.CurrentView = mainViewModel.SummariesListViewModel;
                mainViewModel.MenuRadios = true;
            });
        }

        private void DisplaySummaryFromHistory(string msg)
        {
            Summary = SummaryModel.JsonToObservableCollection(msg);
            onPropertyChanged("Summary");
            GameInProgress = false;
        }

        private void DisplaySummary()
        {
            var msg = mainViewModel._server.PacketReader.ReadMessage();
            Console.WriteLine("\r\n OTO PODSUMOWANIE: " + msg + "\r\n");
            Summary = SummaryModel.JsonToObservableCollection(msg);
            onPropertyChanged("Summary");
            GameInProgress = false;
        }

        private void ChangeStarVisibility()
        {
            IsUserAWinner = true;
        }
    }
}
