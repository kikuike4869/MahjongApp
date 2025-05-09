
namespace MahjongApp
{
    public sealed class Config
    {
        private static readonly Lazy<Config> instance = new Lazy<Config>(() => new Config());

        public Size ScreenSize { get; set; }

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
        }
    }
}