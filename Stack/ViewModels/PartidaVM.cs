using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using Stack.ViewModels.Utilidades;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.SignalR.Client;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace Stack.ViewModels
{
    [QueryProperty(nameof(NameRoom), "nameRoom")]
    [QueryProperty(nameof(Owner), "owner")]
    public class PartidaVM : INotifyPropertyChanged
    {
        #region Atributos
        private RomboDrawable _romboDrawable;
        private System.Timers.Timer _timer;
        private const float MoveLimit = 100; // Límite del área de movimiento
        private List<DynamicRombo> _dynamicRombos = new List<DynamicRombo>();
        private PointF[] _lastRomboPoints; // Puntos del rombo anterior
        private PointF[] _currentRomboPoints; // Puntos del rombo actual
        private PointF[] _recortadoPoints; // Puntos de la figura recortada
        private GraphicsView graphicsView;
        private DelegateCommand tappedScreenCommand;
        private bool miTurno;
        private String nameRoom;
        private String _owner;
        private Color _colorInicial = null;
        private String _tuTurno;
        #endregion

        #region Propiedades
        public RomboDrawable Rombo
        {
            get { return _romboDrawable; }
            set { _romboDrawable = value; }
        }

        public System.Timers.Timer Timer
        {
            get { return _timer; }
            set { _timer = value; }
        }

        public List<DynamicRombo> DynamicRombos
        {
            get { return _dynamicRombos; }
            set { _dynamicRombos = value; }
        }

        public PointF[] LastRomboPoints
        {
            get { return _lastRomboPoints; }
            set { _lastRomboPoints = value; }
        }

        public PointF[] CurrentRomboPoints
        {
            get { return _currentRomboPoints; }
            set { _currentRomboPoints = value; }
        }

        public PointF[] RecortadoPoints
        {
            get { return _recortadoPoints; }
            set { _recortadoPoints = value; }
        }

        public DelegateCommand TappedScreenCommand
        {
            get { return tappedScreenCommand; }
        }

        public bool MiTurno
        {
            get { return miTurno; }
            set
            {
                miTurno = value;
                tappedScreenCommand.RaiseCanExecuteChanged();
            }
        }

        public String NameRoom
        {
            get { return nameRoom; }
            set
            {
                nameRoom = Uri.UnescapeDataString(value);
                NotifyPropertyChanged();

                // Ahora que nameRoom tiene valor, inicializamos la conexión
                Task.Run(async () => await InitConnection());
            }
        }

        public String Owner
        {
            get { return _owner; }
            set
            {
                _owner = Uri.UnescapeDataString(value);
                miTurno = _owner != null ? _owner.Equals("true") : false;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(MiTurno));
            }
        }

        public String TuTurno
        {
            get { return _tuTurno; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _tuTurno = value;
                    NotifyPropertyChanged();
                }
            }
        }
        #endregion

        #region Constructores
        public PartidaVM()
        {
        }

        public PartidaVM(GraphicsView romboGraphicsView) : this()
        {
            tappedScreenCommand = new DelegateCommand(tappedScreenCommandExecuted, tappedScreenCommandCanExecute);

            graphicsView = romboGraphicsView;
            // Inicializar el dibujable del rombo
            _romboDrawable = new RomboDrawable(_dynamicRombos);
            graphicsView.Drawable = _romboDrawable;

            // Configurar un temporizador para animar los rombos
            _timer = new System.Timers.Timer(16); // ~60 FPS
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }
        #endregion

        #region Commands
        private void tappedScreenCommandExecuted()
        {
            OnScreenTapped();
        }

        private bool tappedScreenCommandCanExecute()
        {
            return miTurno;
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
                    GlobalConnection.connection.On<string>("TurnChanged", checkTurn);
                    GlobalConnection.connection.On<string>("PintaRombo", pintaRombo);
                    GlobalConnection.connection.On<float, float>("GetLastRomboPosition", setLastRomboPosition);

                    // Obtener el connectionID del jugador actual
                    string currentConnectionID = await GlobalConnection.connection.InvokeAsync<string>("GetConnectionID");

                    // Obtener el connectionID del jugador cuyo turno es
                    string turnConnectionID = await GlobalConnection.connection.InvokeAsync<string>("GetCurrentTurnPlayer", nameRoom);

                    if (currentConnectionID == turnConnectionID)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            colocarTurnoText(true);
                            CreateRombo();
                        });
                    }
                    else
                    {
                        colocarTurnoText(false);
                    }
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

        private async void setLastRomboPosition(float posX, float posY)
        {
            // **Sincronizar posición exacta desde el servidor**
            if (_dynamicRombos.Count > 0)
            {
                _dynamicRombos[^1].SetPosition(posX, posY);
            }
        }

        private async void pintaRombo(string color)
        {
            _colorInicial = Color.FromArgb(color);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                CreateRombo();
            });
        }

        private async Task checkTurn(String turno)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                // Obtener el connectionID del jugador actual
                string currentConnectionID = await GlobalConnection.connection.InvokeAsync<string>("GetConnectionID");

                // Obtener el connectionID del jugador cuyo turno es
                string turnConnectionID = await GlobalConnection.connection.InvokeAsync<string>("GetCurrentTurnPlayer", nameRoom);

                // Si no es tu turno, detén el último rombo inmediatamente
                if (currentConnectionID != turnConnectionID)
                {
                    if (_dynamicRombos.Count > 0)
                    {
                        _dynamicRombos[^1].IsMoving = false;
                    }
                    miTurno = false;
                }
                else
                {
                    miTurno = true;
                    CreateRombo();
                }

                // Notificar cambios en la propiedad MiTurno
                NotifyPropertyChanged(nameof(MiTurno));
                colocarTurnoText(miTurno);
            });
        }

        private void colocarTurnoText(bool miTurno)
        {
            _tuTurno = miTurno ? "Es tu turno" : "Es el turno del rival";
            NotifyPropertyChanged(nameof(TuTurno));
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // Mover todos los rombos dinámicos que estén en movimiento
            foreach (var rombo in _dynamicRombos)
            {
                if (rombo.IsMoving)
                {
                    rombo.Move();
                }
            }

            // Actualizar el dibujo
            graphicsView.Invalidate();
        }

        private async void OnScreenTapped()
        {
            if (_dynamicRombos.Count > 0)
            {
                _dynamicRombos[^1].IsMoving = false; // Detener el último rombo

                // El ajuste del rombo (recorte y posición) se hace en RomboDrawable al dibujar

                // Guardar la posición final ajustada
                (float x, float y) = _dynamicRombos[^1].GetStoppedPosition();
                await GlobalConnection.connection.InvokeCoreAsync("GetLastRomboPosition", args: new object[]
                { nameRoom, x, y });

                // Verificar cambio de turno
                bool result = await GlobalConnection.connection.InvokeAsync<bool>("StopGameAndCheckTurn", nameRoom);

                if (result)
                {
                    miTurno = false;
                    NotifyPropertyChanged(nameof(MiTurno));
                    tappedScreenCommand.RaiseCanExecuteChanged();
                }
                else
                {
                    Console.WriteLine("No hay oponente en la sala.");
                }
            }
        }

        private async void CreateRombo()
        {
            Color color = _colorInicial;

            //await Application.Current.MainPage.DisplayAlert("Error", MiTurno.ToString(), "Aceptar");
            if (miTurno)
            {
                color = GetRandomColor();
                string colorHex = color.ToHex();

                await GlobalConnection.connection.InvokeCoreAsync("PintaRombo", args: new[]
                { nameRoom, colorHex });
            }

            var newRombo = new DynamicRombo
            {
                OffsetX = 0,
                OffsetY = 0,
                MoveStep = 2, // Velocidad de movimiento
                IsMoving = true, // Comenzar a moverse
                Color = color // Asignar un color aleatorio
            };
            _dynamicRombos.Add(newRombo);
        }

        private Color GetRandomColor()
        {
            // Generar un color aleatorio
            Random random = new Random();
            return new Color(
                random.NextSingle(), // Componente rojo (0 a 1)
                random.NextSingle(), // Componente verde (0 a 1)
                random.NextSingle()  // Componente azul (0 a 1)
            );
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