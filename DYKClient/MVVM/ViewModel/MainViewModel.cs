using DYKClient.Core;
using DYKClient.MVVM.Model;
using DYKClient.MVVM.ViewModel.GameHistoryViewModels;
using DYKClient.Net;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace DYKClient.MVVM.ViewModel
{
    class MainViewModel : ObservableObject
    {
        public ObservableCollection<UserModel> Users { get; set; }
        public ObservableCollection<string> Messages { get; set; }
        public RelayCommand SendMessageCommand { get; set; }
        public RelayCommand HelpViewCommand { get; set; }
        public RelayCommand SummariesListViewCommand { get; set; }
        public RelayCommand AboutViewCommand { get; set; }
        public RelayCommand LobbiesViewCommand { get; set; }
        public HelpViewModel HelpViewModel { get; set; }
        public SummariesListViewModel SummariesListViewModel { get; set; }
        public AboutViewModel AboutViewModel { get; set; }
        public LobbiesViewModel LobbiesViewModel { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
        public Server _server;

        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                onPropertyChanged();
            }
        }

        private bool _menuRadios = true;
        public bool MenuRadios
        {
            get { return _menuRadios; }
            set
            {
                _menuRadios = value;
                onPropertyChanged();
            }
        }

        public MainViewModel()
        {
            InitializeConnectionToServer();
            InitializeViewCommands();
        }

        private void InitializeConnectionToServer()
        {
            Users = new ObservableCollection<UserModel>();
            Messages = new ObservableCollection<string>();
            _server = GlobalClass.Server;
            _server.messageEvent += MessageReceived;
            _server.userDisconnectedEvent += RemoveUser;
            SendMessageCommand = new RelayCommand(o => _server.SendMessageToServer(Message), o => string.IsNullOrEmpty(Message) == false);
        }

        private void InitializeViewCommands()
        {
            HelpViewModel = new HelpViewModel();
            AboutViewModel = new AboutViewModel();
            SummariesListViewModel = new SummariesListViewModel(this);
            LobbiesViewModel = new LobbiesViewModel(this);

            _server.SendOpCodeToServer(OpCodes.SendLobbiesList);
            CurrentView = LobbiesViewModel;

            LobbiesViewCommand = new RelayCommand(o =>
            {
                CurrentView = LobbiesViewModel;
                _server.SendOpCodeToServer(OpCodes.SendLobbiesList);
            });

            AboutViewCommand = new RelayCommand(o =>
            {
                CurrentView = AboutViewModel;
            });

            HelpViewCommand = new RelayCommand(o =>
            {
                CurrentView = HelpViewModel;
            });

            SummariesListViewCommand = new RelayCommand(o =>
            {
                CurrentView = SummariesListViewModel;
                _server.SendOpCodeToServer(OpCodes.SendGamesHistoriesList);
            });
        }

        private void RemoveUser()
        {
            var uid = _server.PacketReader.ReadMessage();
            var user = Users.Where(x => x.UID.ToString() == uid).FirstOrDefault();
            Application.Current.Dispatcher.Invoke(() => Users.Remove(user));
        }

        private void MessageReceived()
        {
            var msg = _server.PacketReader.ReadMessage();
            Application.Current.Dispatcher.Invoke(() => Messages.Add(msg));
        }
    }
}
