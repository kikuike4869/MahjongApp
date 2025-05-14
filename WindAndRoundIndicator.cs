namespace MahjongApp
{
    public class WindAndRoundIndicator : Label
    {
        string Wind = "東";
        int Round = 1;
        public WindAndRoundIndicator()
        {
            int Width = 72;
            int Height = 36;
            this.Text = $"{this.Wind}{this.Round}局";
            this.TextAlign = ContentAlignment.MiddleCenter;
            this.BackColor = Color.Black;
            this.ForeColor = Color.DodgerBlue;
            this.Font = new Font("HGP行書体", 17, FontStyle.Bold);
            this.Size = new Size(Width, Height);
        }

        public void UpdateIndicator(Wind wind, int round)
        {
            this.Wind = ToJapanese(wind);
            this.Round = round;
            this.Text = $"{this.Wind}{this.Round}局";
        }

        public string ToJapanese(Wind wind)
        {
            return wind switch
            {
                Wind x when (int)x == 0 => "東",
                Wind x when (int)x == 1 => "南",
                Wind x when (int)x == 2 => "西",
                Wind x when (int)x == 3 => "北",
                _ => throw new ArgumentOutOfRangeException(nameof(wind), $"Unknown wind: {wind}"),
            };
        }
    }
}