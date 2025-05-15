namespace MahjongApp
{
    public class RemainingTileIndicator : Label
    {
        int RemainingTiles = 70;
        public RemainingTileIndicator()
        {
            int Width = 72;
            int Height = 36;
            this.Text = $"余{this.RemainingTiles}";
            this.TextAlign = ContentAlignment.MiddleCenter;
            this.BackColor = Color.Black;
            this.ForeColor = Color.DodgerBlue;
            this.Font = new Font("HGP行書体", 17, FontStyle.Bold);
            this.Size = new Size(Width, Height);
        }

        public void UpdateRemainingTiles(int remainingTiles)
        {
            this.RemainingTiles = remainingTiles;
            this.Text = $"余{this.RemainingTiles}";
        }
    }
}