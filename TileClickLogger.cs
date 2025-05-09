using System;

namespace MahjongApp
{
    public class TileClickLogger : ITilePictureBoxObserver
    {
        private string tileName;

        public TileClickLogger(string tileName)
        {
            this.tileName = tileName;
        }

        public void OnPictureBoxClicked()
        {
            Console.WriteLine($"Tile {tileName} was clicked!");
        }
    }
}
