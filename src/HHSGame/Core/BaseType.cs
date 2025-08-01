namespace HHSGame.Core
{
    public sealed record Coordinate(int X, int Y)
    {
        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        public (int x, int y) ToTuple => (X, Y);

        public Coordinate Target(int dx, int dy)
        {
            return new(X + dx, Y + dy);
        }

        public Coordinate Move(Vec2D vec)
        {
            return Target(vec.X, vec.Y);
        }
    }

    public sealed record Vec2D(int X, int Y)
    {
        public static Vec2D Zero => new(0, 0);

        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        public static Vec2D operator +(Vec2D self, Vec2D other)
        {
            return new Vec2D(self.X + other.X, self.Y + other.Y);
        }
    }

    public enum Direction : byte
    {
        Up,
        Down,
        Left,
        Right
    }

    public abstract record Move
    {
        private Move() { }
        public sealed record None() : Move;
        public sealed record Forward(Direction Direction) : Move;
        public sealed record Diagonal(Direction First, Direction Second) : Move;

        public Vec2D ToVec()
        {
            return this switch
            {
                Forward f => f.Direction.ToVec(),
                Diagonal d => d.First.ToVec() + d.Second.ToVec(),
                _ => Vec2D.Zero
            };
        }
    }

    public enum ExtendedDirection : byte
    {
        Up,
        Down,
        Left,
        Right,
        UpLeft,
        UpRight,
        DownLeft,
        DownRight,
    }


    public static class DirectionExtensions
    {
        public static Vec2D ToVec(this Direction direction)
        {
            return direction switch
            {
                Direction.Up => new Vec2D(0, -1),
                Direction.Down => new Vec2D(0, 1),
                Direction.Left => new Vec2D(-1, 0),
                Direction.Right => new Vec2D(1, 0),
                _ => Vec2D.Zero
            };
        }

        public static Move ToDirections(this ExtendedDirection extendedDirection)
        {
            return extendedDirection switch
            {
                ExtendedDirection.Up => new Move.Forward(Direction.Up),
                ExtendedDirection.Down => new Move.Forward(Direction.Down),
                ExtendedDirection.Left => new Move.Forward(Direction.Left),
                ExtendedDirection.Right => new Move.Forward(Direction.Right),
                ExtendedDirection.UpLeft => new Move.Diagonal(Direction.Up, Direction.Left),
                ExtendedDirection.UpRight => new Move.Diagonal(Direction.Up, Direction.Right),
                ExtendedDirection.DownLeft => new Move.Diagonal(Direction.Down, Direction.Left),
                ExtendedDirection.DownRight => new Move.Diagonal(Direction.Down, Direction.Right),
                _ => new Move.None()
            };
        }
    }

    public interface IBasicTraits
    {
        int Health { get; set; }
    }
}
