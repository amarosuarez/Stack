namespace Models
{
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
            if (!IsMoving) return; // No mover si está detenido

            // Mover el rombo en diagonal
            OffsetX += MoveStep;
            OffsetY += MoveStep;

            // Si el rombo supera el límite, invertir la dirección
            if (OffsetX > MoveLimit || OffsetX < -MoveLimit)
            {
                MoveStep = -MoveStep; // Invertir la dirección
            }
        }

        // Función para obtener la posición en la que el rombo se ha detenido
        public (float x, float y) GetStoppedPosition()
        {
            // Solo devuelve la posición si el rombo está detenido
            if (!IsMoving)
            {
                return (OffsetX, OffsetY); // Devuelve la posición actual
            }

            // Si el rombo está en movimiento, puedes devolver un valor nulo o predeterminado
            return (0, 0); // Valor por defecto en caso de que el rombo esté en movimiento
        }

        // **Nuevo método para sincronizar la posición con el servidor**
        public void SetPosition(float x, float y)
        {
            OffsetX = x;
            OffsetY = y;
            IsMoving = false; // Asegurar que se detiene correctamente
        }
    }
}
