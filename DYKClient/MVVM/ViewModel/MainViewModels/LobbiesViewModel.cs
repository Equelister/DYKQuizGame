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
    class LobbiesViewModel : INotifyPropertyChanged
    {
        private MainViewModel mainViewModel;
        public List<HubModel> hubs = new List<HubModel>();

        //public event PropertyChangedEventHandler PropertyChanged;

        //public event PropertyChangedEventHandler PropertyChanged;

        public RelayCommand NewLobbyViewCommand { get; set; }
        public RelayCommand ConnectToLobbyViewCommand { get; set; }
        public RelayCommand ReceivedPublicLobbiesListCommand { get; set; }


        public ObservableCollection<HubModel> hubiksy = new ObservableCollection<HubModel>();



        public LobbyViewModel LobbyViewModel { get; set; }

        public LobbiesViewModel(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
            LobbyViewModel = new LobbyViewModel();
            mainViewModel._server.receivedPublicLobbiesListEvent += ReceivedPublicLobbiesList;
            //lobbiesListView.ItemsSource = hubs;
            hubiksy = new ObservableCollection<HubModel>();

            //Customers = new ObservableCollection<HubModel>();
            // Window parentWindow = Window.GetWindow(this);

            NewLobbyViewCommand = new RelayCommand(o =>
            {
                mainViewModel.CurrentView = LobbyViewModel;
            });

            ConnectToLobbyViewCommand = new RelayCommand(o =>
            {
                mainViewModel.CurrentView = LobbyViewModel;
            });

            ReceivedPublicLobbiesListCommand = new RelayCommand(o =>
            {
                ReceivedPublicLobbiesList();
            });
        }

        public void ReceivedPublicLobbiesList()
        {
            var msg = mainViewModel._server.PacketReader.ReadMessage();
            hubs = HubModel.JsonListToHubModelList(msg);
            hubiksy = HubModel.JsonListToHubModelObservableCollection(msg);
            //Application.Current.Dispatcher.Invoke(() => hubs.Add(msg));
            //Application.LoadComponent.lobbiesListView
            //Customers.Add(new HubModel("aaa", 1234));
            Icon = hubs;
            Icon.Add(new HubModel("aaaa", 2134));
            //hubiksy.Add(new HubModel("aaaa", 2134));
            //CollectionViewSource.GetDefaultView(hubiksy).Refresh();
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

        private List<HubModel> _Icon;
        public List<HubModel> Icon
        {
            get { return _Icon; }
            set
            {
                _Icon = value;
                if (PropertyChanged != null)
                    PropertyChanged.Invoke(this,
                        new PropertyChangedEventArgs("Icon"));
            }
        }


    }

    
}
