using System;

namespace Hexagony
{
    readonly struct PointAxial : IEquatable<PointAxial>
    {
        public int Q { get; }
        public int R { get; }
        public PointAxial(int q, int r) : this() { Q = q; R = r; }

        public static PointAxial operator +(PointAxial a, PointAxial b) =>
            new(a.Q + b.Q, a.R + b.R);

        public static PointAxial operator -(PointAxial a, PointAxial b) =>
            new(a.Q - b.Q, a.R - b.R);

        public override string ToString() =>
            $"(Q: {Q,3}, R: {R,3})";

        public static bool operator ==(PointAxial a, PointAxial b) =>
            a.Q == b.Q && a.R == b.R;

        public static bool operator !=(PointAxial a, PointAxial b) =>
            a.Q != b.Q || a.R != b.R;

        public override int GetHashCode() =>
            unchecked(Q * 24567 + R * 47);

        public override bool Equals(object obj) =>
            obj is PointAxial axial && this == axial;

        public bool Equals(PointAxial other) =>
            this == other;

        public void Deconstruct(out int q, out int r)
        {
            q = Q;
            r = R;
        }
    }
}
