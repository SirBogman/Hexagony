using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace Hexagony
{
    class HexagonyEnv
    {
        private readonly Memory _memory = new();
        private readonly Grid _grid;
        private readonly PointAxial[] _ips;
        private readonly Direction[] _ipDirs;
        private readonly byte[] _input;
        private int _activeIp;
        private int _inputIndex;
        private int _tick;
        private int _debugLevel;

        public HexagonyEnv(string source, string input, int debugLevel)
        {
            _grid = Grid.Parse(source);
            _input = input.ToUtf8();
            _debugLevel = debugLevel;
            _ips = Ut.NewArray(
                new PointAxial(0, -_grid.Size + 1),
                new PointAxial(_grid.Size - 1, -_grid.Size + 1),
                new PointAxial(_grid.Size - 1, 0),
                new PointAxial(0, _grid.Size - 1),
                new PointAxial(-_grid.Size + 1, _grid.Size - 1),
                new PointAxial(-_grid.Size + 1, 0));
            _ipDirs = Ut.NewArray(
                Direction.East,
                Direction.SouthEast,
                Direction.SouthWest,
                Direction.West,
                Direction.NorthWest,
                Direction.NorthEast);
        }

        public void Run()
        {
            foreach (var _ in GetProgram())
            {
            }
        }

        private Direction Dir => _ipDirs[_activeIp];
        private PointAxial Coords => _ips[_activeIp];

        private IEnumerable<Position> GetProgram()
        {
            if (_grid.Size == 0)
                yield break;

            while (true)
            {
                yield return _grid.GetPosition(Coords);

                // Execute the current instruction
                var newIp = _activeIp;
                var (opcode, debug) = _grid[Coords];
                var debugTick = _debugLevel > 1 || (_debugLevel == 1 && debug);

                if (debugTick)
                {
                    OutputDebugInfo();
                    if (opcode.Value == '@')
                    {
                        OutputDebugString(_memory.ToDebugString());
                    }
                }

                switch (opcode.Value)
                {
                    // NOP
                    case '.': break;

                    // Terminate
                    case '@': yield break;

                    // Arithmetic
                    case ')': _memory.Set(_memory.Get() + 1); break;
                    case '(': _memory.Set(_memory.Get() - 1); break;
                    case '+': _memory.Set(_memory.GetLeft() + _memory.GetRight()); break;
                    case '-': _memory.Set(_memory.GetLeft() - _memory.GetRight()); break;
                    case '*': _memory.Set(_memory.GetLeft() * _memory.GetRight()); break;
                    case '~': _memory.Set(-_memory.Get()); break;

                    case ':':
                    case '%':
                        var leftVal = _memory.GetLeft();
                        var rightVal = _memory.GetRight();
                        BigInteger rem;
                        var div = BigInteger.DivRem(leftVal, rightVal, out rem);
                        // The semantics of integer division and modulo are different in Hexagony because the
                        // reference interpreter was written in Ruby. Account for this discrepancy.
                        if (rem != 0 && leftVal < 0 ^ rightVal < 0)
                        {
                            rem += rightVal;
                            div--;
                        }
                        _memory.Set(opcode.Value == ':' ? div : rem);
                        break;

                    // Memory manipulation
                    case '{': _memory.MoveLeft(); break;
                    case '}': _memory.MoveRight(); break;
                    case '=': _memory.Reverse(); break;
                    case '"': _memory.Reverse(); _memory.MoveRight(); _memory.Reverse(); break;
                    case '\'': _memory.Reverse(); _memory.MoveLeft(); _memory.Reverse(); break;
                    case '^':
                        if (_memory.Get() > 0)
                            _memory.MoveRight();
                        else
                            _memory.MoveLeft();
                        break;
                    case '&':
                        if (_memory.Get() > 0)
                            _memory.Set(_memory.GetRight());
                        else
                            _memory.Set(_memory.GetLeft());
                        break;

                    // I/O
                    case ',':
                        if (_inputIndex >= _input.Length)
                            _memory.Set(BigInteger.MinusOne);
                        else
                        {
                            _memory.Set(_input[_inputIndex]);
                            _inputIndex++;
                        }
                        break;

                    case ';':
                        AppendOutput((char)(_memory.Get() % 256));
                        break;

                    case '?':
                        _memory.Set(FindInteger());
                        break;

                    case '!':
                        AppendOutput(_memory.Get().ToString());
                        break;

                    // Control flow
                    case '_': _ipDirs[_activeIp] = Dir.ReflectAtUnderscore; break;
                    case '|': _ipDirs[_activeIp] = Dir.ReflectAtPipe; break;
                    case '/': _ipDirs[_activeIp] = Dir.ReflectAtSlash; break;
                    case '\\': _ipDirs[_activeIp] = Dir.ReflectAtBackslash; break;
                    case '<': _ipDirs[_activeIp] = Dir.ReflectAtLessThan(_memory.Get() > 0); break;
                    case '>': _ipDirs[_activeIp] = Dir.ReflectAtGreaterThan(_memory.Get() > 0); break;
                    case ']': newIp = (_activeIp + 1) % 6; break;
                    case '[': newIp = (_activeIp + 5) % 6; break;
                    case '#': newIp = ((int) (_memory.Get() % 6) + 6) % 6; break;
                    case '$': _ips[_activeIp] += Dir.Vector; HandleEdges(); break;

                    // Digits, letters, and other characters.
                    default:
                        if (opcode.Value >= '0' && opcode.Value <= '9')
                        {
                            var opVal = opcode.Value - '0';
                            var memVal = _memory.Get();
                            _memory.Set(memVal * 10 + (memVal < 0 ? -opVal : opVal));
                        }
                        else
                            _memory.Set(opcode.Value);
                        break;
                }

                if (debugTick)
                {
                    OutputDebugString($"New direction: {Dir}");
                    OutputDebugString(_memory.ToDebugString());
                }

                _ips[_activeIp] += Dir.Vector;
                HandleEdges();
                _activeIp = newIp;
                _tick++;
            }
        }

        private BigInteger FindInteger()
        {
            var chs = "0123456789+-".Select(c => (byte) c).ToArray();
            while (_inputIndex < _input.Length && !chs.Contains(_input[_inputIndex]))
                _inputIndex++;
            if (_inputIndex == _input.Length)
                return BigInteger.Zero;
            var sb = new StringBuilder();
            if (_input[_inputIndex] == '-' || _input[_inputIndex] == '+')
            {
                if (_input[_inputIndex] == '-')
                    sb.Append('-');
                _inputIndex++;
            }
            if (_inputIndex == _input.Length)
                return BigInteger.Zero;
            while (_inputIndex < _input.Length && _input[_inputIndex] >= '0' && _input[_inputIndex] <= '9')
            {
                sb.Append((char) _input[_inputIndex]);
                _inputIndex++;
            }
            return BigInteger.Parse(sb.ToString());
        }

        private void HandleEdges()
        {
            if (_grid.Size == 1)
            {
                _ips[_activeIp] = new PointAxial(0, 0);
                return;
            }

            var(x, z) = Coords;
            var y = -x - z;

            if (Ut.Max(Math.Abs(x), Math.Abs(y), Math.Abs(z)) < _grid.Size)
                return;

            var xBigger = Math.Abs(x) >= _grid.Size;
            var yBigger = Math.Abs(y) >= _grid.Size;
            var zBigger = Math.Abs(z) >= _grid.Size;

            // Move the pointer back to the hex near the edge
            _ips[_activeIp] -= Dir.Vector;

            // If two values are still in range, we are wrapping around an edge (not a corner).
            if (!xBigger && !yBigger)
                _ips[_activeIp] = new PointAxial(Coords.Q + Coords.R, -Coords.R);
            else if (!yBigger && !zBigger)
                _ips[_activeIp] = new PointAxial(-Coords.Q, Coords.Q + Coords.R);
            else if (!zBigger && !xBigger)
                _ips[_activeIp] = new PointAxial(-Coords.R, -Coords.Q);
            else
            {
                // If two values are out of range, we navigated into a corner.
                // We teleport to a location that depends on the current memory value.
                var isPositive = _memory.Get() > 0;

                if (!xBigger && !isPositive || !yBigger && isPositive)
                    _ips[_activeIp] = new PointAxial(Coords.Q + Coords.R, -Coords.R);
                else if (!yBigger || !zBigger && isPositive)
                    _ips[_activeIp] = new PointAxial(-Coords.Q, Coords.Q + Coords.R);
                else if (!zBigger || !xBigger)
                    _ips[_activeIp] = new PointAxial(-Coords.R, -Coords.Q);
            }
        }

        private void AppendOutput(char output) =>
            Console.Write(output);

        private void AppendOutput(string output) =>
            Console.Write(output);

        private void OutputDebugString(string output) =>
            Console.Error.WriteLine(output);

        private void OutputDebugInfo()
        {
            OutputDebugString($"Tick {_tick}");
            OutputDebugString(_grid.ToDebugString());
            var i = 0;
            foreach (var (q, r) in _ips)
            {
                var active = _activeIp == i ? " (active)" : null;
                OutputDebugString($"IP #{i}: (Q: {q,3}, R: {r,3}, Dir: {_ipDirs[i],2}){active}");
                i++;
            }
        }
    }
}
