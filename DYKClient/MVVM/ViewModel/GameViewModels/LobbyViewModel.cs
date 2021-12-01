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
                    _selectedCategory = new CategoryModel();
                }
                return _selectedCategory;
            }
            set
            {
                _selectedCategory = value;
                onPropertyChanged("SelectedCategory");
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
                        _playerNumberStr = value;
                        onPropertyChanged("PlayerNumberStr");
                    }
                }else
                {
                    _playerNumberStr = "8";
                }
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
            }
        }

        private bool _isPublic;
        public bool IsPublic
        {
            get
            {
                return _isPublic;
            }
            set
            {
                _isPublic = value;
                onPropertyChanged("IsPublic");                
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
            if (hub is not null)
            {
                InitializeFields();
            }
            else
            {
                IsPublic = true;
            }

            this.mainViewModel = mainViewModel;
            mainViewModel.MenuRadios = false;
            mainViewModel._server.receivedCategoryListEvent += ReceivedCategoryList;
            mainViewModel._server.receivedNewLobbyInfoEvent += ReceivedLobbyInfo;
            mainViewModel._server.receivedNewPlayersInfoEvent += ReceivedPlayersInfo;

            UpdateLobbyDataCommand = new RelayCommand(o =>
            {
                if (hub is not null)
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


            /*            SendCategoriesListReqCommand = new RelayCommand(o =>                                              //request sent after succesfull finding lobby, no need here
                        {
                            mainViewModel._server.SendOpCodeToServer(Convert.ToByte(OpCodes.SendCategoriesList));
                        });
            */
            //command to start game, to get ready, etc

        }

        private void InitializeFields()
        {
            SelectedCategory = Hub.Category;
            PlayerNumberStr = Hub.MaxSize.ToString();
            LobbyName = Hub.Name;
            IsPublic = !Hub.IsPrivate;
        }

        private void QuitFromLobby()
        {
            mainViewModel._server.SendOpCodeToServer(OpCodes.SendDisconnectFromLobby);
            mainViewModel._server.receivedCategoryListEvent -= ReceivedCategoryList;   // need fix, when 2 players are in lobby, first one quits, the lobby is bugged - 95% from client side
            mainViewModel._server.receivedNewLobbyInfoEvent -= ReceivedLobbyInfo;       // 5% because client is still in lobby (on server side) and request is sending to him, but hes on different view
            mainViewModel._server.receivedNewPlayersInfoEvent -= ReceivedPlayersInfo;       // 5% because client is still in lobby (on server side) and request is sending to him, but hes on different view
                                                                                          // make user deletion from hub on server side while quiting lobby by button and check if still not working
            this.Hub = null;
            this.Categories = null;
            this.PlayerNumberStr = null;
            this.SelectedCategory = null;
            this.UpdateLobbyDataCommand = null;
            this.QuitFromLobbyCommand = null;

            mainViewModel.MenuRadios = true;
            mainViewModel.CurrentView = mainViewModel.LobbiesViewModel;
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
            Hub.Category = SelectedCategory;
            if(Hub.Users.Count() > Int32.Parse(PlayerNumberStr))
            {
                return;
            }
            Hub.MaxSize = Int32.Parse(PlayerNumberStr);
            Hub.IsPrivate = IsPublic;
            Hub.Name = LobbyName;
            var jsonMessage = Hub.ConvertToJson();
            mainViewModel._server.SendMessageToServerOpCode(jsonMessage, OpCodes.SendNewLobbyInfo);
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
