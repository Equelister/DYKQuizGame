using DYKClient.Core;
using DYKClient.MVVM.Model;
using DYKClient.MVVM.ViewModel.GameViewModels;
using DYKClient.Net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DYKClient.MVVM.ViewModel
{
    class MainViewModel : ObservableObject
    {
        public ObservableCollection<UserModel> Users { get; set; }
        public ObservableCollection<string> Messages { get; set; }
        //public RelayCommand ConnectToServerCommand { get; set; }
        public RelayCommand SendMessageCommand { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
        public Server _server;


        public RelayCommand HelpViewCommand { get; set; }
        public RelayCommand AboutViewCommand { get; set; }
        public RelayCommand LobbiesViewCommand { get; set; }
        public HelpViewModel HelpViewModel { get; set; }
        public AboutViewModel AboutViewModel { get; set; }
        public LobbiesViewModel LobbiesViewModel { get; set; }
        private object _currentView;

/*        public RelayCommand NewLobbyViewCommand { get; set; }
        public RelayCommand ConnectToLobbyViewCommand { get; set; }

        public LobbyViewModel LobbyViewModel { get; set; }*/


        public object CurrentView
        { 
            get { return _currentView; }
            set { _currentView = value;
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
            //_server = new Server();
            //GlobalClass gc = new GlobalClass();
            _server = GlobalClass.Server;
            //_server.connectedEvent += UserConnected;
            _server.messageEvent += MessageReceived;
            _server.userDisconnectedEvent += RemoveUser;
            //ConnectToServerCommand = new RelayCommand(o => _server.ConnectToServer(Username), o => string.IsNullOrEmpty(Username) == false);
            SendMessageCommand = new RelayCommand(o => _server.SendMessageToServer(Message), o => string.IsNullOrEmpty(Message) == false);
        }

        private void InitializeViewCommands()
        {
            HelpViewModel = new HelpViewModel();
            AboutViewModel = new AboutViewModel();
            //CurrentView = LobbiesViewModel;
            LobbiesViewModel = new LobbiesViewModel(this);

            LobbiesViewCommand = new RelayCommand(o =>
            {
                //LobbiesViewModel.ReceivedPublicLobbiesList();

                //_server.SendOpCodeToServer(Convert.ToByte(OpCodes.SendLobbiesList));
                //LobbiesViewModel.ReceivedPublicLobbiesList();
                CurrentView = LobbiesViewModel;
            });

            AboutViewCommand = new RelayCommand(o =>
            {
                CurrentView = AboutViewModel;
            });

            HelpViewCommand = new RelayCommand(o =>
            {
                CurrentView = HelpViewModel;
            });

            /*     LobbyViewModel = new LobbyViewModel();

                 NewLobbyViewCommand = new RelayCommand(o =>
                 {
                     CurrentView = LobbyViewModel;
                 });

                 ConnectToLobbyViewCommand = new RelayCommand(o =>
                 {
                     CurrentView = LobbyViewModel;
                 });*/

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

      /*  private void UserConnected()
        {
            UserModel user = new UserModel
            {
                Username = _server.PacketReader.ReadMessage(),
                UID = _server.PacketReader.ReadMessage()
            };

            if (Users.Any(x => x.UID == user.UID) == false)
            {
                Application.Current.Dispatcher.Invoke(() => Users.Add(user));
            }
        }*/

    }
}
