using System.Text.RegularExpressions;

public class UI
{
    Game? m_Game;

    public void ShowWelcomeScreen()
    {
        System.Console.WriteLine("Checkers! Press ENTER to start play...");
        string userInput = System.Console.ReadLine() ?? "No input";
        while (userInput != string.Empty)
        {
            userInput = System.Console.ReadLine() ?? "No input";
        }

        Ex02.ConsoleUtils.Screen.Clear();
    }

    public Game CreateGame()
    {
        string mainUserName = getUserName();
        string? secondUserName = null;

        int boardSize = getBoardSize();

        GameType gameType = getGameType();

        if (gameType == GameType.PlayerVsPlayer)
        {
            secondUserName = getUserName();
        }

        GameConfig gameConfig = new GameConfig(boardSize, gameType);

        Game game = new Game(mainUserName, secondUserName, gameConfig);

        return game;
    }

    public void PlayGame(Game i_Game)
    {
        m_Game = i_Game;
        bool gameInProgress = true;

        while (gameInProgress)
        {
            playMatch();

            bool rematch = askForRematch();

            if (rematch)
            {
                m_Game.RestartMatch();
            }

            else
            {
                gameInProgress = false;
            }
        }
    }

    private void playMatch()
    {
        while (m_Game?.GetMatchStatus() == eMatchStatus.OnGoing)
        {
            printCurrentGameState();

            printLastPlayedMove();

            playTurn();

            m_Game.CheckIfMatchEnded();
        }

        Ex02.ConsoleUtils.Screen.Clear();

        displayMatchResults();

    }

    private void playTurn()
    {
        bool turnWasMade = false;

        while (!turnWasMade)
        {
            if (m_Game?.IsCPUCurrentlyPlaying() ?? false)
            {
                displayContinueForCPUMoveMessage();
                m_Game.ExecutePlayForCPU();
                break;
            }

            string userInput = getValidUserTurnInput();

            if (userQuitted(ref userInput))
            {
                m_Game?.SetMatchStatusToQuitted();
                turnWasMade = true;
            }

            else
            {
                Move userRequestedMove = parseMoveFromUserInput(userInput);

                if (m_Game?.IsMoveAllowed(userRequestedMove) ?? false)
                {
                    m_Game.ExecuteMove();
                    turnWasMade = true;
                }

                else
                {
                    printInvalidMoveMsg();
                }
            }
        }
    }

    private void printCurrentGameState()
    {
        Ex02.ConsoleUtils.Screen.Clear();
        Checker?[,] board = m_Game.GetBoard();
        int boardSize = board.GetLength(0);
        char colChar = 'a';
        //print first line
        System.Console.Write(" ");
        for (int i = 0; i < boardSize; i++)
        {
            System.Console.Write("  " + colChar + " ");
            colChar = nextCharacter(colChar);
        }
        System.Console.Write("\n");

        //print separator
        int numOfSeparator = (4 * boardSize) + 1;
        System.Console.Write(" ");
        for (int i = 0;i < numOfSeparator; i++)
        {
            System.Console.Write("=");
        }
        System.Console.Write("\n");

        //print rows
        char rowChar = 'A';

        for (int i = 0; i< boardSize; i++)
        {
            //print row
            System.Console.Write(rowChar + "|");
            for(int j = 0; j < boardSize; j++)
            {
                string checkerToPrint = getCharToPrint(ref board[i, j]);
                System.Console.Write(' ' + checkerToPrint + " |");
            }

            System.Console.Write("\n");


            //print separator
            System.Console.Write(" ");

            for (int k = 0; k < numOfSeparator; k++)
            {
                System.Console.Write("=");
            }

            System.Console.Write("\n");

            rowChar = nextCharacter(rowChar);
        }

        System.Console.WriteLine();
    }

    private void printLastPlayedMove()
    {
        Move? lastPlayedMove = m_Game.GetLastMadeMove();
        if (lastPlayedMove != null)
        {
            string playedMove = unParseMoveToPrint(ref lastPlayedMove);
            string previousPlayer = m_Game.GetPreviousTurnPlayerName() ?? "CPU";
            eColors previousColorPlayed = m_Game.GetPreviousPlayerColor();
            string colorSymbol = previousColorPlayed == eColors.White ? "O" : "X";
            System.Console.WriteLine(previousPlayer + $"'s move was ({colorSymbol}): " + playedMove);
            System.Console.WriteLine();
        }
    }

