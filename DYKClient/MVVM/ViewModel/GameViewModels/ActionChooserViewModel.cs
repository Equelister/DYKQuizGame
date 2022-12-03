using DYKClient.Core;
using DYKShared.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace DYKClient.MVVM.ViewModel.GameViewModels
{
    class ActionChooserViewModel : ObservableObject
    {
        public RelayCommand PickedActionAndUserCommand { get; set; }

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

        private ObservableCollection<InGameActions> _enhancements = new ObservableCollection<InGameActions>();
        public ObservableCollection<InGameActions> Enhancements
        {
            get { return _enhancements; }
            set
            {
                _enhancements = value;
                onPropertyChanged();
            }
        }

        private InGameActions _selectedEnhancement;
        public InGameActions SelectedEnhancement
        {
            get
            {
                if (_selectedEnhancement is null)
                {
                    return null;
                }
                return _selectedEnhancement;
            }
            set
            {
                _selectedEnhancement = value;
                onPropertyChanged("SelectedEnhancement");
            }
        }

        private ObservableCollection<UserModel> _users = new ObservableCollection<UserModel>();
        public ObservableCollection<UserModel> Users
        {
            get { return _users; }
            set
            {
                _users = value;
                onPropertyChanged();
            }
        }

        private UserModel _selectedUser;
        public UserModel SelectedUser
        {
            get
            {
                if (_selectedUser is null)
                {
                    return null;
                }
                return _selectedUser;
            }
            set
            {
                _selectedUser = value;
                onPropertyChanged("SelectedUser");
            }
        }


        public List<InGameActions> PickedActions = new List<InGameActions>();
        private MainViewModel mainViewModel;
        private GameViewModel gameViewModel;
        private List<int> EnhancementsForMe = new List<int>();

        public ActionChooserViewModel(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
            InitializeEvents();
            InitializeCommands();
        }

        private void InitializeEvents()
        {
            mainViewModel._server.receivedACEnhancementsEvent += ReceivedEnhancementList;
            mainViewModel._server.receivedACUsersEvent += ReceivedUsersList;
            mainViewModel._server.startEnhancendGameNextRoundEvent += EnhancedGameChangeView;
            mainViewModel._server.getGameSummaryEvent += DisplaySummary;
            mainViewModel._server.receivedSendSecondHalfQuestionsEvent += ChangeViewForRoundTwo;
        }

        private void ChangeViewForRoundTwo()
        {
            gameViewModel = null;
            gameViewModel = new GameViewModel(mainViewModel, 1, DYKShared.Enums.GameTypes.EnhancedQuizGame, EnhancementsForMe);
            mainViewModel.CurrentView = gameViewModel;
            mainViewModel._server.receivedSendSecondHalfQuestionsEvent -= ChangeViewForRoundTwo;
            UnInitializeViewChangerEvent();
        }

        private void DisplaySummary()
        {
            throw new NotImplementedException();
        }

        private void UnInitializeViewChangerEvent()
        {
            mainViewModel._server.startEnhancendGameNextRoundEvent -= EnhancedGameChangeView;
        }

        private void UnInitializeEventsExceptViewChanger()
        {
            mainViewModel._server.receivedACEnhancementsEvent -= ReceivedEnhancementList;
            mainViewModel._server.receivedACUsersEvent -= ReceivedUsersList;
            mainViewModel._server.getGameSummaryEvent -= DisplaySummary;
        }

        private void InitializeCommands()
        {
            PickedActionAndUserCommand = new RelayCommand(o =>
            {
                ApplySelection();
            });
        }

        private void ReceivedEnhancementList()
        {
            var msg = mainViewModel._server.PacketReader.ReadMessage();
            Console.WriteLine("\r\n Enhancements: " + msg + "\r\n");
            Enhancements = InGameActions.JsonToObservableCollection(msg);
            onPropertyChanged("Enhancements");
            GameInProgress = false;
        }

        private void ReceivedUsersList()
        {
            var msg = mainViewModel._server.PacketReader.ReadMessage();
            Console.WriteLine("\r\n Enhancements: " + msg + "\r\n");
            Users = UserModel.JsonListToUserModelObservableCollection(msg);
            onPropertyChanged("Users");
        }

        private void EnhancedGameChangeView()
        {
            EnhancementsForMe = ReadMyEnhancements();
        }

        private List<int> ReadMyEnhancements()
        {
            var msg = mainViewModel._server.PacketReader.ReadMessage();
            Console.WriteLine("\r\n \r\n \r\n My Enhancements: " + msg + "\r\n \r\n \r\n");
            return JsonSerializer.Deserialize<List<int>>(msg);
        }

        private void ApplySelection()
        {
            if (SelectedEnhancement is not null && SelectedUser is not null)
            {
                SelectedEnhancement.UserNickname = SelectedUser.Username;
                PickedActions.Add(SelectedEnhancement);
                Enhancements.Remove(SelectedEnhancement);
                Users.Remove(SelectedUser);
                SelectedEnhancement = null;
                SelectedUser = null;
                if (Enhancements.Count <= 0)
                {
                    UnInitializeEventsExceptViewChanger();
                    string json = JsonSerializer.Serialize(PickedActions);
                    mainViewModel._server.SendMessageToServerOpCode(json, Net.OpCodes.SendPickedEnhancements);
                    PickedActions.Clear();
                }
            }
        }
    }
}
