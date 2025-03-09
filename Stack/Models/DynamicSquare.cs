using Stack.Enums;

namespace Models
{
    public class DynamicSquare
    {
        public float OffsetX { get; set; } = 0;
        public float OffsetY { get; set; } = 0;
        public float MoveStep { get; set; } // Velocidad de movimiento
        public bool IsMoving { get; set; } = true; // Estado de movimiento
        public MovementDirection MovementDirection { get; set; } = MovementDirection.Vertical; // Dirección de movimiento

        public Color Color { get; set; } // Color del cuadrado

        public float MoveLimit { get; set; } // Límite del área de movimiento

        public float Width { get; set; }
        public float Height { get; set; }

        public void Move()
        {
            if (!IsMoving) return; // No mover si está detenido

            // Mover el cuadrado en la dirección especificada
            switch (MovementDirection)
            {
                case MovementDirection.Vertical:
                    OffsetY += MoveStep; // Mover verticalmente
                    break;

                case MovementDirection.Horizontal:
                    OffsetX += MoveStep; // Mover horizontalmente
                    break;
            }

            // Si el cuadrado supera el límite, invertir la dirección
            if (MovementDirection == MovementDirection.Vertical)
            {
                if (OffsetY > MoveLimit || OffsetY < -MoveLimit)
                {
                    MoveStep = -MoveStep; // Invertir la dirección vertical
                }
            }
            else if (MovementDirection == MovementDirection.Horizontal)
            {
                if (OffsetX > MoveLimit || OffsetX < -MoveLimit)
                {
                    MoveStep = -MoveStep; // Invertir la dirección horizontal
                }
            }
        }

        // Función para obtener la posición en la que el cuadrado se ha detenido
        public (float x, float y) GetStoppedPosition()
        {
            // Solo devuelve la posición si el cuadrado está detenido
            if (!IsMoving)
            {
                return (OffsetX, OffsetY); // Devuelve la posición actual
            }

            // Si el cuadrado está en movimiento, puedes devolver un valor nulo o predeterminado
            return (0, 0); // Valor por defecto en caso de que el cuadrado esté en movimiento
        }

        // **Nuevo método para sincronizar la posición con el servidor**
        public void SetPosition(float x, float y, float width, float height)
        {
            OffsetX = 0;
            OffsetY = 0;
            Width = width;
            Height = height;
            MoveLimit = MovementDirection == MovementDirection.Horizontal ? width * 2 : height * 2;
            IsMoving = false; // Asegurar que se detiene correctamente
        }
    }
}