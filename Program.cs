namespace MahjongApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
            // new HandAnalysisResultTests().RunAllTests();
        }
    }
}