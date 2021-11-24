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
        public RelayCommand SendCategoriesListReqCommand { get; set; }
        private MainViewModel mainViewModel;
        private ObservableCollection<CategoryModel> _categories;
        public ObservableCollection<CategoryModel> Categories
        {
            get { return _categories; }
            set
            {
                _categories = value;
                onPropertyChanged();
            }
        }


        public LobbyViewModel(MainViewModel mainViewModel, HubModel hub)
        {
            Hub = hub;
            this.mainViewModel = mainViewModel;
            mainViewModel._server.receivedCategoryListEvent += ReceivedCategoryList;


            /*            SendCategoriesListReqCommand = new RelayCommand(o =>                                              //request sent after succesfull finding lobby, no need here
                        {
                            mainViewModel._server.SendOpCodeToServer(Convert.ToByte(OpCodes.SendCategoriesList));
                        });
            */
            //command to start game, to get ready, etc

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
