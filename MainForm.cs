using System;
using System.Drawing;
using System.Windows.Forms;

namespace MahjongApp
{
    public partial class MainForm : Form
    {
        private MahjongGameManager gameManager;

        public MainForm()
        {
            InitializeComponent();

            gameManager = new MahjongGameManager();
            DisplayPlayerHand();
        }

        private void DisplayPlayerHand()
        {

            List<Tile> hand = gameManager.GetHumanPlayerHand();
            int startX = 10;
            int startY = 300;
            int tileWidth = 60;
            int tileHeight = 80; 

            for (int i = 0; i < hand.Count; i++)
            {
                Tile tile = hand[i];

                var tilePictureBox = new ObservableTilePictureBox
                {
                    Image = tile.GetImage(),
                    Size = new Size(tileWidth, tileHeight),
                    Location = new Point(startX + i * tileWidth, startY),
                    BorderStyle = BorderStyle.FixedSingle,
                };

                tilePictureBox.RegisterObserver(new TileClickLogger(tile.Name()));

                Controls.Add(tilePictureBox);
            }
        }
    }
}
