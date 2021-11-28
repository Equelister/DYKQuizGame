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
                if (IsTextNumeric(_playerNumberStr))
                {
                    _playerNumberStr = value;
                    onPropertyChanged("PlayerNumberStr");
                }else
                {
                    _playerNumberStr = "";
                }
            }
        }

        private bool IsTextNumeric(string str)
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("[^0-9]");
            if (str is not null)
            {
                return reg.IsMatch(str);
            }
            return false;
        }

        public LobbyViewModel(MainViewModel mainViewModel, HubModel hub)
        {
            Hub = hub;
            this.mainViewModel = mainViewModel;
            mainViewModel.MenuRadios = false;
            mainViewModel._server.receivedCategoryListEvent += ReceivedCategoryList;
            mainViewModel._server.receivedNewLobbyInfoEvent += ReceivedLobbyInfo;

            UpdateLobbyDataCommand = new RelayCommand(o =>
            {
                SendNewLobbyData();
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

        private void QuitFromLobby()
        {
            this.Hub = null;
            this.Categories = null;
            this.PlayerNumberStr = null;
            this.SelectedCategory = null;
            this.UpdateLobbyDataCommand = null;
            this.QuitFromLobbyCommand = null;
            mainViewModel._server.receivedCategoryListEvent -= ReceivedCategoryList;
            mainViewModel._server.receivedNewLobbyInfoEvent -= ReceivedLobbyInfo;
            mainViewModel.MenuRadios = true;
            mainViewModel.CurrentView = mainViewModel.LobbiesViewModel;
        }

        public void ReceivedLobbyInfo()
        {
            throw new NotImplementedException("ReceivedLobbyInfo() => Not implemented");
        }
        public void SendNewLobbyData()
        {
            throw new NotImplementedException("SendNewLobbyData() => Not implemented");
        }

        public void ReceivedCategoryList()
        {
            var msg = mainViewModel._server.PacketReader.ReadMessage();
            Categories = CategoryModel.JsonListToCategoryModelObservableCollection(msg);
        }

        //clock every10s request users list(or new complete hub to update) ////////// button refresh to do the same



        public event PropertyChangedEventHandler PropertyChanged;


        private HubModel _Hub;
        public HubModel Hub
        {
            get { return _Hub; }
            set
            {
                _Hub = value;
                onPropertyChanged();
            }
        }







    }
}
