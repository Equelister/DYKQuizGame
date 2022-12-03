using DYKClient.Core;
using DYKClient.MVVM.ViewModel.GameViewModels;
using DYKClient.Net;
using DYKShared.ModelHelpers;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DYKClient.MVVM.ViewModel.GameHistoryViewModels
{
    class SummariesListViewModel : ObservableObject, INotifyCollectionChanged
    {
        public RelayCommand ReceivedPublicLobbiesListCommand { get; set; }
        public RelayCommand SendLobbiesListRequestCommand { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        private ObservableCollection<GameModelHelper> _gameHistories;
        public ObservableCollection<GameModelHelper> GameHistories
        {
            get { return _gameHistories; }
            set
            {
                _gameHistories = value;
                onPropertyChanged("GameHistories");
            }
        }

        private GameModelHelper _selectedGameHistory;
        private MainViewModel mainViewModel;
        public GameModelHelper SelectedGameHistory
        {
            get
            {
                return _selectedGameHistory;
            }
            set
            {
                _selectedGameHistory = value;
                onPropertyChanged("SelectedGameHistory");
            }
        }

        public SummariesListViewModel()
        {

        }

        public SummariesListViewModel(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
            InitializeCommands();
        }

        public void ReceivedGameHistoriesList()
        {
            var msg = mainViewModel._server.PacketReader.ReadMessage();
            GameHistories = GameModelHelper.JsonListToObservableCollection(msg);
        }

        public void ReceivedGameHistoryDetails()
        {
            var msg = mainViewModel._server.PacketReader.ReadMessage();
            SummaryViewModel svm = new SummaryViewModel(mainViewModel, msg);
            mainViewModel.CurrentView = svm;
        }

        public void GetGameHistoryDetailsFromServer()
        {
            if (SelectedGameHistory is not null)
            {
                mainViewModel._server.SendMessageToServerOpCode(SelectedGameHistory.ID.ToString(), OpCodes.SendGameHistoryDetails);
            }
        }

        public RelayCommand GetGameHistoriesCommand { get; set; }
        public RelayCommand GetGameHistoryDetailsCommand { get; set; }

        private void InitializeCommands()
        {
            mainViewModel._server.receivedGameHistoriesListEvent += ReceivedGameHistoriesList;
            mainViewModel._server.receiveGameHistoryDetailsEvent += ReceivedGameHistoryDetails;

            GetGameHistoryDetailsCommand = new RelayCommand(o =>
            {
                GetGameHistoryDetailsFromServer();
            });
        }
    }
}
