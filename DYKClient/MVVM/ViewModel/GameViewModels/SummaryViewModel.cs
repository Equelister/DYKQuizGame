using DYKClient.Core;
using DYKShared.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DYKClient.MVVM.ViewModel.GameViewModels
{
    class SummaryViewModel : ObservableObject
    {
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
                return !GameInProgress ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
        }

        private ObservableCollection<SummaryModel> _summary = new ObservableCollection<SummaryModel>();
        public ObservableCollection<SummaryModel> Summary
        {
            get { return _summary; }
            set
            {
                _summary = value;
                onPropertyChanged("Summary");
            }
        }

        private MainViewModel mainViewModel;

        public SummaryViewModel(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;

            mainViewModel._server.getGameSummaryEvent += DisplaySummary;
            /*
            Summary.Add(new SummaryModel("a", "b", "c", new List<string> { "a", "bc" }));
            Summary.Add(new SummaryModel("avbc", "b", "c", new List<string> {}));
            Summary.Add(new SummaryModel("a", "basddasadsdasad", "c", new List<string> { "a" }));*/
        }

        private void DisplaySummary()
        {
            var msg = mainViewModel._server.PacketReader.ReadMessage();
            Console.WriteLine("\r\n OTO PODSUMOWANIE: " + msg + "\r\n");
            Summary = SummaryModel.JsonToObservableCollection(msg);
            onPropertyChanged("Summary");
            GameInProgress = false;
        }
    }
}
