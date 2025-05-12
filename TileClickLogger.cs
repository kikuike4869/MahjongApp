using System;

namespace MahjongApp
{
    public class TileClickLogger : ITilePictureBoxObserver
    {
        private Tile SelectedTile;
        private List<Tile> Hand;
        private Action RefreshHandDisplay;
        private MahjongGameManager GameManager;
        private HumanPlayer Player;

        public TileClickLogger(HumanPlayer player, int index, Action refreshHandDisplay, MahjongGameManager gameManager)
        {
            this.Player = player;
            this.Hand = this.Player.Hand;
            this.SelectedTile = this.Hand[index];
            this.RefreshHandDisplay = refreshHandDisplay;
            this.GameManager = gameManager;
        }

        public void OnPictureBoxClicked()
        {
            Console.WriteLine($"Tile {SelectedTile.Name()} was clicked!");


            if (SelectedTile.IsSelected)
            {
                if (GameManager.CurrentPhase == GamePhase.DiscardPhase)
                {
                    Player.DiscardTile(SelectedTile);
                    RefreshHandDisplay?.Invoke();

                    GameManager.FinishTurn();
                }
                else
                {
                    Console.WriteLine("It's not yet the Phase to discard a tile!");
                }
            }
            else
            {
                foreach (var t in Hand)
                {
                    t.IsSelected = false;
                }
                SelectedTile.IsSelected = true;

                RefreshHandDisplay.Invoke();
            }
        }
    }
}
