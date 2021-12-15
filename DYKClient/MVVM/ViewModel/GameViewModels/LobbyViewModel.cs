using DYKClient.Core;
using DYKClient.Net;
using DYKShared.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DYKClient.MVVM.ViewModel.GameViewModels
{
    class LobbyViewModel : ObservableObject
    {
        public RelayCommand UpdateLobbyDataCommand { get; set; }
        public RelayCommand SetPlayerReadyCommand { get; set; }
        public RelayCommand StartEnhancedGameCommand { get; set; }
        public RelayCommand StartNormalGameCommand { get; set; }
        //public RelayCommand SendCategoriesListReqCommand { get; set; }
        public RelayCommand QuitFromLobbyCommand { get; set; }
        private MainViewModel mainViewModel;
        private ObservableCollection<CategoryModel> _categories = new ObservableCollection<CategoryModel>();
        public ObservableCollection<CategoryModel> Categories
        {
            get { return _categories; }
            set
            {
                _categories = value;
                onPropertyChanged();
            }
        }
        private CategoryModel _selectedCategory;
        public CategoryModel SelectedCategory
        {
            get
            {
                if (_selectedCategory is null)
                {
                    return null;
                }
                return _selectedCategory;
            }
            set
            {
                _selectedCategory = value;
                onPropertyChanged("SelectedCategory");
                IsHubChanged = true;
            }
        }

        private string _playerNumberStr;
        public string PlayerNumberStr
        {
            get
            {
                return _playerNumberStr;
            }
            set
            {
                if (IsTextNumeric(value))
                {
                    if (Int32.Parse(value) > 8 || Int32.Parse(value) < 2)
                    {
                        _playerNumberStr = "8";
                    }
                    else
                    {
                        if (Hub is not null)
                        {
                            if (Hub.Users is not null)
                            {
                                if (Int32.Parse(value) < Hub.Users.Count)
                                {
                                    _playerNumberStr = Hub.Users.Count.ToString();
                                }else
                                {
                                    _playerNumberStr = value;
                                }
                            }
                        }
                        else
                        {
                            _playerNumberStr = value;
                        }
                    }
                }else
                {
                    _playerNumberStr = "8";
                }
                onPropertyChanged("PlayerNumberStr");
                IsHubChanged = true;
            }
        }

        private string _lobbyName;
        public string LobbyName
        {
            get
            {
                return _lobbyName;
            }
            set
            {
                _lobbyName = value;
                onPropertyChanged("LobbyName");
                IsHubChanged = true;
            }
        }

        private bool _isPrivate;
        public bool IsPrivate
        {
            get
            {
                return _isPrivate;
            }
            set
            {
                _isPrivate = value;
                onPropertyChanged("IsPrivate");
                IsHubChanged = true;
            }
        }

        private bool _isHubChanged = false;
        public bool IsHubChanged
        {
            get
            {
                return _isHubChanged;
            }
            set
            {
                _isHubChanged = value;
                onPropertyChanged("IsHubChanged");
                onPropertyChanged("IsHubChangedVisibility");
            }
        }
        public System.Windows.Visibility IsHubChangedVisibility
        {
            get 
            {
                return IsHubChanged ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
        }

        private bool IsTextNumeric(string str)
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("^[0-9]$");
            if (str is not null)
            {
                return reg.IsMatch(str);
            }
            return false;
        }

        public LobbyViewModel(MainViewModel mainViewModel, HubModel hub)
        {
            Hub = hub;
            if (Hub is not null)
            {
                InitializeFields();
            }
            else
            {
                IsPrivate = true;
                IsHubChanged = true;
            }

            this.mainViewModel = mainViewModel;
            mainViewModel.MenuRadios = false;
            InitializeEvents();
            InitializeCommands();
        }

        private void InitializeEvents()
        {
            mainViewModel._server.receivedCategoryListEvent += ReceivedCategoryList;
            mainViewModel._server.receivedNewLobbyInfoEvent += ReceivedLobbyInfo;
            mainViewModel._server.receivedNewPlayersInfoEvent += ReceivedPlayersInfo;
            mainViewModel._server.startEnhancedGameEvent += EnhancedGameChangeView;
            mainViewModel._server.startNormalGameEvent += NormalGameChangeView;
        }
        private void UnInitializeEvents()
        {
            mainViewModel._server.receivedCategoryListEvent -= ReceivedCategoryList;
            mainViewModel._server.receivedNewLobbyInfoEvent -= ReceivedLobbyInfo;
            mainViewModel._server.receivedNewPlayersInfoEvent -= ReceivedPlayersInfo;
            mainViewModel._server.startEnhancedGameEvent -= EnhancedGameChangeView;
            mainViewModel._server.startNormalGameEvent -= NormalGameChangeView;
        }

        private void InitializeCommands()
        {
            UpdateLobbyDataCommand = new RelayCommand(o =>
            {
                if (Hub is not null)
                {
                    SendUpdatedLobbyData();
                }
                else
                {
                    SendNewLobbyData();
                }
            });

            QuitFromLobbyCommand = new RelayCommand(o =>
            {
                QuitFromLobby();
                //Sendquiting info to server
            });

            SetPlayerReadyCommand = new RelayCommand(o =>
            {
                SetPlayerReady();
            });

            StartEnhancedGameCommand = new RelayCommand(o =>
            {
                StartGame(OpCodes.SendNewEnhancedGame);
                //Sendquiting info to server
            });

            StartNormalGameCommand = new RelayCommand(o =>
            {
                StartGame(OpCodes.SendNewNormalGame);
            });
        }

        private void StartGame(OpCodes gameType)
        {            
            if(CheckArePlayersReady())
            {
                mainViewModel._server.SendOpCodeToServer(gameType);
            }
        }

        private bool CheckArePlayersReady()
        {
            if (Hub is not null && IsHubChanged == false)
            {
                if (Hub.Users.Count < 2)
                {
                    return false;
                }
                foreach (var user in Hub.Users)
                {
                    if (user.IsReady == false)
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        private void SetPlayerReady()
        {
            mainViewModel._server.SendOpCodeToServer(OpCodes.SendUserReady);
        }

        private void InitializeFields()
        {
            SelectedCategory = Hub.Category;
            PlayerNumberStr = Hub.MaxSize.ToString();
            LobbyName = Hub.Name;
            IsPrivate = Hub.IsPrivate;
            IsHubChanged = false;
        }

        private void QuitFromLobby()
        {
            mainViewModel._server.SendOpCodeToServer(OpCodes.SendDisconnectFromLobby);
            UnInitializeEvents();
            this.Hub = null;
            this.Categories = null;
            this.PlayerNumberStr = null;
            this.SelectedCategory = null;
            this.UpdateLobbyDataCommand = null;
            this.QuitFromLobbyCommand = null;
            this.IsHubChanged = false;

            mainViewModel.MenuRadios = true;
            mainViewModel.CurrentView = mainViewModel.LobbiesViewModel;
        }

        private GameViewModel gameViewModel;
        private void EnhancedGameChangeView()
        {         
/*            gameViewModel = null;
            gameViewModel = new GameViewModel(mainViewModel);
            mainViewModel.CurrentView = gameViewModel;   */         
        
            throw new NotImplementedException("ENHANCED MODE NOT IMPLEMENTED YET");
        }

        private void NormalGameChangeView()
        {
            gameViewModel = null;
            gameViewModel = new GameViewModel(mainViewModel);
            mainViewModel.CurrentView = gameViewModel;
        }

        public void ReceivedLobbyInfo()
        {
            var msg = mainViewModel._server.PacketReader.ReadMessage();
            Hub = HubModel.JsonToSingleLobby(msg);
            InitializeFields();
            //throw new NotImplementedException("ReceivedLobbyInfo() => Not implemented");
        }
        
        public void ReceivedPlayersInfo()
        {
            var msg = mainViewModel._server.PacketReader.ReadMessage();
            Hub.Users = UserModel.JsonListToUserModelList(msg);
            onPropertyChanged("Hub");

            //InitializeFields();
            //throw new NotImplementedException("ReceivedLobbyInfo() => Not implemented");
        }
        public void SendNewLobbyData()
        {
            Hub = new HubModel();
            SendUpdatedLobbyData();
            //throw new NotImplementedException("SendNewLobbyData() => Not implemented");
        }

        public void SendUpdatedLobbyData()
        {
            if (IsHubChanged)
            {
                if (CheckIfInputFieldsAreEmpty() == false)
                {
                    Hub.Category = SelectedCategory;
                    if (Hub.Users is not null)
                    {
                        if (Hub.Users.Count() > Int32.Parse(PlayerNumberStr))
                        {
                            return;
                        }
                    }
                    Hub.MaxSize = Int32.Parse(PlayerNumberStr);
                    Hub.IsPrivate = IsPrivate;
                    Hub.Name = LobbyName;
                    var jsonMessage = Hub.ConvertToJson();
                    mainViewModel._server.SendMessageToServerOpCode(jsonMessage, OpCodes.SendNewLobbyInfo);
                }
            }
        }

        private bool CheckIfInputFieldsAreEmpty()
        {
            if(SelectedCategory is null ||
                PlayerNumberStr is null ||
                LobbyName is null)
            {
                return true;
            }
            return false;
        }

        public void ReceivedCategoryList()
        {
            var msg = mainViewModel._server.PacketReader.ReadMessage();
            Categories = CategoryModel.JsonListToCategoryModelObservableCollection(msg);
        }

        //clock every10s request users list(or new complete hub to update) ////////// button refresh to do the same



        public event PropertyChangedEventHandler PropertyChanged;


        private HubModel _hub;
        public HubModel Hub
        {
            get { return _hub; }
            set
            {
                _hub = value;
                onPropertyChanged();
            }
        }







    }
}
