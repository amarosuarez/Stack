using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Stack.ViewModels
{
    [QueryProperty(nameof(PlayerName), "playerName")]
    [QueryProperty(nameof(OpponentName), "opponentName")]
    [QueryProperty(nameof(NameRoom), "nameRoom")]
    [QueryProperty(nameof(Owner), "owner")]
    public class PrePartidaVM : INotifyPropertyChanged
    {
        #region Atributos
        private String _playerName;
        private String _opponentName;
        private String _countdown;
        private String _nameRoom;
        private String _owner;
        private bool _isOwner;
        #endregion

        #region Propiedades
        public String PlayerName {
            get { return _playerName; }
            set
            {
                _playerName = Uri.UnescapeDataString(value);
                NotifyPropertyChanged();
            }
        }

        public String OpponentName
        {
            get { return _opponentName; }
            set
            {
                _opponentName = Uri.UnescapeDataString(value);
                NotifyPropertyChanged();
            }
        }

        public String Countdown
        {
            get => _countdown;
            set
            {
                _countdown = value;
                NotifyPropertyChanged();
            }
        }

        public String NameRoom
        {
            get { return _nameRoom; }
            set
            {
                _nameRoom = Uri.UnescapeDataString(value);
                NotifyPropertyChanged();
            }
        }

        public String Owner
        {
            get { return _owner; }
            set
            {
                _owner = Uri.EscapeDataString(value);
                NotifyPropertyChanged();
            }
        }
        #endregion

        #region Constructores
        public PrePartidaVM()
        {
            StartCountdown();
        }
        #endregion

        #region Métodos
        private async void StartCountdown()
        {
            int countdownValue = 0;
            while (countdownValue > 0)
            {
                Countdown = countdownValue.ToString();
                await Task.Delay(1000); // Esperar 1 segundo
                countdownValue--;
            }

            Countdown = "¡Comienza el juego!";
            await Task.Delay(1000);
            await Shell.Current.GoToAsync($"///partida?nameRoom={_nameRoom}&owner={_owner}");
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
