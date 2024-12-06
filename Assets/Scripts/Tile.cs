

using UnityEngine;

namespace Bejeweled
{
    public class Tile
    {
        public Vector2 position;
        public Gem gem;
        public int tileIndexX;
        public int tileIndexY;
    
        public Tile(Vector2 position)
        {
            this.position = position;
        }
    
        public Tile(float x, float y, int tileindexX, int tileindexY)
        {
            position = new Vector2(x, y);
            tileIndexX = tileindexX;
            tileIndexY = tileindexY;
        }
    }
}