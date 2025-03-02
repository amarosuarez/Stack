using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Timers;

namespace Stack.Pages
{
    public partial class MainPage : ContentPage
    {
        private RomboDrawable _romboDrawable;
        private System.Timers.Timer _timer;
        private const float MoveLimit = 100; // Límite del área de movimiento
        private List<DynamicRombo> _dynamicRombos = new List<DynamicRombo>();
        private PointF[] _lastRomboPoints; // Puntos del rombo anterior
        private PointF[] _currentRomboPoints; // Puntos del rombo actual
        private PointF[] _recortadoPoints; // Puntos de la figura recortada

        public MainPage()
        {
            InitializeComponent();

            // Inicializar el dibujable del rombo
            _romboDrawable = new RomboDrawable(_dynamicRombos);
            RomboGraphicsView.Drawable = _romboDrawable;

            // Configurar un temporizador para animar los rombos
            _timer = new System.Timers.Timer(16); // ~60 FPS
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = true;
            _timer.Enabled = true;

            // Crear un rombo inicial
            CreateRombo();
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
            RomboGraphicsView.Invalidate();
        }

        private void OnScreenTapped(object sender, EventArgs e)
        {
            // Detener el último rombo en movimiento (si existe)
            if (_dynamicRombos.Count > 0)
            {
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
                        DisplayAlert("Atención", "El nuevo rombo no encaja con el anterior.", "Aceptar");
                    }
                    else
                    {
                        // Calcular la figura recortada
                        _recortadoPoints = CalcularRecortado(_lastRomboPoints, _currentRomboPoints);

                        // Forzar el redibujado para mostrar la figura recortada
                        RomboGraphicsView.Invalidate();
                    }
                }
            }

            // Crear un nuevo rombo dinámico en el centro con un color aleatorio
            CreateRombo();
        }

        private void CreateRombo()
        {
            var newRombo = new DynamicRombo
            {
                OffsetX = 0,
                OffsetY = 0,
                MoveStep = 2, // Velocidad de movimiento
                IsMoving = true, // Comenzar a moverse
                Color = GetRandomColor() // Asignar un color aleatorio
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
    }

    public class DynamicRombo
    {
        public float OffsetX { get; set; } = 0;
        public float OffsetY { get; set; } = 0;
        public float MoveStep { get; set; } = 2; // Velocidad de movimiento
        public bool IsMoving { get; set; } = true; // Estado de movimiento
        public Color Color { get; set; } = Colors.Blue; // Color del rombo

        private const float MoveLimit = 100; // Límite del área de movimiento

        public void Move()
        {
            // Mover el rombo en diagonal
            OffsetX += MoveStep;
            OffsetY += MoveStep;

            // Si el rombo supera el límite, invertir la dirección
            if (OffsetX > MoveLimit || OffsetX < -MoveLimit)
            {
                MoveStep = -MoveStep; // Invertir la dirección
            }
        }
    }

    public class RomboDrawable : IDrawable
    {
        private List<DynamicRombo> _dynamicRombos;
        private PointF[] _recortadoPoints; // Puntos de la figura recortada

        public RomboDrawable(List<DynamicRombo> dynamicRombos)
        {
            _dynamicRombos = dynamicRombos;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            // Definir el tamaño del rombo
            float romboWidth = 100;

            // Calcular el centro de la pantalla
            float centerX = dirtyRect.Width / 2;
            float centerY = dirtyRect.Height / 2;

            // Dibujar el rombo estático (centrado y sin movimiento)
            DrawRombo(canvas, centerX, centerY, romboWidth, 100, Colors.Green);

            // Dibujar todos los rombos dinámicos
            foreach (var rombo in _dynamicRombos)
            {
                DrawRombo(canvas, centerX + rombo.OffsetX, centerY + rombo.OffsetY, romboWidth, 100, rombo.Color);
            }

            // Dibujar la figura recortada en la esquina superior derecha
            if (_recortadoPoints != null)
            {
                float margin = 20; // Margen desde la esquina superior derecha
                float offsetX = dirtyRect.Width - 100 - margin; // Posición X de la figura recortada
                float offsetY = margin; // Posición Y de la figura recortada

                // Dibujar la figura recortada
                DrawRecortado(canvas, offsetX, offsetY);
            }
        }

        private void DrawRombo(ICanvas canvas, float centerX, float centerY, float width, float height, Color color)
        {
            // Crear un PathF para el rombo
            PathF path = new PathF();
            path.MoveTo(centerX, centerY - height / 2); // Arriba
            path.LineTo(centerX + width / 2, centerY); // Derecha
            path.LineTo(centerX, centerY + height / 2); // Abajo
            path.LineTo(centerX - width / 2, centerY); // Izquierda
            path.Close(); // Cerrar el path para completar el rombo

            // Dibujar el rombo
            canvas.FillColor = color;
            canvas.StrokeColor = Colors.Black;
            canvas.StrokeSize = 2;
            canvas.FillPath(path);
            canvas.DrawPath(path);
        }

        private void DrawRecortado(ICanvas canvas, float offsetX, float offsetY)
        {
            // Crear un PathF para la figura recortada
            PathF path = new PathF();
            path.MoveTo(offsetX, offsetY); // Esquina superior izquierda
            path.LineTo(offsetX + 100, offsetY); // Esquina superior derecha
            path.LineTo(offsetX + 100, offsetY + 100); // Esquina inferior derecha
            path.LineTo(offsetX, offsetY + 100); // Esquina inferior izquierda
            path.Close(); // Cerrar el path para completar la figura

            // Dibujar la figura recortada
            canvas.FillColor = Colors.Red; // Color de la figura recortada
            canvas.StrokeColor = Colors.Black;
            canvas.StrokeSize = 2;
            canvas.FillPath(path);
            canvas.DrawPath(path);
        }
    }
}