    private void printHowMatchEnded()
    {
        eMatchStatus matchStatus = m_Game.GetMatchStatus();
        System.Console.WriteLine();
        switch (matchStatus)
        {
            case eMatchStatus.Winner:
                System.Console.WriteLine("There is a winner!");
                System.Console.WriteLine();
                break;
            case eMatchStatus.Tie:
                System.Console.WriteLine("The match ended in a tie!");
                System.Console.WriteLine();
                break;
            case eMatchStatus.Quitted:
                System.Console.WriteLine("Game quitted...");
                System.Console.WriteLine();
                break;
            default:
                break;
        }
    }

    private void printWinner()
    {
        string winnerName = m_Game.GetWinnerName() ?? "CPU";
        System.Console.WriteLine("The winner of the match is : " + winnerName);
        System.Console.WriteLine();
    }

    private void printOverallPlayerScores()
    {
        ((string,int),(string?, int)) playerScoresAndNames = m_Game.GetOverallPlayersScores();
        string player1Name = playerScoresAndNames.Item1.Item1;
        int player1OverallScore = playerScoresAndNames.Item1.Item2;
        string player2Name = playerScoresAndNames.Item2.Item1 ?? "CPU";
        int player2OverallScore = playerScoresAndNames.Item2.Item2;

        System.Console.WriteLine("The overall score is: ");
        System.Console.WriteLine();
        System.Console.WriteLine(player1Name + "'s overall score is: " + player1OverallScore);
        System.Console.WriteLine(player2Name + "'s overall score is: " + player2OverallScore);
        System.Console.WriteLine();
    }

    private void printInvalidMoveMsg()
    {
        System.Console.WriteLine("Invalid move! Try again.");
        System.Console.WriteLine();
    }

    private void displayMatchResults()
    {
        printHowMatchEnded();
        printWinner();
        printMatchScores();
        printOverallPlayerScores();

    }

    private void printMatchScores()
    {
        ((string, int), (string?, int)) matchScores = m_Game.GetCurrentMatchScoresAndUpdateOverall();
        string Player1Name = matchScores.Item1.Item1;
        int Player1MatchScore = matchScores.Item1.Item2;
        string Player2Name = matchScores.Item2.Item1 ?? "CPU";
        int Player2MatchScore = matchScores.Item2.Item2;

        System.Console.WriteLine("The match results are: ");
        System.Console.WriteLine();
        System.Console.WriteLine(Player1Name + "'s match score is: " + Player1MatchScore);
        System.Console.WriteLine(Player2Name + "'s match score is: " + Player2MatchScore);
        System.Console.WriteLine();
    }

    private string getCharToPrint(ref readonly Checker? i_Checker)
    {
        string charToPrint;

        if (i_Checker == null)
        {
            charToPrint = " ";
        }

        else
        {
            if (i_Checker?.Color == eColors.White )
            {
                if (i_Checker?.IsKing()?? false)
                {
                    charToPrint = "U";
                }

                else
                {
                    charToPrint = "O";

                }
            }

            else
            {
                if (i_Checker?.IsKing() ?? false)
                {
                    charToPrint = "K";
                }

                else
                {
                    charToPrint = "X";

                }
            }
        }

        return charToPrint;
    }

    private bool askForRematch()
    {
        bool rematch;

        System.Console.WriteLine();
        System.Console.WriteLine("Rematch? (Y/N)");
        string userInput = System.Console.ReadLine() ?? "No input";
        userInput = userInput.ToLower();

        while (userInput != "y" && userInput != "n")
        {
            System.Console.WriteLine("Invalid input! Type Y/N only.");
            userInput = System.Console.ReadLine()??"No input".ToLower();
        }

        if (userInput == "y")
        {
            rematch = true;
        }

        else
        {
            rematch = false;
        }

        return rematch;
    }

    private string getValidUserTurnInput()
    {
        eColors colorOfCurrentTurnHolder = m_Game.GetCurrentPlayerColor();
        string colorSymbol = colorOfCurrentTurnHolder == eColors.White ? "O" : "X";
        string pattern = @"^[A-Z][a-z]>[A-Z][a-z]$";
        string currentPlayer = m_Game.GetCurrentTurnHolderName() ?? "CPU";
        System.Console.WriteLine(currentPlayer + $"'s move ({colorSymbol}): ");
        string turnInput = System.Console.ReadLine() ?? "No input";

        if (!userQuitted(ref turnInput))
        {
            bool isTurnInputValid = Regex.IsMatch(turnInput, pattern);

            while (!isTurnInputValid)
            {
                System.Console.WriteLine("Invalid move, Please re-enter:");
                turnInput = System.Console.ReadLine() ?? "No input";
                isTurnInputValid = Regex.IsMatch(turnInput, pattern);
            }
        }

        return turnInput;
    }

