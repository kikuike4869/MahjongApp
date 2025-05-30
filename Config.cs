
namespace MahjongApp
{
    public sealed class Config
    {
        private static readonly Lazy<Config> instance = new Lazy<Config>(() => new Config());

        public Size ScreenSize { get; set; }
        public int NumberOfPlayers { get; set; }
        public int NumberOfFirstHands { get; set; }
        public int DiscardTileWidth { get; set; }
        public int DiscardTileHeight { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public int AiThinkTimeMs { get; set; } = 0;

        public static Config Instance
        {
            get
            {
                return instance.Value;
            }
        }

        private Config()
        {
            int width = 1280;
            int height = 920;

            ScreenSize = new Size(width, height);
            NumberOfPlayers = 4;
            NumberOfFirstHands = 13;
            DiscardTileWidth = 36;
            DiscardTileHeight = 48;
            TileWidth = 60;
            TileHeight = 80;
        }
    }
}