using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Stack.ViewModels.Utilidades;

namespace Stack.ViewModels
{
    public class UnirseSalaVM : INotifyPropertyChanged
    {
        #region Atributos
        private String _nameRoom;
        private String _playerName;
        private DelegateCommand unirseCommand;
        private DelegateCommand volverCommand;
        #endregion

        #region Propiedades
        public String NameRoom
        {
            get { return _nameRoom; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _nameRoom = value;
                    unirseCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public String PlayerName
        {
            get { return _playerName; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _playerName = value;
                    unirseCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public DelegateCommand UnirseCommand
        {
            get { return unirseCommand; }
        }

        public DelegateCommand VolverCommand
        {
            get { return volverCommand; }
        }
        #endregion

        #region Constructores
        public UnirseSalaVM()
        {
            unirseCommand = new DelegateCommand(unirseSalaCommandExecuted, unirseSalaCommandCanExecute);
            volverCommand = new DelegateCommand(salirCommandExecuted);
        }
        #endregion

        #region Commands 
        /// <summary>
        /// Función que se une a una sala con el nombre especificado<br>
        /// Pre: Debe haberse especificado el grupo y el nombre del jugador</br>
        /// Post: Ninguno
        /// </summary>
        public async void unirseSalaCommandExecuted()
        {
            if (!string.IsNullOrEmpty(_nameRoom) && !string.IsNullOrEmpty(_playerName))
            {
                Console.WriteLine($"Unirse a la sala: {_nameRoom}, con el jugador: {_playerName}");

                await GlobalConnection.StartConnection();

                try
                {
                    await GlobalConnection.connection.InvokeCoreAsync("JoinRoom", args: new[] { _nameRoom, _playerName });
                    Console.WriteLine("JoinRoom exitoso.");

                    await GlobalConnection.connection.InvokeCoreAsync("ReceiveNamePlayer", args: new[] { _playerName, _nameRoom });
                    Console.WriteLine("Nombre del jugador recibido.");

                    unirseCommand.RaiseCanExecuteChanged();
                    limpiarTexts();
                    //await Shell.Current.GoToAsync("///wait");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al unirse a la sala: {ex.Message}");
                }
            }
        }


        /// <summary>
        /// Función que determina si el command para unirse a la sala está activo o no<br>
        /// Pre: Ninguno</br>
        /// Post: Ninguno
        /// </summary>
        /// <returns>Puede ejecutarse o no el command</returns>
        public bool unirseSalaCommandCanExecute()
        {
            return !string.IsNullOrEmpty(_nameRoom) && !string.IsNullOrEmpty(_playerName);
        }

        /// <summary>
        /// Función que vuelve a la pantalla de home
        /// </summary>
        public async void salirCommandExecuted()
        {
            limpiarTexts();
            await Shell.Current.GoToAsync("///home");
        }
        #endregion

        #region Métodos
        /// <summary>
        /// Función que limpia el texto de los entrys
        /// </summary>
        private void limpiarTexts()
        {
            // Limpiamos los textos
            _nameRoom = "";
            NotifyPropertyChanged(nameof(NameRoom));
            _playerName = "";
            NotifyPropertyChanged(nameof(PlayerName));
        }
        #endregion

        #region Notify
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
