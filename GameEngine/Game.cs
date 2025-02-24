using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class Game
{
    private Match m_Match;
    private readonly Player r_MainPlayer;
    private readonly Player r_SecondPlayer;
    private readonly GameConfig r_GameConfig;

    public Game(string i_MainPlayer, string? i_SecondPlayer, GameConfig i_Config)
    {
        r_MainPlayer = new Player(i_MainPlayer);
        r_SecondPlayer = new Player(i_SecondPlayer);
        r_GameConfig = i_Config;
        m_Match = new Match(i_Config.BoardSize, r_MainPlayer, r_SecondPlayer);
    }

    public int GetBoardSize()
    {
        return r_GameConfig.BoardSize;
    }

    public string? GetWinnerName()
    {
        return m_Match.GetWinnerName();
    }

    public eColors GetCurrentPlayerColor()
    {
        return m_Match.GetCurrentPlayerColor();
    }

    public eColors GetPreviousPlayerColor()
    {
        return m_Match.GetPreviousPlayerColor();
    }

    public string? GetCurrentTurnHolderName()
    {
        return m_Match.GetCurrentTurnHolderName();
    }

    public string? GetPreviousTurnPlayerName()
    {
        return m_Match.GetPreviousTurnPlayerName();
    }

    public void CheckIfMatchEnded()
    {
        m_Match.CheckIfMatchEndedAndUpdateState();
    }

    private void updatePlayersScores(ref readonly int i_Player1Score, ref readonly int i_Player2Score)
    {
        r_MainPlayer.AddToPlayerScore(i_Player1Score);
        r_SecondPlayer.AddToPlayerScore(i_Player2Score);
    }

    public ((string, int), (string?, int)) GetCurrentMatchScoresAndUpdateOverall()
    {
        int player1Score;
        int player2Score;
        m_Match.GetMatchScores(out player1Score, out player2Score);
        updatePlayersScores(ref player1Score, ref player2Score);
        (string, int) Player1Stats = (r_MainPlayer.Name, player1Score);
        (string?, int) Player2Stats = (r_SecondPlayer.Name, player2Score);
        ((string, int), (string ?, int)) result = (Player1Stats, Player2Stats);
        return result;
    }

    public ((string, int), (string?, int)) GetOverallPlayersScores()
    {
        (string, int) firstNameScorePair = (r_MainPlayer.Name, r_MainPlayer.Score);

        (string?, int) secondNameScorePair = (r_SecondPlayer.Name, r_SecondPlayer.Score);

        ((string, int), (string?, int)) result = (firstNameScorePair, secondNameScorePair);

        return result;
    }

    public bool IsCPUCurrentlyPlaying()
    {
        return m_Match.IsCurrentTurnOfCPU();
    }

    public void RestartMatch()
    {
        m_Match = new Match(r_GameConfig.BoardSize, r_MainPlayer, r_SecondPlayer);
    }

    public eMatchStatus GetMatchStatus()
    {
        return m_Match.MatchStatus;
    }

    public Checker?[,] GetBoard()
    {
        return m_Match.GetCheckersArray();
    }

    public void SetMatchStatusToQuitted()
    {
        m_Match.MatchStatus = eMatchStatus.Quitted;
    }

    public Move? GetLastMadeMove()
    {
        return m_Match.LastPlayedMove;
    }

    public void ExecutePlayForCPU()
    {
        m_Match.ExecutePlayForCPU();
    }

    public bool IsMoveAllowed(Move i_Move)
    {
        return m_Match.CheckMove(i_Move);
    }

    public void ExecuteMove()
    {
        m_Match.ExecuteMoveAndUpdateTurnHolder();
    }

    class Match
    {
        private eMatchStatus m_MatchStatus;
        private Board m_Board;
        private Player m_Player1;
        private Player m_Player2;
        private ePlayerIdentifier m_CurrentTurnHolder;
        private ePlayerIdentifier m_PreviousTurnPlayer;
        private ePlayerIdentifier? m_Winner;
        private Move? m_CurrentMove;
        private bool m_AnotherJumpAvailable; 

        internal Match(int i_BoardSize, Player io_Player1, Player io_Player2)
        {
            m_MatchStatus = eMatchStatus.OnGoing;
            m_Board = new Board(i_BoardSize);
            m_Player1 = io_Player1;
            m_Player2 = io_Player2;
            m_CurrentTurnHolder = ePlayerIdentifier.Player1;
            m_PreviousTurnPlayer = m_CurrentTurnHolder;
            m_AnotherJumpAvailable = false;
        }

        internal eColors GetCurrentPlayerColor()
        {
            eColors result;

            if (m_CurrentTurnHolder == ePlayerIdentifier.Player1)
            {
                result = eColors.White;
            }

            else
            {
                result = eColors.Black;
            }

            return result;
        }

        internal eColors GetPreviousPlayerColor()
        {
            eColors result;

            if (m_PreviousTurnPlayer == ePlayerIdentifier.Player1)
            {
                result = eColors.White;
            }

            else
            {
                result = eColors.Black;
            }

            return result;
        }

        internal string? GetCurrentTurnHolderName()
        {
            string? result;

            if (m_CurrentTurnHolder == ePlayerIdentifier.Player1)
            {
                result = m_Player1.Name;
            }

            else
            {
                result = m_Player2.Name;
            }

            return result;
        }

        internal string? GetPreviousTurnPlayerName()
        {
            string? result;

            if (m_PreviousTurnPlayer == ePlayerIdentifier.Player1)
            {
                result = m_Player1.Name;
            }

            else
            {
                result = m_Player2.Name;
            }

            return result;
        }

        internal void GetMatchScores(out int o_ScoreOfPlayer1, out int o_ScoreOfPlayer2)
        {
            m_Board.GetScores(out o_ScoreOfPlayer1, out o_ScoreOfPlayer2);
        }

        internal void ExecutePlayForCPU()
        {
            List<Move> movesList = createLegalMovesList(getCurrentColorToPlay());
            Random random = new Random();
            int randomIndex = random.Next(movesList.Count);
            m_CurrentMove = movesList[randomIndex];
            ExecuteMoveAndUpdateTurnHolder();
        }

        internal void ExecuteMoveAndUpdateTurnHolder()
        {
            m_Board.ExecuteMove(m_CurrentMove);
            updateTurnHolder();
        }

        private void updateTurnHolder()
        {
            bool toFlipTurnHolder = true;

            m_PreviousTurnPlayer = m_CurrentTurnHolder;

            if (m_CurrentMove?.IsJump ?? false)
            {
                List<Move> possibleMoves = new List<Move>();

                bool anotherJumpAvailable = false;

                Coord newLocationOfChecker = m_CurrentMove?.Destination ?? new Coord(0, 0);

                addCurrentCheckerMovesToList(newLocationOfChecker, possibleMoves, ref anotherJumpAvailable);

                if (anotherJumpAvailable)
                {
                    toFlipTurnHolder = false;
                    m_AnotherJumpAvailable = true;
                }
            }

            if (toFlipTurnHolder)
            {
                m_AnotherJumpAvailable = false;

                if (m_CurrentTurnHolder == ePlayerIdentifier.Player1)
                {
                    m_CurrentTurnHolder = ePlayerIdentifier.Player2;
                }

                else
                {
                    m_CurrentTurnHolder = ePlayerIdentifier.Player1;
                }
            }
        }

        private bool playerHasMovesByColor(eColors i_Color)
        {
            bool result = true;

            List<Move> opponentLegalMovesList = createLegalMovesList(i_Color);

            if (opponentLegalMovesList.Count == 0)
            {
                result = false;
            }

            return result;
        }

        private List<Move> createLegalMovesList(eColors i_Color)
        {
            List<Move> legalMovesList = new List<Move>();

            bool jumpIsAvailable = false;

            eColors currentCheckerToPlay = i_Color;

            if (!m_AnotherJumpAvailable)
            {
                for (int i = 0; i < m_Board.BoardSize; i++)
                {
                    for (int j = 0; j < m_Board.BoardSize; j++)
                    {
                        bool inBounds;

                        if (m_Board.GetCheckerByCoord(new Coord(i, j), out inBounds)?.Color == currentCheckerToPlay)
                        {
                            addCurrentCheckerMovesToList(new Coord(i, j), legalMovesList, ref jumpIsAvailable);
                        }
                    }
                }
            }

            else
            {
                Coord lastMoveDestination = m_CurrentMove?.Destination ?? new Coord(0, 0);
                addCurrentCheckerMovesToList(lastMoveDestination, legalMovesList, ref jumpIsAvailable);
            }

            if (jumpIsAvailable)
            {
                filterNonJumpMovesFromList(legalMovesList);
            }

            return legalMovesList;
        }

        private void filterNonJumpMovesFromList(List<Move> legalMovesList)
        {
            for (int i = legalMovesList.Count - 1; i >= 0; i--)
            {
                if (!legalMovesList[i].IsJump)
                {
                    legalMovesList.RemoveAt(i);
                }
            }
        }

        private void addCurrentCheckerMovesToList(Coord i_Coord, List<Move> io_LegalMovesList, ref bool io_JumpIsAvailable)
        {
            bool inBounds;

            if (m_Board.GetCheckerByCoord(i_Coord, out inBounds)?.IsKing() ?? false)
            {
                tryBackLeftMoveForKing(i_Coord, io_LegalMovesList, ref io_JumpIsAvailable);
                tryBackRightMoveForKing(i_Coord, io_LegalMovesList, ref io_JumpIsAvailable);
            }

            tryLeftMoveForChecker(i_Coord, io_LegalMovesList, ref io_JumpIsAvailable);
            tryRightMoveForChecker(i_Coord, io_LegalMovesList, ref io_JumpIsAvailable);
        }

        private void tryBackLeftMoveForKing(Coord i_CurrentCheckerCoord, List<Move> io_LegalMovesList, ref bool io_JumpIsAvailable)
        {
            bool inBounds;

            Checker? currentChecker = m_Board.GetCheckerByCoord(i_CurrentCheckerCoord, out inBounds);

            if (currentChecker?.Color == eColors.Black)
            {
                Coord coordOfNeighbor = new Coord(i_CurrentCheckerCoord.X + 1, i_CurrentCheckerCoord.Y - 1);

                Checker? neighbor = m_Board.GetCheckerByCoord(coordOfNeighbor, out inBounds);

                if (inBounds)
                {
                    if (neighbor == null)
                    {
                        const bool v_IsJumpMove = true;
                        io_LegalMovesList.Add(new Move(i_CurrentCheckerCoord, coordOfNeighbor, !v_IsJumpMove));
                    }

                    else if (neighbor?.Color == eColors.White)
                    {
                        tryJumpBackLeftForKing(i_CurrentCheckerCoord, io_LegalMovesList, ref io_JumpIsAvailable);
                    }
                }
            }

            if (currentChecker?.Color == eColors.White)
            {
                Coord coordOfNeighbor = new Coord(i_CurrentCheckerCoord.X - 1, i_CurrentCheckerCoord.Y + 1);

                Checker? neighbor = m_Board.GetCheckerByCoord(coordOfNeighbor, out inBounds);

                if (inBounds)
                {
                    if (neighbor == null)
                    {
                        const bool v_IsJumpMove = true;
                        io_LegalMovesList.Add(new Move(i_CurrentCheckerCoord, coordOfNeighbor, !v_IsJumpMove));
                    }

                    else if (neighbor?.Color == eColors.Black)
                    {
                        tryJumpBackLeftForKing(i_CurrentCheckerCoord, io_LegalMovesList, ref io_JumpIsAvailable);
                    }
                }
            }
        }

        private void tryBackRightMoveForKing(Coord i_CurrentCheckerCoord, List<Move> io_LegalMovesList, ref bool io_JumpIsAvailable)
        {
            bool inBounds;

            Checker? currentChecker = m_Board.GetCheckerByCoord(i_CurrentCheckerCoord, out inBounds);

            if (currentChecker?.Color == eColors.Black)
            {
                Coord coordOfNeighbor = new Coord(i_CurrentCheckerCoord.X + 1, i_CurrentCheckerCoord.Y + 1);

                Checker? neighbor = m_Board.GetCheckerByCoord(coordOfNeighbor, out inBounds);

                if (inBounds)
                {
                    if (neighbor == null)
                    {
                        const bool v_IsJumpMove = true;
                        io_LegalMovesList.Add(new Move(i_CurrentCheckerCoord, coordOfNeighbor, !v_IsJumpMove));
                    }

                    else if (neighbor?.Color == eColors.White)
                    {
                        tryJumpBackRightForKing(i_CurrentCheckerCoord, io_LegalMovesList, ref io_JumpIsAvailable);
                    }
                }
            }

            if (currentChecker?.Color == eColors.White)
            {
                Coord coordOfNeighbor = new Coord(i_CurrentCheckerCoord.X - 1, i_CurrentCheckerCoord.Y - 1);

                Checker? neighbor = m_Board.GetCheckerByCoord(coordOfNeighbor, out inBounds);

                if (inBounds)
                {
                    if (neighbor == null)
                    {
                        const bool v_IsJumpMove = true;
                        io_LegalMovesList.Add(new Move(i_CurrentCheckerCoord, coordOfNeighbor, !v_IsJumpMove));
                    }

                    else if (neighbor?.Color == eColors.Black)
                    {
                        tryJumpBackRightForKing(i_CurrentCheckerCoord, io_LegalMovesList, ref io_JumpIsAvailable);
                    }
                }
            }
        }

        private void tryLeftMoveForChecker(Coord i_CurrentCheckerCoord, List<Move> io_LegalMovesList, ref bool io_JumpIsAvailable)
        {
            bool inBounds;

            Checker? currentChecker = m_Board.GetCheckerByCoord(i_CurrentCheckerCoord, out inBounds);

            if (currentChecker?.Color == eColors.Black)
            {
                Coord coordOfNeighbor = new Coord(i_CurrentCheckerCoord.X - 1, i_CurrentCheckerCoord.Y - 1);

                Checker? neighbor = m_Board.GetCheckerByCoord(coordOfNeighbor, out inBounds);

                if (inBounds)
                {
                    if (neighbor == null)
                    {
                        const bool v_IsJumpMove = true;
                        io_LegalMovesList.Add(new Move(i_CurrentCheckerCoord, coordOfNeighbor, !v_IsJumpMove));
                    }

                    else if (neighbor?.Color == eColors.White)
                    {
                        tryJumpLeftForChecker(i_CurrentCheckerCoord, io_LegalMovesList, ref io_JumpIsAvailable);
                    }
                }
            }

            if (currentChecker?.Color == eColors.White)
            {
                Coord coordOfNeighbor = new Coord(i_CurrentCheckerCoord.X + 1, i_CurrentCheckerCoord.Y + 1);

                Checker? neighbor = m_Board.GetCheckerByCoord(coordOfNeighbor, out inBounds);

                if (inBounds)
                {
                    if (neighbor == null)
                    {
                        const bool v_IsJumpMove = true;
                        io_LegalMovesList.Add(new Move(i_CurrentCheckerCoord, coordOfNeighbor, !v_IsJumpMove));
                    }

                    else if (neighbor?.Color == eColors.Black)
                    {
                        tryJumpLeftForChecker(i_CurrentCheckerCoord, io_LegalMovesList, ref io_JumpIsAvailable);
                    }
                }
            }
        }

        private void tryRightMoveForChecker(Coord i_CurrentCheckerCoord, List<Move> io_LegalMovesList, ref bool io_JumpIsAvailable)
        {
            bool inBounds;

            Checker? currentChecker = m_Board.GetCheckerByCoord(i_CurrentCheckerCoord, out inBounds);

            if (currentChecker?.Color == eColors.Black)
            {
                Coord coordOfNeighbor = new Coord(i_CurrentCheckerCoord.X - 1, i_CurrentCheckerCoord.Y + 1);

                Checker? neighbor = m_Board.GetCheckerByCoord(coordOfNeighbor, out inBounds);

                if (inBounds)
                {
                    if (neighbor == null)
                    {
                        const bool v_IsJumpMove = true;
                        io_LegalMovesList.Add(new Move(i_CurrentCheckerCoord, coordOfNeighbor, !v_IsJumpMove));
                    }

                    else if (neighbor?.Color == eColors.White)
                    {
                        tryJumpRightForChecker(i_CurrentCheckerCoord, io_LegalMovesList, ref io_JumpIsAvailable);
                    }
                }
            }

            if (currentChecker?.Color == eColors.White)
            {
                Coord coordOfNeighbor = new Coord(i_CurrentCheckerCoord.X + 1, i_CurrentCheckerCoord.Y - 1);

                Checker? neighbor = m_Board.GetCheckerByCoord(coordOfNeighbor, out inBounds);

                if (inBounds)
                {
                    if (neighbor == null)
                    {
                        const bool v_IsJumpMove = true;
                        io_LegalMovesList.Add(new Move(i_CurrentCheckerCoord, coordOfNeighbor, !v_IsJumpMove));
                    }

                    else if (neighbor?.Color == eColors.Black)
                    {
                        tryJumpRightForChecker(i_CurrentCheckerCoord, io_LegalMovesList, ref io_JumpIsAvailable);
                    }
                }
            }
        }

        private void tryJumpLeftForChecker(Coord i_CurrentCheckerCoord, List<Move> io_LegalMovesList, ref bool io_JumpIsAvailable)
        {
            bool inBounds;

            Coord coordToCheck;

            Checker? currentChecker = m_Board.GetCheckerByCoord(i_CurrentCheckerCoord, out inBounds);

            if (currentChecker?.Color == eColors.Black)
            {
                coordToCheck = new Coord(i_CurrentCheckerCoord.X - 2, i_CurrentCheckerCoord.Y - 2);
            }

            else
            {
                coordToCheck = new Coord(i_CurrentCheckerCoord.X + 2, i_CurrentCheckerCoord.Y + 2);
            }

            Checker? checkerInCoordToCheck = m_Board.GetCheckerByCoord(coordToCheck, out inBounds);

            if (inBounds)
            {
                if (checkerInCoordToCheck == null)
                {
                    const bool v_IsJumpMove = true;
                    io_LegalMovesList.Add(new Move(i_CurrentCheckerCoord, coordToCheck, v_IsJumpMove));
                    io_JumpIsAvailable = true;
                }
            }
        }

        private void tryJumpRightForChecker(Coord i_CurrentCheckerCoord, List<Move> io_LegalMovesList, ref bool io_JumpIsAvailable)
        {
            bool inBounds;

            Coord coordToCheck;

            Checker? currentChecker = m_Board.GetCheckerByCoord(i_CurrentCheckerCoord, out inBounds);

            if (currentChecker?.Color == eColors.Black)
            {
                coordToCheck = new Coord(i_CurrentCheckerCoord.X - 2, i_CurrentCheckerCoord.Y + 2);
            }

            else
            {
                coordToCheck = new Coord(i_CurrentCheckerCoord.X + 2, i_CurrentCheckerCoord.Y - 2);
            }

            Checker? checkerInCoordToCheck = m_Board.GetCheckerByCoord(coordToCheck, out inBounds);

            if (inBounds)
            {
                if (checkerInCoordToCheck == null)
                {
                    const bool v_IsJumpMove = true;
                    io_LegalMovesList.Add(new Move(i_CurrentCheckerCoord, coordToCheck, v_IsJumpMove));
                    io_JumpIsAvailable = true;
                }
            }
        }

        private void tryJumpBackLeftForKing(Coord i_CurrentCheckerCoord, List<Move> io_LegalMovesList, ref bool io_JumpIsAvailable)
        {
            bool inBounds;

            Coord coordToCheck;

            Checker? currentChecker = m_Board.GetCheckerByCoord(i_CurrentCheckerCoord, out inBounds);

            if (currentChecker?.Color == eColors.Black)
            {
                coordToCheck = new Coord(i_CurrentCheckerCoord.X + 2, i_CurrentCheckerCoord.Y - 2);
            }

            else
            {
                coordToCheck = new Coord(i_CurrentCheckerCoord.X - 2, i_CurrentCheckerCoord.Y + 2);
            }

            Checker? checkerInCoordToCheck = m_Board.GetCheckerByCoord(coordToCheck, out inBounds);

            if (inBounds)
            {
                if (checkerInCoordToCheck == null)
                {
                    const bool v_IsJumpMove = true;
                    io_LegalMovesList.Add(new Move(i_CurrentCheckerCoord, coordToCheck, v_IsJumpMove));
                    io_JumpIsAvailable = true;
                }
            }
        }

        private void tryJumpBackRightForKing(Coord i_CurrentCheckerCoord, List<Move> io_LegalMovesList, ref bool io_JumpIsAvailable)
        {
            bool inBounds;

            Coord coordToCheck;

            Checker? currentChecker = m_Board.GetCheckerByCoord(i_CurrentCheckerCoord, out inBounds);

            if (currentChecker?.Color == eColors.Black)
            {
                coordToCheck = new Coord(i_CurrentCheckerCoord.X + 2, i_CurrentCheckerCoord.Y + 2);
            }

            else
            {
                coordToCheck = new Coord(i_CurrentCheckerCoord.X - 2, i_CurrentCheckerCoord.Y - 2);
            }

            Checker? checkerInCoordToCheck = m_Board.GetCheckerByCoord(coordToCheck, out inBounds);

            if (inBounds)
            {
                if (checkerInCoordToCheck == null)
                {
                    const bool v_IsJumpMove = true;
                    io_LegalMovesList.Add(new Move(i_CurrentCheckerCoord, coordToCheck, v_IsJumpMove));
                    io_JumpIsAvailable = true;
                }
            }
        }

        private eColors getCurrentColorToPlay()
        {
            eColors currentCheckerToPlay;

            if (m_CurrentTurnHolder == ePlayerIdentifier.Player1)
            {
                currentCheckerToPlay = eColors.White;
            }

            else
            {
                currentCheckerToPlay = eColors.Black;
            }

            return currentCheckerToPlay;
        }

        internal void CheckIfMatchEndedAndUpdateState()
        {
            if (m_MatchStatus == eMatchStatus.Quitted)
            {
                if (m_CurrentTurnHolder == ePlayerIdentifier.Player1)
                {
                    m_Winner = ePlayerIdentifier.Player2;
                }

                else
                {
                    m_Winner = ePlayerIdentifier.Player1;
                }
            }

            else
            {
                checkForTieOrWinner();
            }

            if (m_MatchStatus == eMatchStatus.Tie)
            {
                setWinnerOnTie();
            }
        }

        private void setWinnerOnTie()
        {
            int player1Score, player2Score;

            GetMatchScores(out player1Score, out player2Score);

            if (player1Score > player2Score)
            {
                m_Winner = ePlayerIdentifier.Player1;
            }

            else
            {
                m_Winner = ePlayerIdentifier.Player2;
            }
        }

        private void checkForTieOrWinner()
        {
            if (!playerHasMovesByColor(eColors.White) && !playerHasMovesByColor(eColors.Black))
            {
                m_MatchStatus = eMatchStatus.Tie;
            }

            else if (m_Board.NumberOfBlackCheckers == 0 || !playerHasMovesByColor(eColors.Black))
            {
                m_MatchStatus = eMatchStatus.Winner;
                m_Winner = ePlayerIdentifier.Player1;
            }

            else if (m_Board.NumberOfWhiteCheckers == 0 || !playerHasMovesByColor(eColors.White))
            {
                m_MatchStatus = eMatchStatus.Winner;
                m_Winner = ePlayerIdentifier.Player2;
            }
        }

        internal string? GetWinnerName()
        {
            string? winnerName = null;

            if (m_Winner == ePlayerIdentifier.Player1)
            {
                winnerName = m_Player1.Name;
            }

            else if (m_Winner == ePlayerIdentifier.Player2)
            {
                winnerName = m_Player2.Name;
            }

            return winnerName;
        }

        internal bool IsCurrentTurnOfCPU()
        {
            return (m_CurrentTurnHolder == ePlayerIdentifier.Player2 && m_Player2.IsCPU);
        }

        internal Move? LastPlayedMove
        {
            get { return m_CurrentMove; }
        }
          
        internal eMatchStatus MatchStatus
        {
            get
            {
                return m_MatchStatus;
            }

            set
            {
                m_MatchStatus = value;
            }
        }

        internal Checker?[,] GetCheckersArray()
        {
            return m_Board.CheckersArray;
        }

        internal bool CheckMove(Move i_Move)
        {
            bool moveIsValid = false;

            List<Move> validMovesList = createLegalMovesList(getCurrentColorToPlay());

            foreach (Move move in validMovesList)
            {
                if (Move.CompareMoves(i_Move, move))
                {
                    moveIsValid = true;
                    m_CurrentMove = move;
                }
            }

            return moveIsValid;
        }

        class Board
        {
            private Checker?[,] m_CheckersArray;
            private readonly int r_BoardSize;
            private int m_NumberOfWhiteCheckers;
            private int m_NumberOfBlackCheckers;

            internal Board(int i_BoardSize)
            {
                m_CheckersArray = new Checker?[i_BoardSize, i_BoardSize];
                m_NumberOfBlackCheckers = 0;
                m_NumberOfWhiteCheckers = 0;
                r_BoardSize = i_BoardSize;
                initializeBoard(i_BoardSize);
            }

            internal void GetScores(out int o_WhiteScore, out int o_BlackScore)
            {
                o_WhiteScore = 0;
                o_BlackScore = 0;

                const int k_ValueOfKing = 4;
                const int k_ValueOfChecker = 1;

                for (int i = 0; i < r_BoardSize; i++)
                {
                    for (int j = 0; j < r_BoardSize;  j++)
                    {
                        bool inBounds;
                        Checker? currChecker = GetCheckerByCoord(new Coord(i, j), out inBounds);
                        if (currChecker?.Color == eColors.White)
                        {
                            if (currChecker?.IsKing() ?? false)
                            {
                                o_WhiteScore += k_ValueOfKing;
                            }

                            else
                            {
                                o_WhiteScore += k_ValueOfChecker;
                            }
                        }

                        else if (currChecker?.Color == eColors.Black)
                        {
                            if (currChecker?.IsKing() ?? false)
                            {
                                o_BlackScore += k_ValueOfKing;
                            }

                            else
                            {
                                o_BlackScore += k_ValueOfChecker;
                            }
                        }
                    }
                }

                int scoreDifferenceABSValue = Math.Abs(o_WhiteScore - o_BlackScore);

                if (o_WhiteScore > o_BlackScore)
                {
                    o_WhiteScore = scoreDifferenceABSValue;
                    o_BlackScore = 0;
                }

                else
                {
                    o_BlackScore = scoreDifferenceABSValue;
                    o_WhiteScore = 0;
                }
            }

            internal int NumberOfWhiteCheckers
            {
                get { return m_NumberOfWhiteCheckers; }
            }

            internal int NumberOfBlackCheckers
            {
                get { return m_NumberOfBlackCheckers; }
            }

            internal int BoardSize
            {
                get { return r_BoardSize; }
            }

            internal Checker?[,] CheckersArray
            {
                get
                {
                    return m_CheckersArray;
                }
            }

            internal Checker? GetCheckerByCoord(Coord? i_Coord, out bool o_InBounds)
            {
                Checker? result;

                if (checkIfCoordOutOfBounds(ref i_Coord))
                {
                    o_InBounds = false;
                    result = null;
                }

                else
                {
                    o_InBounds = true;
                    result = m_CheckersArray[i_Coord?.X ?? 0, i_Coord?.Y ?? 0];
                }

                return result;
            }

            private bool checkIfCoordOutOfBounds(ref readonly Coord? i_Coord)
            {
                bool outOfBounds = false;

                if (i_Coord == null)
                {
                    outOfBounds = true;
                }

                if (i_Coord?.X >= r_BoardSize || i_Coord?.X < 0)
                {
                    outOfBounds = true;
                }

                if (i_Coord?.Y >= r_BoardSize || i_Coord?.Y < 0)
                {
                    outOfBounds = true;
                }

                return outOfBounds;
            }

            private void initializeBoard(int i_BoardSize)
            {
                int numOfCheckerLinesForEachPlayer;
                int numOfBlankLines;

                numOfBlankLines = 2;

                numOfCheckerLinesForEachPlayer = (r_BoardSize - numOfBlankLines) / 2;

                m_CheckersArray = new Checker?[r_BoardSize, r_BoardSize];

                for (int i = 0; i < numOfCheckerLinesForEachPlayer; i++)
                {
               
                    for (int j = 0; j < r_BoardSize; j++)
                    {
                        if (i % 2 == 0 && j % 2 != 0)
                        {
                            addCheckerByCoord(new Checker(eColors.White), new Coord(i , j));
                            m_NumberOfWhiteCheckers++;
                        }

                        if (i % 2 != 0 && j % 2 == 0)
                        {
                            addCheckerByCoord(new Checker(eColors.White), new Coord(i, j));
                            m_NumberOfWhiteCheckers++;
                        }
                    }
                }

                for (int i = numOfCheckerLinesForEachPlayer + numOfBlankLines; i < r_BoardSize; i++)
                {
                    for (int j = 0; j < r_BoardSize; j++)
                    {
                        if (i % 2 == 0 && j % 2 != 0)
                        {
                            addCheckerByCoord(new Checker(eColors.Black), new Coord(i, j));
                            m_NumberOfBlackCheckers++;
                        }

                        if (i % 2 != 0 && j % 2 == 0)
                        {
                            addCheckerByCoord(new Checker(eColors.Black), new Coord(i, j));
                            m_NumberOfBlackCheckers++;
                        }
                    }
                }
            }

            internal void ExecuteMove(Move? i_MoveToExecute)
            {
                if (i_MoveToExecute?.IsJump ?? false)
                {
                    Coord coordOfCheckerToRemove = getCoordOfRemovedCheckerByJump(i_MoveToExecute);
         
                    if (m_CheckersArray[coordOfCheckerToRemove.X, coordOfCheckerToRemove.Y]?.Color == eColors.White)
                    {
                        m_NumberOfWhiteCheckers--;
                    }

                    else
                    {
                        m_NumberOfBlackCheckers--;
                    }

                    removeCheckerByCoord(coordOfCheckerToRemove);
                }

                replaceCheckerLocationAndSetAsKingIfRequired(i_MoveToExecute);
            }

            private void replaceCheckerLocationAndSetAsKingIfRequired(Move? i_MoveToExecute)
            {
                bool inBounds;
                Checker? checkerToReplace = GetCheckerByCoord(i_MoveToExecute?.Source, out inBounds);
                removeCheckerByCoord(i_MoveToExecute?.Source);

                if (didCheckerBecomeKingAfterMove(checkerToReplace, i_MoveToExecute))
                {
                    if (checkerToReplace != null)
                    {
                        Checker modifiedChecker = checkerToReplace.Value;
                        modifiedChecker.SetAsKing();
                        checkerToReplace = modifiedChecker;
                    }
                }

                addCheckerByCoord(checkerToReplace, i_MoveToExecute?.Destination);
            }

            private bool didCheckerBecomeKingAfterMove(Checker? i_Checker, Move? i_Move)
            {
                bool result = false;

                if (i_Checker?.Color == eColors.White)
                {
                    if (i_Move?.Destination.X == r_BoardSize - 1)
                    {
                        result = true;
                    }
                }

                else
                {
                    if (i_Move?.Destination.X == 0)
                    {
                        result = true;
                    }
                }

                return result;
            }

            private void removeCheckerByCoord(Coord? i_Coord)
            {
                if (i_Coord != null)
                {
                    m_CheckersArray[i_Coord?.X ?? 0, i_Coord?.Y ?? 0] = null;
                }
            }

            private void addCheckerByCoord(Checker? i_Checker, Coord? i_Coord)
            {
                m_CheckersArray[i_Coord?.X ?? 0, i_Coord?.Y ?? 0] = i_Checker;
            }

            private Coord getCoordOfRemovedCheckerByJump(Move? i_MoveToExecute)
            {
                int xOfCoord = i_MoveToExecute?.Source.X ?? 0;
                xOfCoord += i_MoveToExecute?.Destination.X ?? 0;
                xOfCoord = xOfCoord / 2;

                int yOfCoord = i_MoveToExecute?.Source.Y ?? 0;
                yOfCoord += i_MoveToExecute?.Destination.Y ?? 0;
                yOfCoord = yOfCoord / 2;

                Coord result = new Coord(xOfCoord, yOfCoord);

                return result;
            }
        }

        private enum ePlayerIdentifier
        {
            Player1,
            Player2,
        }
    }

    class Player
    {
        private readonly string? r_Name;
        private int m_Score;
        private readonly bool r_IsCPU;

        internal Player(string? name)
        {
            m_Score = 0;

            r_Name = name;

            if (name != null)
            {
                r_IsCPU = false;
            }

            else
            {
                r_IsCPU = true;
            }
        }

        internal string? Name
        {
            get
            {
                return r_Name;
            }
        }

        internal int Score
        {
            get
            {
                return m_Score;
            }
        }

        internal void AddToPlayerScore(int i_Value)
        {
            m_Score += i_Value;
        }

        internal bool IsCPU
        {
            get
            {
                return r_IsCPU;
            }
        }
    }
}