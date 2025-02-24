class Program
{
    public static void Main()
    {
        UI UI = new UI();
        UI.ShowWelcomeScreen();
        Game game = UI.CreateGame();
        UI.PlayGame(game);
        UI.ThankYouMsg();
    }
}
