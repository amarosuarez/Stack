using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Models;
using Stack.ViewModels.Utilidades;

namespace Stack.ViewModels
{
    public class UnirseSalaVM : INotifyPropertyChanged
    {
        #region Atributos
        private String _nameRoom = "hola";
        private String _playerName = "auri";
        private String _error;
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
                    _error = "";
                    NotifyPropertyChanged(nameof(Error));
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
                    _error = "";
                    NotifyPropertyChanged(nameof(Error));
                    unirseCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public String Error
        {
            get { return _error; }
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
                await GlobalConnection.StartConnection();

                try
                {
                    RoomJoinResult roomJoinResult = await GlobalConnection.connection.InvokeAsync<RoomJoinResult>("JoinRoom", _nameRoom, _playerName, false);


                    if (roomJoinResult.Success) {
                        String opponentName = await GlobalConnection.connection.InvokeAsync<string>("GetOpponentName", _nameRoom);


                        unirseCommand.RaiseCanExecuteChanged();
                        await Shell.Current.GoToAsync($"///prePartida?playerName={_playerName}&opponentName={opponentName}&nameRoom={_nameRoom}");
                        limpiarTexts();
                    } else
                    {
                        _error = roomJoinResult.Message;
                        NotifyPropertyChanged(nameof(Error));
                    }

                    
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
            _error = "";
            NotifyPropertyChanged(nameof(Error));
            unirseCommand.RaiseCanExecuteChanged();
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
