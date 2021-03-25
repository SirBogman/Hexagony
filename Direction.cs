using System;

namespace Hexagony
{
    abstract class Direction : IEquatable<Direction>
    {
        public static readonly Direction East = new East();
        public static readonly Direction SouthEast = new SouthEast();
        public static readonly Direction SouthWest = new SouthWest();
        public static readonly Direction West = new West();
        public static readonly Direction NorthWest = new NorthWest();
        public static readonly Direction NorthEast = new NorthEast();

        public abstract Direction ReflectAtSlash { get; }
        public abstract Direction ReflectAtBackslash { get; }
        public abstract Direction ReflectAtUnderscore { get; }
        public abstract Direction ReflectAtPipe { get; }
        public abstract Direction ReflectAtLessThan(bool positive);
        public abstract Direction ReflectAtGreaterThan(bool positive);

        public abstract PointAxial Vector { get; }

        public abstract bool Equals(Direction other);
        public override bool Equals(object obj) => obj is Direction other && Equals(other);
        public static bool operator ==(Direction a, Direction b) => a is not null && a.Equals(b);
        public static bool operator !=(Direction a, Direction b) => a is not null && !a.Equals(b);
        public abstract override int GetHashCode();
    }

    sealed class NorthEast : Direction
    {
        public override Direction ReflectAtSlash => NorthEast;

        public override Direction ReflectAtBackslash => West;

        public override Direction ReflectAtUnderscore => SouthEast;

        public override Direction ReflectAtPipe => NorthWest;

        public override Direction ReflectAtLessThan(bool positive) => SouthWest;
        public override Direction ReflectAtGreaterThan(bool positive) => East;

        public override PointAxial Vector => new(1, -1);

        public override int GetHashCode() => 245;
        public override bool Equals(Direction other) => other is NorthEast;
        public override string ToString() => "NE";
    }

    sealed class NorthWest : Direction
    {
        public override Direction ReflectAtSlash => East;

        public override Direction ReflectAtBackslash => NorthWest;

        public override Direction ReflectAtUnderscore => SouthWest;

        public override Direction ReflectAtPipe => NorthEast;

        public override Direction ReflectAtLessThan(bool positive) => West;
        public override Direction ReflectAtGreaterThan(bool positive) => SouthEast;

        public override PointAxial Vector => new(0, -1);

        public override int GetHashCode() => 2456;
        public override bool Equals(Direction other) => other is NorthWest;
        public override string ToString() => "NW";
    }

    sealed class West : Direction
    {
        public override Direction ReflectAtSlash => SouthEast;

        public override Direction ReflectAtBackslash => NorthEast;

        public override Direction ReflectAtUnderscore => West;

        public override Direction ReflectAtPipe => East;

        public override Direction ReflectAtLessThan(bool positive) => East;
        public override Direction ReflectAtGreaterThan(bool positive) => positive ? NorthWest : SouthWest;

        public override PointAxial Vector => new(-1, 0);

        public override int GetHashCode() => 24567;
        public override bool Equals(Direction other) => other is West;
        public override string ToString() => "W";
    }

    sealed class SouthWest : Direction
    {
        public override Direction ReflectAtSlash => SouthWest;

        public override Direction ReflectAtBackslash => East;

        public override Direction ReflectAtUnderscore => NorthWest;

        public override Direction ReflectAtPipe => SouthEast;

        public override Direction ReflectAtLessThan(bool positive) => West;
        public override Direction ReflectAtGreaterThan(bool positive) => NorthEast;

        public override PointAxial Vector => new(-1, 1);

        public override int GetHashCode() => 245678;
        public override bool Equals(Direction other) => other is SouthWest;
        public override string ToString() => "SW";
    }

    sealed class SouthEast : Direction
    {
        public override Direction ReflectAtSlash => West;

        public override Direction ReflectAtBackslash => SouthEast;

        public override Direction ReflectAtUnderscore => NorthEast;

        public override Direction ReflectAtPipe => SouthWest;

        public override Direction ReflectAtLessThan(bool positive) => NorthWest;
        public override Direction ReflectAtGreaterThan(bool positive) => East;

        public override PointAxial Vector => new(0, 1);

        public override int GetHashCode() => 2456783;
        public override bool Equals(Direction other) => other is SouthEast;
        public override string ToString() => "SE";
    }

    sealed class East : Direction
    {
        public override Direction ReflectAtSlash => NorthWest;

        public override Direction ReflectAtBackslash => SouthWest;

        public override Direction ReflectAtUnderscore => East;

        public override Direction ReflectAtPipe => West;

        public override Direction ReflectAtLessThan(bool positive) => positive ? SouthEast : NorthEast;
        public override Direction ReflectAtGreaterThan(bool positive) => West;

        public override PointAxial Vector => new(1, 0);

        public override int GetHashCode() => 24567837;
        public override bool Equals(Direction other) => other is East;
        public override string ToString() => "E";
    }
}
