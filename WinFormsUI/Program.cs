namespace WindowsUI
{
    internal static class Program
    {
        public static void Main()
        {
            WindowsUI UI = new WindowsUI();
            UI.SetGameSettings();
            UI.Play();
        }
    }
}