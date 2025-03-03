using Microsoft.AspNetCore.SignalR.Client;
using Stack.ViewModels.Utilidades;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Stack.ViewModels
{
    [QueryProperty(nameof(_connection), "connection")]
    public class WaitVM : INotifyPropertyChanged
    {
        #region Atributos
        private readonly HubConnection _connection;
        private String _opponentName;
        private DelegateCommand volverCommand;
        #endregion

        #region Propiedades
        public String OpponentName
        {
            get { return _opponentName; }
            set { _opponentName = value; }
        }

        public DelegateCommand VolverCommand
        {
            get { return volverCommand; }
        }
        #endregion

        #region Constructores
        public WaitVM()
        {
            volverCommand = new DelegateCommand(volverCommandExecuted);

            // Creamos la conexión
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5279/chathub")
                .Build();

            // Nos conectamos
            Task.Run(async () =>
            {
                await MainThread.InvokeOnMainThreadAsync(async () => await _connection.StartAsync());
            });

            // Recibimos el nombre del jugador cuando se una
            _connection.On<String>("ReceiveNamePlayer", receiveNamePlayer);
        }
        #endregion

        #region Métodos
        /// <summary>
        /// Método que recibe el nombre del oponente y lo pinta en pantalla<br>
        /// Pre: Ninguno</br>
        /// Post: Ninguno
        /// </summary>
        /// <param name="playerName">Nombre del oponente</param>
        private async void receiveNamePlayer(String playerName)
        {
            await MainThread.InvokeOnMainThreadAsync(async () => _opponentName = playerName);
            NotifyPropertyChanged(nameof(OpponentName));
        }
        #endregion

        #region Commands
        private async void volverCommandExecuted()
        {
            await Shell.Current.GoToAsync("///home");
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
