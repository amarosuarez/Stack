namespace Models
{
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
