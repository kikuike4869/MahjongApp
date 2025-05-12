using System;
using System.Drawing;
using System.Windows.Forms;

namespace MahjongApp
{
    public partial class MainForm : Form
    {
        private MahjongGameManager gameManager;
        private List<ObservableTilePictureBox> TilePictureBoxes;

        public MainForm()
        {
            InitializeComponent();
            TilePictureBoxes = new List<ObservableTilePictureBox>();

            gameManager = new MahjongGameManager();
            gameManager.SetUpdateUICallBack(RefreshHandDisplay);
            DisplayPlayerHand();
            gameManager.Test();
        }

        private void DisplayPlayerHand()
        {
            HumanPlayer player = gameManager.GetHumanPlayer();
            List<Tile> hand = player.Hand;
            int tileWidth = 60;
            int tileHeight = 80;
            int startX = tileWidth;
            int startY = Config.Instance.ScreenSize.Height - 150 - (int)tileHeight / 2;

            foreach (var pictureBox in TilePictureBoxes)
            {
                Controls.Remove(pictureBox);
                pictureBox.Dispose();
            }
            TilePictureBoxes.Clear();

            for (int i = 0; i < hand.Count; i++)
            {
                Tile tile = hand[i];
                int selectedOffset = tile.IsSelected ? tileHeight / 2 : 0;
                int drawnTileOffset = ((player.IsTsumo) && (i == hand.Count - 1)) ? tileWidth : 0;


                var tilePictureBox = new ObservableTilePictureBox
                {
                    Image = tile.GetImage(),
                    Size = new Size(tileWidth, tileHeight),
                    Location = new Point(startX + i * tileWidth + drawnTileOffset, startY - selectedOffset),
                    BorderStyle = BorderStyle.FixedSingle,
                };

                tilePictureBox.RegisterObserver(new TileClickLogger(player, i, RefreshHandDisplay, gameManager));

                Controls.Add(tilePictureBox);
                TilePictureBoxes.Add(tilePictureBox);
            }
        }



        private void RefreshHandDisplay()
        {
            DisplayPlayerHand();
        }
    }
}
