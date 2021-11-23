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
        private MainViewModel mainViewModel;
        //public List<HubModel> hubs = new List<HubModel>();

        //public event PropertyChangedEventHandler PropertyChanged;

        //public event PropertyChangedEventHandler PropertyChanged;

        public RelayCommand NewLobbyViewCommand { get; set; }
        public RelayCommand ConnectToLobbyViewCommand { get; set; }
        public RelayCommand SendConnectToLobbyWithListCommand { get; set; }
        public RelayCommand SendConnectToLobbyReqCommand { get; set; }
        public RelayCommand ReceivedPublicLobbiesListCommand { get; set; }
        public RelayCommand SendLobbiesListRequestCommand { get; set; }
        //public RelayCommand ConnectToLobbyCommand { get; set; }


        //public ObservableCollection<HubModel> hubiksy = new ObservableCollection<HubModel>();



        public LobbyViewModel LobbyViewModel { get; set; }

        public LobbiesViewModel(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
            mainViewModel._server.receivedPublicLobbiesListEvent += ReceivedPublicLobbiesList;
            mainViewModel._server.connectToLobbyViewEvent += GetConnectToLobbyResult;
            //lobbiesListView.ItemsSource = hubs;
            //hubiksy = new ObservableCollection<HubModel>();
/*            Hubs = new List<HubModel>();
            JoinCode = "";*/
            //Customers = new ObservableCollection<HubModel>();
            // Window parentWindow = Window.GetWindow(this);

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
            //ReceivedPublicLobbiesList();

            //mainViewModel._server.SendOpCodeToServer(Convert.ToByte(OpCodes.SendLobbiesList));
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
            }
        }

        private int counter = 1;

        public void ReceivedPublicLobbiesList()
        {
            var msg = mainViewModel._server.PacketReader.ReadMessage();
            //Hubs = HubModel.JsonListToHubModelList(msg);
            Hubs = HubModel.JsonListToHubModelObservableCollection(msg);

            //TimeStepDataCollection.Add(new HubModel(counter++.ToString(), 9999));
            //Hubs.Add(new HubModel(counter++.ToString(), 9998));


            //hubiksy = HubModel.JsonListToHubModelObservableCollection(msg);
            //Application.Current.Dispatcher.Invoke(() => hubs.Add(msg));
            //Application.LoadComponent.lobbiesListView
            //Customers.Add(new HubModel("aaa", 1234));
            //Icon = hubs;
            //Icon.Add(new HubModel("aaaa", 2134));
            //Hubs.Add(new HubModel("aaa", 1234));
            //hubiksy.Add(new HubModel("aaaa", 2134));

            //CollectionViewSource.GetDefaultView(Hubs).Refresh();
        }


        /*
                public RelayCommand HelpViewCommand { get; set; }
                public RelayCommand AboutViewCommand { get; set; }
                public RelayCommand LobbiesViewCommand { get; set; }


                public HelpViewModel HelpViewModel { get; set; }
                public AboutViewModel AboutViewModel { get; set; }
                public LobbiesViewModel LobbiesViewModel { get; set; }
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


                public MainViewModel()
                {
                    HelpViewModel = new HelpViewModel();
                    AboutViewModel = new AboutViewModel();
                    LobbiesViewModel = new LobbiesViewModel(this);
                    CurrentView = LobbiesViewModel;



                    LobbiesViewCommand = new RelayCommand(o =>
                    {
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


                }*/


        /*public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler(this, new PropertyChangedEventArgs(name));
        }
        private ObservableCollection<HubModel> customers;
        public ObservableCollection<HubModel> Customers
        {
            get { return customers; }
            set
            {
                customers = value;
                OnPropertyChanged("Customers");
            }
        }*/


        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        //public HubModel SelectedHub = new HubModel();

        private ObservableCollection<HubModel> _Hubs;
        public ObservableCollection<HubModel> Hubs
        {
            get { return _Hubs; }
            set
            {
                _Hubs = value;
                //if (this.CollectionChanged != null)
                onPropertyChanged();
            }
        }

        private HubModel _selectedHub;
        public HubModel SelectedHub
        {
            get
            {
                if(_selectedHub is null)
                {
                    _selectedHub = new HubModel();
                }
                return _selectedHub;
            }
            set
            {
                _selectedHub = value;
                //Departures = Departure.GetDepartures(_currentStation);
                Console.WriteLine("New station selected: " + _selectedHub.ToString());
                onPropertyChanged("SelectedHub");
            }
        }

        private string joinCode;
        public string JoinCode
        {
            get { return this.joinCode; }
            set
            {
                // Implement with property changed handling for INotifyPropertyChanged
                if (string.Equals(this.joinCode, value) == false)
                {
                    //    this.joinCode = value;
                    //    this.PropertyChanged.Invoke(this, 
                    //        new PropertyChangedEventArgs("JoinCode")); // Method to raise the PropertyChanged event in your BaseViewModel class...

                    this.joinCode = value;
                    this.onPropertyChanged(this.joinCode); // Method to raise the PropertyChanged event in your BaseViewModel class...

                }
            }
        }

    }

    
}
