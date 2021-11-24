using DYKClient.Core;
using DYKClient.MVVM.Model;
using DYKClient.MVVM.ViewModel.GameViewModels;
using DYKClient.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DYKShared.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Collections.Specialized;

namespace DYKClient.MVVM.ViewModel
{
    class LobbiesViewModel : ObservableObject, INotifyCollectionChanged
    {
        public RelayCommand NewLobbyViewCommand { get; set; }
        public RelayCommand ConnectToLobbyViewCommand { get; set; }
        public RelayCommand SendConnectToLobbyWithListCommand { get; set; }
        public RelayCommand SendConnectToLobbyReqCommand { get; set; }
        public RelayCommand ReceivedPublicLobbiesListCommand { get; set; }
        public RelayCommand SendLobbiesListRequestCommand { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public LobbyViewModel LobbyViewModel { get; set; }
        public ObservableCollection<HubModel> Hubs
        {
            get { return _Hubs; }
            set
            {
                _Hubs = value;
                onPropertyChanged();
            }
        }
        public HubModel SelectedHub
        {
            get
            {
                if (_selectedHub is null)
                {
                    _selectedHub = new HubModel();
                }
                return _selectedHub;
            }
            set
            {
                _selectedHub = value;
                onPropertyChanged("SelectedHub");
            }
        }
        public string JoinCode
        {
            get { return this.joinCode; }
            set
            {
                if (string.Equals(this.joinCode, value) == false)
                {
                    this.joinCode = value;
                    this.onPropertyChanged(this.joinCode);
                }
            }
        }
        private MainViewModel mainViewModel;
        private ObservableCollection<HubModel> _Hubs;
        private HubModel _selectedHub;
        private string joinCode;


        public LobbiesViewModel(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
            InitializeCommands();

        }

        public void ReceivedPublicLobbiesList()
        {
            var msg = mainViewModel._server.PacketReader.ReadMessage();
            Hubs = HubModel.JsonListToHubModelObservableCollection(msg);
        }

        private void InitializeCommands()
        {
            mainViewModel._server.receivedPublicLobbiesListEvent += ReceivedPublicLobbiesList;
            mainViewModel._server.connectToLobbyViewEvent += GetConnectToLobbyResult;

            NewLobbyViewCommand = new RelayCommand(o =>
            {
                mainViewModel.CurrentView = LobbyViewModel;
            });

            SendConnectToLobbyReqCommand = new RelayCommand(o =>
            {
                SendConnectToLobbyRequest(JoinCode);
            });

            SendConnectToLobbyWithListCommand = new RelayCommand(o =>
            {
                SendConnectToLobbyWithList();
            });

            ConnectToLobbyViewCommand = new RelayCommand(o =>
            {
                GetConnectToLobbyResult();
            });

            ReceivedPublicLobbiesListCommand = new RelayCommand(o =>
            {
                ReceivedPublicLobbiesList();
            });

            SendLobbiesListRequestCommand = new RelayCommand(o =>
            {
                mainViewModel._server.SendOpCodeToServer(Convert.ToByte(OpCodes.SendLobbiesList));
            });
        }

        private void SendConnectToLobbyWithList()
        {
            if (SelectedHub.JoinCode != 0)
            {
                SendConnectToLobbyRequest(SelectedHub.JoinCode.ToString());
            }
        }

        private void SendConnectToLobbyRequest(string joincode)
        {
            int joinCodeNum;
            bool result = Int32.TryParse(joincode, out joinCodeNum);
            if (result)
            {
                if (joinCodeNum >= 1000 && joinCodeNum < 10000)
                {
                    string joinCodeStr = joinCodeNum.ToString();
                    mainViewModel._server.SendMessageToServerOpCode(joinCodeStr, Convert.ToByte(OpCodes.SendLobbyJoinCode));
                }
            }
        }

        private void GetConnectToLobbyResult()
        {
            var msg = mainViewModel._server.PacketReader.ReadMessage();
            if (msg.Equals("lobbyDoesntExists") == false && msg.Equals("wrongJoinCode") == false)
            {
                HubModel lobby = HubModel.JsonToSingleLobby(msg);
                LobbyViewModel = new LobbyViewModel(mainViewModel, lobby);
                mainViewModel.CurrentView = LobbyViewModel;
                mainViewModel._server.SendOpCodeToServer(Convert.ToByte(OpCodes.SendCategoriesList));
            }
        }
    }    
}
