using DYKClient.Core;
using DYKClient.MVVM.ViewModel.GameViewModels;
using DYKClient.Net;
using DYKShared.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

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
            hubListTemp = new List<HubModel>(Hubs);
        }

        private void InitializeCommands()
        {
            mainViewModel._server.receivedPublicLobbiesListEvent += ReceivedPublicLobbiesList;
            mainViewModel._server.connectToLobbyViewEvent += GetConnectToLobbyResult;

            NewLobbyViewCommand = new RelayCommand(o =>
            {
                CreateNewLobby();
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
                mainViewModel._server.SendOpCodeToServer(OpCodes.SendLobbiesList);
                HubFilter = "";
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
                    mainViewModel._server.SendMessageToServerOpCode(joinCodeStr, OpCodes.SendLobbyJoinCode);
                }
            }
        }

        private void GetConnectToLobbyResult()
        {
            var msg = mainViewModel._server.PacketReader.ReadMessage();
            if (msg.Equals("lobbyDoesntExists") == false && msg.Equals("wrongJoinCode") == false)
            {
                HubModel lobby = HubModel.JsonToSingleLobby(msg);
                LobbyViewModel = null;
                LobbyViewModel = new LobbyViewModel(mainViewModel, lobby);
                mainViewModel.CurrentView = LobbyViewModel;
                mainViewModel._server.SendOpCodeToServer(OpCodes.SendCategoriesList);
            }
        }

        private void CreateNewLobby()
        {
            LobbyViewModel = null;
            LobbyViewModel = new LobbyViewModel(mainViewModel, null);
            mainViewModel.CurrentView = LobbyViewModel;
            mainViewModel._server.SendOpCodeToServer(OpCodes.SendCategoriesList);
        }

        private List<HubModel> hubListTemp;
        private string _hubFilter = "";
        public string HubFilter
        {
            get
            {

                return _hubFilter;
            }
            set
            {
                _hubFilter = value;
                onPropertyChanged();
                if (_hubFilter.Equals(""))
                {
                    Hubs = new ObservableCollection<HubModel>(hubListTemp);
                    onPropertyChanged("Hubs");
                }
                else
                {
                    Hubs = new ObservableCollection<HubModel>(hubListTemp);
                    Hubs = DoAFilter();
                    onPropertyChanged("Hubs");
                }
            }
        }
        private ObservableCollection<HubModel> DoAFilter()
        {
            ObservableCollection<HubModel> filteredList = new ObservableCollection<HubModel>();
            foreach (var elem in Hubs)
            {
                if (elem.Name.ToLower().Contains(HubFilter.ToLower()) ||
                    elem.Category.Name.ToLower().Contains(HubFilter.ToLower()))
                {
                    filteredList.Add(elem);
                }
            }
            return filteredList;
        }
    }
}
