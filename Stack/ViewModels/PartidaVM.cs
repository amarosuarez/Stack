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
        private SquareDrawable _romboDrawable;
        private System.Timers.Timer _timer;
        private const float MoveLimit = 100; // Límite del área de movimiento
        private List<DynamicSquare> _dynamicSquare = new List<DynamicSquare>();
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
        private (float lastOffsetX, float lastOffsetY) _lastRomboPosition = (0, 0);
        private DynamicSquare _staticSquare; // Cuadrado estático
        private float _width = 200;
        private float _height = 200;
        #endregion

        #region Propiedades
        public SquareDrawable Rombo
        {
            get { return _romboDrawable; }
            set { _romboDrawable = value; }
        }

        public System.Timers.Timer Timer
        {
            get { return _timer; }
            set { _timer = value; }
        }

        public List<DynamicSquare> DynamicRombos
        {
            get { return _dynamicSquare; }
            set { _dynamicSquare = value; }
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

        public DynamicSquare StaticSquare
        {
            get { return _staticSquare; }
            set { _staticSquare = value; }
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

            // Inicializar el cuadrado estático
            _staticSquare = new DynamicSquare
            {
                OffsetX = 0,
                OffsetY = 0,
                MoveStep = 0, // No se mueve
                IsMoving = false, // Estático
                Width = 200, // Tamaño del cuadrado estático
                Height = 200,
                Color = Colors.Green // Color del cuadrado estático
            };

            // Inicializar el dibujable del rombo
            _romboDrawable = new SquareDrawable(_dynamicSquare, _staticSquare); // Pasar el cuadrado estático
            graphicsView.Drawable = _romboDrawable;

            _dynamicSquare.Add(_staticSquare);

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
                    GlobalConnection.connection.On<string, float, float>("PintaRombo", pintaRombo);
                    GlobalConnection.connection.On<float, float, float, float, String>("GetLastRomboPosition", setLastRomboPosition);

                    // Obtener el connectionID del jugador actual
                    string currentConnectionID = await GlobalConnection.connection.InvokeAsync<string>("GetConnectionID");

                    // Obtener el connectionID del jugador cuyo turno es
                    string turnConnectionID = await GlobalConnection.connection.InvokeAsync<string>("GetCurrentTurnPlayer", nameRoom);

                    if (currentConnectionID == turnConnectionID)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            colocarTurnoText(true);
                            CreateRombo(_width, _height);
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

        private async void setLastRomboPosition(float posX, float posY, float width, float height, String color)
        {
            // **Sincronizar posición exacta desde el servidor**
            if (_dynamicSquare.Count > 0)
            {
                // **Eliminar el cuadrado anterior**
                _dynamicSquare.Clear();

                // **Crear un nuevo cuadrado con la posición, ancho y alto proporcionados**
                var newSquare = new DynamicSquare
                {
                    OffsetX = 0,
                    OffsetY = 0,
                    Width = width,
                    Height = height,
                    IsMoving = false, // No se mueve después de establecer la posición
                    Color = Color.FromArgb(color)
                };

                _width = width;
                _height = height;

                // **Agregar el nuevo cuadrado a la lista**
                _dynamicSquare.Add(newSquare);

                // **Actualizar la vista**
                graphicsView.Invalidate();
            }
        }

        private async void pintaRombo(string color, float width, float height)
        {
            _colorInicial = Color.FromArgb(color);
            _width = width;
            _height = height;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                CreateRombo(_width, _height);
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
                    if (_dynamicSquare.Count > 0)
                    {
                        _dynamicSquare[^1].IsMoving = false;
                    }
                    miTurno = false;
                }
                else
                {
                    miTurno = true;
                    CreateRombo(_width, _height);
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
            foreach (var rombo in _dynamicSquare)
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
            if (_dynamicSquare.Count > 0)
            {
                _dynamicSquare[^1].IsMoving = false; // Detener el último rombo

                // Guardar la posición final ajustada
                (float x, float y) = _dynamicSquare[^1].GetStoppedPosition();

                float height = _dynamicSquare[^1].Height;

                if (y != _lastRomboPosition.lastOffsetY)
                {
                    await Shell.Current.DisplayAlert("Atención", $"Sobresale", "Aceptar");

                    float finalY;

                    // Comprobamos si sobresale por arriba o por abajo
                    if (y > _lastRomboPosition.lastOffsetY)
                    {
                        // Sobresale por abajo
                        float excess = y - _lastRomboPosition.lastOffsetY;
                        height -= excess; // Reducir la altura
                        finalY = 0; // Colocar en la posición (0, 0)
                    }
                    else
                    {
                        // Sobresale por arriba
                        float excess = _lastRomboPosition.lastOffsetY - y;
                        height -= excess; // Reducir la altura
                        finalY = 0; // Colocar en la posición (0, 0)
                    }

                    if (height > 0)
                    {
                        await Shell.Current.DisplayAlert("Atención", $"Altura anterior {_dynamicSquare[^1].Height} Nueva altura {height}, offset anterior {_lastRomboPosition.lastOffsetY}, nuevo offset {finalY}", "Aceptar");

                        // Crear un nuevo cuadrado recortado en (0, 0)
                        var newRombo = new DynamicSquare
                        {
                            OffsetX = 0, // Posición X en (0, 0)
                            OffsetY = 0, // Posición Y en (0, 0)
                            MoveStep = 2, // Velocidad de movimiento
                            IsMoving = false, // No se mueve después de recortar
                            Width = _dynamicSquare[^1].Width, // Mantener el ancho
                            Height = height, // Nueva altura recortada
                            Color = _dynamicSquare[^1].Color // Mantener el color
                        };

                        // **Eliminar todos los cuadrados, incluido el estático**
                        _dynamicSquare.Clear();

                        // **Agregar el nuevo cuadrado recortado**
                        _dynamicSquare.Add(newRombo);

                        // Actualizar la vista
                        graphicsView.Invalidate();
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Atención", "La altura no es válida", "Aceptar");
                    }
                }
                else
                {
                    await Shell.Current.DisplayAlert("Atención", $"No Sobresale", "Aceptar");
                }

                _lastRomboPosition = (x, y);
                await GlobalConnection.connection.InvokeCoreAsync("GetLastRomboPosition", args: new object[]
                { nameRoom, x, y, 200, height, _dynamicSquare[^1].Color.ToHex() });

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

        private async void CreateRombo(float width, float height)
        {
            Color color = _colorInicial;

            //await Application.Current.MainPage.DisplayAlert("Error", MiTurno.ToString(), "Aceptar");
            if (miTurno)
            {
                color = GetRandomColor();
                string colorHex = color.ToHex();

                await GlobalConnection.connection.InvokeCoreAsync("PintaRombo", args: new object[]
                { nameRoom, colorHex, width, height });
            }

            var newRombo = new DynamicSquare
            {
                OffsetX = 0,
                OffsetY = 0,
                MoveStep = 2, // Velocidad de movimiento
                IsMoving = true, // Comenzar a moverse
                Width = width,
                Height = height,
                Color = color // Asignar un color aleatorio
            };
            _dynamicSquare.Add(newRombo);
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