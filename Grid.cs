using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace Hexagony
{
    class Grid
    {
        public int Size { get; }
        private readonly (Rune rune, bool debug)[][] _grid;
        private readonly Position[][] _gridPositions;
        private readonly Position _endPos;

        public Grid(int size)
            : this(size, 0)
        {
        }

        private Grid(int size, int fileLength, IReadOnlyList<(Rune rune, Position position, bool debug)> data = null)
        {
            Size = size;

            // ReSharper disable AccessToDisposedClosure
            using (var e = data?.GetEnumerator())
                _grid = Ut.NewArray(2 * size - 1, j =>
                    Ut.NewArray(2 * size - 1 - Math.Abs(size - 1 - j), _ =>
                        e != null && e.MoveNext() ?
                            (e.Current.rune, e.Current.debug) :
                            (new Rune('.'), false)));

            _endPos = new Position(fileLength, 0);

            using (var e = data?.GetEnumerator())
                _gridPositions = Ut.NewArray(2 * size - 1, j =>
                    Ut.NewArray(2 * size - 1 - Math.Abs(size - 1 - j), _ =>
                        e != null && e.MoveNext() ? e.Current.position : _endPos));
        }

        public static Grid Parse(string input)
        {
            var index = 0;
            var debug = false;
            var data = new List<(Rune rune, Position position, bool debug)>();
            foreach (var rune in input.EnumerateRunes())
            {
                switch (rune.Value)
                {
                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n':
                        // Ignore specific whitespace chars.
                        continue;
                    case '`':
                        debug = true;
                        continue;
                }

                var position = new Position(index, rune.Utf16SequenceLength);
                index += position.Length;
                data.Add((rune, position, debug));
                debug = false;
            }

            var size = 1;
            while (3 * size * (size - 1) + 1 < data.Count)
                size++;
            return new Grid(size, input.Length, data);
        }

        public (Rune rune, bool debug) this[PointAxial coords]
        {
            get
            {
                var tup = AxialToIndex(coords);
                return tup == null ? (new Rune('.'), false) : _grid[tup.Item1][tup.Item2];
            }
        }

        private Tuple<int, int> AxialToIndex(PointAxial coords)
        {
            var (x, z) = coords;
            var y = -x - z;
            if (Ut.Max(Math.Abs(x), Math.Abs(y), Math.Abs(z)) >= Size)
                return null;

            var i = z + Size - 1;
            var j = x + Math.Min(i, Size - 1);
            return Tuple.Create(i, j);
        }

        public override string ToString() =>
            _grid.Select(line =>
                new string(' ', 2 * Size - line.Length) + line.Select(x => x.rune).JoinString(" "))
            .JoinString(Environment.NewLine);

        /// <summary>
        /// Return a string containing the grid and the range of coordinates for each row.
        /// </summary>
        public string ToDebugString() =>
            _grid
                .Select((line, index) =>
                {
                    var padding = new string(' ', 2 * Size - line.Length);
                    var row = index - Size + 1;
                    var q1 = Math.Max(1 - Size, -index);
                    var q2 = q1 + line.Length - 1;
                    return padding + line.Select(x => x.rune).JoinString(" ") + padding +
                        $"    Q: [{q1,3},{q2,3}], R: {row,2}";
                })
                .JoinString(Environment.NewLine);

        public Position GetPosition(PointAxial coords)
        {
            var tup = AxialToIndex(coords);
            return tup == null ? _endPos : _gridPositions[tup.Item1][tup.Item2];
        }
    }
}
