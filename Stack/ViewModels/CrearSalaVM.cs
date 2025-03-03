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
            crearSalaCommand = new DelegateCommand(crearSalaCommandExecuted, crearSalaCommandCanExecute);
        }
        #endregion

        #region Commands
        /// <summary>
        /// Función que crea una sala con el nombre especificado<br>
        /// Pre: Debe haberse especificado el grupo y el nombre del jugador</br>
        /// Post: Ninguno
        /// </summary>
        public async void crearSalaCommandExecuted()
        {
            if (!string.IsNullOrEmpty(_nameRoom) && !string.IsNullOrEmpty(_playerName)) {
                await _connection.InvokeCoreAsync("JoinRoom", args: new[]
                 { _nameRoom, _playerName });

                //crearSalaCommandCanExecute.RaiseCanExecuteChanged();
            }
        }

        public bool crearSalaCommandCanExecute()
        {
            return true;
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
