using DYKClient.Core;
using DYKClient.MVVM.ViewModel.GameViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DYKClient.MVVM.ViewModel
{
    class LobbiesViewModel
    {
        private MainViewModel mainViewModel;

        public RelayCommand NewLobbyViewCommand { get; set; }
        public RelayCommand ConnectToLobbyViewCommand { get; set; }

        public LobbyViewModel LobbyViewModel { get; set; }

        public LobbiesViewModel(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
            LobbyViewModel = new LobbyViewModel();

           // Window parentWindow = Window.GetWindow(this);

            NewLobbyViewCommand = new RelayCommand(o =>
            {
                mainViewModel.CurrentView = LobbyViewModel;
            });

            ConnectToLobbyViewCommand = new RelayCommand(o =>
            {
                mainViewModel.CurrentView = LobbyViewModel;
            });
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

    }
}
