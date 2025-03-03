using Microsoft.AspNetCore.SignalR.Client;
using Stack.ViewModels.Utilidades;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Stack.ViewModels
{
    public class CrearSalaVM : INotifyPropertyChanged
    {
        #region Atributos
        private readonly HubConnection _connection;
        private String _nameRoom;
        private String _playerName;
        private DelegateCommand crearSalaCommand;
        private DelegateCommand volverCommand;
        #endregion

        #region Propiedades
        public String NameRoom
        {
            get { return _nameRoom; }
            set { _nameRoom = value; }
        }

        public String PlayerName
        {
            get { return _playerName; }
            set { _playerName = value; }
        }

        public DelegateCommand CrearSalaCommand
        {
            get { return crearSalaCommand; }
        }

        public DelegateCommand VolverCommand
        {
            get { return volverCommand; }
        }
        #endregion

        #region Constructores
        public CrearSalaVM()
        {

        }
        #endregion

        #region Notify
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")

        {

            PropertyChanged?.Invoke(this, new
            PropertyChangedEventArgs(propertyName));

        }
        #endregion
    }
}
