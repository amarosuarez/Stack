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
        private bool _isOwner;
        private Color _colorInicial = null;
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
            set { 
                miTurno = value;
                tappedScreenCommand.RaiseCanExecuteChanged();
            }
        }

        public String NameRoom
        {
            get { return nameRoom; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    nameRoom = Uri.UnescapeDataString(value);
                    NotifyPropertyChanged();
                }
            }
        }

        public String Owner
        {
            get { return _owner; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _owner = Uri.UnescapeDataString(value);
                    miTurno = _owner != null ? _owner.Equals("true") : false;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(MiTurno));
                }
            }
        }
        #endregion

        #region Constructores
        public PartidaVM() {
            // Inicia la conexión de SignalR de forma asíncrona
            Task.Run(async () => await InitConnection());
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

            // Crear un rombo inicial
            CreateRombo();
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
                    GlobalConnection.connection.On<Color>("PintaRombo", pintaRombo);
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

        private void pintaRombo(Color color)
        {
            _colorInicial = Colors.Red;
        }

        private async Task checkTurn(String turno)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                // Obtener el connectionID del jugador actual
                string currentConnectionID = await GlobalConnection.connection.InvokeAsync<string>("GetConnectionID");

                // Obtener el connectionID del jugador cuyo turno es
                string turnConnectionID = await GlobalConnection.connection.InvokeAsync<string>("GetCurrentTurnPlayer", nameRoom);

                // Comparar los connectionIDs para verificar si es tu turno
                if (currentConnectionID == turnConnectionID)
                {
                    // Si el connectionID coincide, es tu turno
                    miTurno = true;
                }
                else
                {
                    // Si no coincide, no es tu turno
                    miTurno = false;
                }

                // Notificar cambios en la propiedad MiTurno para que se actualice la UI
                NotifyPropertyChanged(nameof(MiTurno));
            });
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
                await GlobalConnection.StartConnection();
                _dynamicRombos[^1].IsMoving = false; // Detener el último rombo

                // Guardar los puntos del rombo actual
                _currentRomboPoints = GetRomboPoints(_dynamicRombos[^1]);

                // Si hay un rombo anterior, comparar los puntos
                if (_dynamicRombos.Count > 1)
                {
                    _lastRomboPoints = GetRomboPoints(_dynamicRombos[^2]); // Guardar los puntos del rombo anterior

                    // Comparar los puntos de los rombos
                    if (!CheckOverlap(_lastRomboPoints, _currentRomboPoints))
                    {
                        // Mostrar un mensaje si no hay superposición
                        //DisplayAlert("Atención", "El nuevo rombo no encaja con el anterior.", "Aceptar");
                    }
                    else
                    {
                        // Calcular la figura recortada
                        _recortadoPoints = CalcularRecortado(_lastRomboPoints, _currentRomboPoints);

                        // Forzar el redibujado para mostrar la figura recortada
                        graphicsView.Invalidate();
                    }
                }

                // Llamada al método del Hub para verificar el cambio de turno
                bool result = await GlobalConnection.connection.InvokeAsync<bool>("StopGameAndCheckTurn", nameRoom);

                if (result)
                {
                    // Si es tu turno, enviar el color del nuevo rombo
                    if (miTurno)
                    {
                        var colorRombo = _dynamicRombos[^1].Color;
                        //await GlobalConnection.connection.InvokeAsync("PintaRomboi", nameRoom, colorRombo);  // Enviar color al Hub
                    }

                    miTurno = false;
                    NotifyPropertyChanged(nameof(MiTurno));
                    tappedScreenCommand.RaiseCanExecuteChanged();
                }
                else
                {
                    Console.WriteLine("No hay oponente en la sala.");
                }
            }

            // Crear un nuevo rombo dinámico en el centro con un color aleatorio
            CreateRombo();
        }

        private async void CreateRombo()
        {
            Color color = _colorInicial;

            await Application.Current.MainPage.DisplayAlert("Error", MiTurno.ToString(), "Aceptar");
            if (miTurno)
            {
                color = GetRandomColor();

                await GlobalConnection.connection.InvokeCoreAsync("PintaRombo", args: new[]
                { color });
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

        private bool CheckOverlap(PointF[] rombo1Points, PointF[] rombo2Points)
        {
            // Verificar si algún punto de rombo2 coincide con rombo1
            foreach (var point in rombo2Points)
            {
                if (IsPointInsideRombo(point, rombo1Points))
                {
                    return true; // Hay superposición
                }
            }

            return false; // No hay superposición
        }

        private PointF[] CalcularRecortado(PointF[] lastRomboPoints, PointF[] currentRomboPoints)
        {
            // Calcular la línea superior del rombo anterior (A)
            float lastTop = lastRomboPoints[0].Y; // Línea superior del rombo A

            // Calcular la línea derecha del rombo actual (B)
            float currentRight = currentRomboPoints[1].X; // Línea derecha del rombo B

            // Calcular los puntos de la figura recortada
            return new PointF[]
            {
                new PointF(lastRomboPoints[1].X, lastTop), // Esquina superior derecha del rombo A
                new PointF(currentRight, lastTop),         // Esquina superior derecha del rombo B
                new PointF(currentRight, currentRomboPoints[2].Y), // Esquina inferior derecha del rombo B
                new PointF(lastRomboPoints[1].X, currentRomboPoints[2].Y) // Esquina inferior derecha del rombo A
            };
        }

        private PointF[] GetRomboPoints(DynamicRombo rombo)
        {
            // Calcular los puntos del rombo
            float centerX = 0 + rombo.OffsetX; // El centro del rombo dinámico
            float centerY = 0 + rombo.OffsetY; // El centro del rombo dinámico
            float width = 100;
            float height = 100;

            return new PointF[]
            {
                new PointF(centerX, centerY - height / 2), // Arriba
                new PointF(centerX + width / 2, centerY),  // Derecha
                new PointF(centerX, centerY + height / 2), // Abajo
                new PointF(centerX - width / 2, centerY)   // Izquierda
            };
        }

        private bool IsPointInsideRombo(PointF point, PointF[] romboPoints)
        {
            // Verificar si un punto está dentro de un rombo usando el método de ray casting
            int intersections = 0;
            for (int i = 0; i < romboPoints.Length; i++)
            {
                var p1 = romboPoints[i];
                var p2 = romboPoints[(i + 1) % romboPoints.Length];

                if (RayIntersectsSegment(point, p1, p2))
                {
                    intersections++;
                }
            }

            // Si el número de intersecciones es impar, el punto está dentro del rombo
            return intersections % 2 != 0;
        }

        private bool RayIntersectsSegment(PointF point, PointF p1, PointF p2)
        {
            // Verificar si un rayo horizontal desde el punto intersecta el segmento p1-p2
            if (p1.Y > p2.Y)
            {
                (p1, p2) = (p2, p1); // Asegurar que p1.Y <= p2.Y
            }

            if (point.Y < p1.Y || point.Y > p2.Y)
            {
                return false; // El rayo no intersecta el segmento
            }

            if (point.X > Math.Max(p1.X, p2.X))
            {
                return false; // El rayo está a la derecha del segmento
            }

            if (p1.Y == p2.Y)
            {
                return point.X >= Math.Min(p1.X, p2.X); // Segmento horizontal
            }

            // Calcular la intersección
            float xIntersect = (point.Y - p1.Y) * (p2.X - p1.X) / (p2.Y - p1.Y) + p1.X;
            return point.X <= xIntersect;
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
