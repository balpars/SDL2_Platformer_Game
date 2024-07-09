namespace Platformer_Game
{
    class Tile
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Id { get; private set; }

        public Tile(int x, int y, int id)
        {
            X = x;
            Y = y;
            Id = id;
        }
    }
}
