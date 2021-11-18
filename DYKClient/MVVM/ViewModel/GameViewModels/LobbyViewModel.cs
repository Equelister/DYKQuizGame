using DYKClient.Core;
using DYKShared.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DYKClient.MVVM.ViewModel.GameViewModels
{
    class LobbyViewModel : ObservableObject
    {
        public LobbyViewModel(MainViewModel mainViewModel, HubModel hub)
        {
            Hub = hub;

            //command to start game, to get ready, etc

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
                if (PropertyChanged != null)
                    PropertyChanged.Invoke(this,
                        new PropertyChangedEventArgs("Hub"));
            }
        }







    }
}
