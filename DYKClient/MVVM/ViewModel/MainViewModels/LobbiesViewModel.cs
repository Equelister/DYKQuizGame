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

namespace DYKClient.MVVM.ViewModel
{
    class LobbiesViewModel : ObservableObject
    {
        private MainViewModel mainViewModel;
        //public List<HubModel> hubs = new List<HubModel>();

        //public event PropertyChangedEventHandler PropertyChanged;

        //public event PropertyChangedEventHandler PropertyChanged;

        public RelayCommand NewLobbyViewCommand { get; set; }
        public RelayCommand ConnectToLobbyViewCommand { get; set; }
        public RelayCommand ReceivedPublicLobbiesListCommand { get; set; }
        //public RelayCommand ConnectToLobbyCommand { get; set; }


        //public ObservableCollection<HubModel> hubiksy = new ObservableCollection<HubModel>();



        public LobbyViewModel LobbyViewModel { get; set; }

        public LobbiesViewModel(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
            mainViewModel._server.receivedPublicLobbiesListEvent += ReceivedPublicLobbiesList;
            mainViewModel._server.connectToLobbyViewEvent += ConnectToLobby;
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


            ConnectToLobbyViewCommand = new RelayCommand(o =>
            {
                ConnectToLobby();
            });

            ReceivedPublicLobbiesListCommand = new RelayCommand(o =>
            {
                ReceivedPublicLobbiesList();
            });
            //ReceivedPublicLobbiesList();
        }

        private void ConnectToLobby()
        {
            int joinCodeNum;
            bool result = Int32.TryParse(joinCode, out joinCodeNum);
            if (result)
            {
                if (joinCodeNum >= 1000 && joinCodeNum < 10000)
                {
                    mainViewModel._server.SendOpCodeToServer(Convert.ToByte(OpCodes.SendLobbyJoinCode));
                    var msg = mainViewModel._server.PacketReader.ReadMessage();
                    if (msg.Equals("lobbyDoesntExists") == false)
                    {
                        HubModel lobby = HubModel.JsonToSingleLobby(msg);
                        LobbyViewModel = new LobbyViewModel(mainViewModel, lobby);
                        mainViewModel.CurrentView = LobbyViewModel;
                    }
                }
            }
        }

        public void ReceivedPublicLobbiesList()
        {
            var msg = mainViewModel._server.PacketReader.ReadMessage();
            Hubs = HubModel.JsonListToHubModelList(msg);
            //hubiksy = HubModel.JsonListToHubModelObservableCollection(msg);
            //Application.Current.Dispatcher.Invoke(() => hubs.Add(msg));
            //Application.LoadComponent.lobbiesListView
            //Customers.Add(new HubModel("aaa", 1234));
            //Icon = hubs;
            //Icon.Add(new HubModel("aaaa", 2134));
            Hubs.Add(new HubModel("aaa", 1234));
            //hubiksy.Add(new HubModel("aaaa", 2134));
            
            CollectionViewSource.GetDefaultView(Hubs).Refresh();
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

        private List<HubModel> _Hubs;
        public List<HubModel> Hubs
        {
            get { return _Hubs; }
            set
            {
                _Hubs = value;
                if (PropertyChanged != null)
                    PropertyChanged.Invoke(this,
                        new PropertyChangedEventArgs("Hubs"));
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
