using Microsoft.AspNetCore.SignalR.Client;
using Stack.ViewModels.Utilidades;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Data.Common;

namespace Stack.ViewModels
{
    public class CrearSalaVM : INotifyPropertyChanged
    {
        #region Atributos
        private string _nameRoom;
        private string _playerName;
        private DelegateCommand crearSalaCommand;
        private DelegateCommand volverCommand;
        #endregion

        #region Propiedades
        public string NameRoom
        {
            get { return _nameRoom; }
            set
            {
                _nameRoom = value;
                crearSalaCommand.RaiseCanExecuteChanged();
            }
        }

        public string PlayerName
        {
            get { return _playerName; }
            set
            {
                _playerName = value;
                crearSalaCommand.RaiseCanExecuteChanged();
            }
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
            GlobalConnection.connection = new HubConnectionBuilder()
                .WithUrl(GlobalConnection.ruta)
                .Build();

            // Nos conectamos
            Task.Run(async () =>
            {
                await MainThread.InvokeOnMainThreadAsync(async () => await GlobalConnection.connection.StartAsync());
            });

            crearSalaCommand = new DelegateCommand(crearSalaCommandExecuted, crearSalaCommandCanExecute);
            volverCommand = new DelegateCommand(salirCommandExecuted);
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
            if (!string.IsNullOrEmpty(_nameRoom) && !string.IsNullOrEmpty(_playerName))
            {
                await GlobalConnection.connection.InvokeCoreAsync("JoinRoom", args: new[]
                 { _nameRoom, _playerName });

                crearSalaCommand.RaiseCanExecuteChanged();
                limpiarTexts();
                await Shell.Current.GoToAsync("///wait");
            }
        }

        /// <summary>
        /// Función que determina si el command para crear la sala está activo o no<br>
        /// Pre: Ninguno</br>
        /// Post: Ninguno
        /// </summary>
        /// <returns>Puede ejecutarse o no el command</returns>
        public bool crearSalaCommandCanExecute()
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