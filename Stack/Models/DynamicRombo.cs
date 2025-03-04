
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
}
