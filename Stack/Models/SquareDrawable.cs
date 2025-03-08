namespace Models
{
    public class SquareDrawable : IDrawable
    {
        private List<DynamicSquare> _dynamicSquares; // Cuadrados dinámicos
        private DynamicSquare _staticSquare; // Cuadrado estático
        private PointF[] _recortadoPoints; // Puntos de la figura recortada

        public SquareDrawable(List<DynamicSquare> dynamicSquares, DynamicSquare staticSquare)
        {
            _dynamicSquares = dynamicSquares;
            _staticSquare = staticSquare;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            // Calcular el centro de la pantalla
            float centerX = dirtyRect.Width / 2;
            float centerY = dirtyRect.Height / 2;

            // Dibujar el cuadrado estático (centrado y sin movimiento)
            //DrawSquare(canvas, centerX, centerY, _staticSquare.Width, _staticSquare.Height, _staticSquare.Color);

            // Dibujar todos los cuadrados dinámicos
            foreach (var square in _dynamicSquares)
            {
                // Asegurarse de que el ancho y el alto no sean 0 antes de dibujar
                if (square.Width > 0 && square.Height > 0)
                {
                    DrawSquare(canvas, centerX + square.OffsetX, centerY + square.OffsetY, square.Width, square.Height, square.Color);
                }
            }

            // Dibujar la figura recortada en la esquina superior derecha (si existe)
            if (_recortadoPoints != null && _recortadoPoints.Length > 0)
            {
                float margin = 20; // Margen desde la esquina superior derecha
                float offsetX = dirtyRect.Width - 100 - margin; // Posición X de la figura recortada
                float offsetY = margin; // Posición Y de la figura recortada

                // Dibujar la figura recortada
                DrawRecortado(canvas, offsetX, offsetY);
            }
        }

        private void DrawSquare(ICanvas canvas, float centerX, float centerY, float width, float height, Color color)
        {
            // Crear un PathF para el cuadrado
            PathF path = new PathF();
            path.MoveTo(centerX - width / 2, centerY - height / 2); // Esquina superior izquierda
            path.LineTo(centerX + width / 2, centerY - height / 2); // Esquina superior derecha
            path.LineTo(centerX + width / 2, centerY + height / 2); // Esquina inferior derecha
            path.LineTo(centerX - width / 2, centerY + height / 2); // Esquina inferior izquierda
            path.Close(); // Cerrar el path para completar el cuadrado

            // Dibujar el cuadrado
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