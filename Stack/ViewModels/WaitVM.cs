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
    public class WaitVM : INotifyPropertyChanged
    {
        #region Atributos
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

            // Inicia la conexión de SignalR de forma asíncrona
            Task.Run(async () => await InitConnection());
        }
        #endregion

        #region Métodos
        private async Task InitConnection()
        {
            try
            {
                await GlobalConnection.StartConnection();

                // Verifica si la conexión fue exitosa
                if (GlobalConnection.connection.State == HubConnectionState.Connected)
                {
                    Console.WriteLine("Conexión exitosa");

                    // Obtiene el nombre de la sala
                    string roomName = await GlobalConnection.connection.InvokeAsync<string>("GetMyRoom");

                    // Recibe el mensaje cuando otro jugador se conecta
                    GlobalConnection.connection.On<string>("ReceiveNamePlayer", receiveNamePlayer);
                }
                else
                {
                    Console.WriteLine("Conexión no establecida");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de conexión: {ex.Message}");
            }
        }

        /// <summary>
        /// Método que recibe el nombre del oponente y lo pinta en pantalla<br>
        /// Pre: Ninguno</br>
        /// Post: Ninguno
        /// </summary>
        /// <param name="playerName">Nombre del oponente</param>
        private async void receiveNamePlayer(String playerName)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                _opponentName = $"{playerName} se ha unido a la sala!";
                NotifyPropertyChanged(nameof(OpponentName));
            });
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
