namespace Hexagony
{
    public sealed class Position
    {
        public int Index { get; }
        public int Length { get; }

        public Position(int index, int length)
        {
            Index = index; Length = length;
        }
    }
}