    private static string getUserName()
    {
        bool isOnlyCharacters = false;

        System.Console.WriteLine("Please enter user name:");
        string userInput = System.Console.ReadLine() ?? "No input";
        isOnlyCharacters = userInput.All(char.IsLetter);

        while (!isOnlyCharacters && userInput != string.Empty)
        {
            System.Console.WriteLine("Invalid user name, please re-enter:");
            userInput = System.Console.ReadLine() ?? "No input";
            isOnlyCharacters = userInput.All(char.IsLetter);
        }

        return userInput;
    }

    private static int getBoardSize()
    {
        bool isValidBoardSize = false;
        int boardSize = 6;

        System.Console.WriteLine();
        System.Console.WriteLine("Please enter board size (6/8/10):");
        string userInput = System.Console.ReadLine() ?? "No input";

        if (userInput != string.Empty && userInput != null && userInput.All(char.IsDigit))
        {
            boardSize = int.Parse(userInput);
            isValidBoardSize = boardSize == 6 || boardSize == 8 || boardSize == 10;
        }

        else
        {
            isValidBoardSize = false;
        }

        while (!isValidBoardSize)
        {
            System.Console.WriteLine("Invalid board size, please re-enter (6/8/10):");
            userInput = System.Console.ReadLine() ?? "No input";

            if (userInput != string.Empty && userInput != null && userInput.All(char.IsDigit))
            {
                boardSize = int.Parse(userInput);
                isValidBoardSize = boardSize == 6 || boardSize == 8 || boardSize == 10;
            }

            else
            {
                isValidBoardSize = false;
            }
        }

        return boardSize;
    }

    private static GameType getGameType()
    {
        GameType gameType = GameType.PlayerVsPlayer;

        System.Console.WriteLine();
        System.Console.WriteLine("Please choose game type:" + "\n" +
            "(1) Player Vs Player" + "\n" + "(2) Player Vs CPU");
        string userInput = System.Console.ReadLine() ?? "No input";
        while (!(userInput == "1" || userInput == "2"))
        {
            System.Console.WriteLine("Invalid input, please re-enter:");
            userInput = System.Console.ReadLine() ?? "No input";
        }

        if (userInput == "1")
        {
            gameType = GameType.PlayerVsPlayer;
        }
        else if (userInput == "2")
        {
            gameType = GameType.PlayerVsComp;
        }

        System.Console.WriteLine();
        return gameType;
    }

    private bool userQuitted(ref string i_UserInput)
    {
        bool res = false;
        res = i_UserInput.ToLower() == "q";
        return res;
    }

    private char nextCharacter(char i_Char)
    {
        char nextChar;
        if (char.IsUpper(i_Char))
        {
            nextChar = (char)((i_Char - 'A' + 1) % 26 + 'A');

        }
        else
        {
            nextChar = (char)((i_Char - 'a' + 1) % 26 + 'a');

        }
        return nextChar;
    }

    public void ThankYouMsg()
    {
        System.Console.WriteLine();
        System.Console.WriteLine("Thanks for playing!");
    }

    private int charToIndex(char i_Char)
    {
        i_Char = char.ToLower(i_Char);

        return (int)(i_Char - 'a');
    }

    private char indexToChar(int i_Index)
    {
        return (char)('a' + i_Index);
    }

    private Move parseMoveFromUserInput(string i_MoveAsString)
    {
        bool? isJump = null;

        Coord source = new Coord(charToIndex(i_MoveAsString[0]), charToIndex(i_MoveAsString[1]));
        Coord dest = new Coord(charToIndex(i_MoveAsString[3]), charToIndex(i_MoveAsString[4]));

        return new Move(source, dest, isJump);
    }

    private string unParseMoveToPrint(ref readonly Move? i_Move)
    {
        string parsedMove = string.Empty;

        parsedMove += char.ToUpper(indexToChar(i_Move?.Source.X ?? 0));
        parsedMove += indexToChar(i_Move?.Source.Y ?? 0);
        parsedMove += '>';
        parsedMove += char.ToUpper(indexToChar(i_Move?.Destination.X ?? 0));
        parsedMove += indexToChar(i_Move?.Destination.Y ?? 0);

        return parsedMove;
    }


    private void displayContinueForCPUMoveMessage()
    {
        System.Console.WriteLine("Press ENTER to execute CPU's move");

        string userInput = System.Console.ReadLine() ?? "No input";

        while (userInput != string.Empty)
        {
            System.Console.WriteLine("Invalid input!");
            userInput = System.Console.ReadLine() ?? "No input";
        }
    }
}
