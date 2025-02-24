namespace WindowsUI
{
    internal class GameForm : Form
    {
        Game m_Game;
        BoardSquareButton[,] m_SquaresButtonsMatrix;
        Label m_Player1StatsLabel;
        Label m_Player2StatsLabel;
        BoardSquareButton m_FirstSelectedSquare;
        BoardSquareButton m_SecondSelectedSquare;

        public GameForm(Game i_game)
        {
            m_Game = i_game;
            initializeGameForm();
            m_FirstSelectedSquare = null;
            m_SecondSelectedSquare = null;
        }

        private void initializeGameForm()
        {
            this.Text = "Damka";
            this.Size = new Size(500, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            createSquaresMatrix();
            initializeSquaresMatrixButtons();
            initializePlayersLabels();
            updateSquaresArray();
            updatePlayersLabels();
        }

        private void initializePlayersLabels()
        {
            m_Player1StatsLabel = new Label();
            m_Player2StatsLabel = new Label();
            var namesAndScores = m_Game.GetCurrentMatchScoresAndUpdateOverall();
            m_Player1StatsLabel.Text = namesAndScores.Item1.Item1 + ": " + namesAndScores.Item1.Item2;
            m_Player2StatsLabel.Text = namesAndScores.Item2.Item1 ?? "CPU" + ": " + namesAndScores.Item2.Item2;
            m_Player1StatsLabel.Location = new Point(80, 10);
            m_Player2StatsLabel.Location = new Point(320, 10);
            m_Player2StatsLabel.Size = new Size(120, 20);
            m_Player1StatsLabel.Size = new Size(120, 20);
            this.Controls.Add(m_Player1StatsLabel);
            this.Controls.Add(m_Player2StatsLabel);
            m_Player2StatsLabel.Click += playerLabel_Click;
        }

        private void initializeSquaresMatrixButtons()
        {
            for (int i = 0; i < m_Game.GetBoardSize(); i++)
            {
                for (int j = 0; j < m_Game.GetBoardSize(); j++)
                {
                    m_SquaresButtonsMatrix[i, j] = new BoardSquareButton();
                    m_SquaresButtonsMatrix[i, j].AutoSize = false;
                    m_SquaresButtonsMatrix[i, j].Size = new Size(400 / m_Game.GetBoardSize(), 400 / m_Game.GetBoardSize());
                    m_SquaresButtonsMatrix[i, j].Location = new Point(42 + j * 400 / m_Game.GetBoardSize(), 42 + i * 400 / m_Game.GetBoardSize());
                    this.Controls.Add(m_SquaresButtonsMatrix[i, j]);
                    m_SquaresButtonsMatrix[i, j].Click += square_Click;
                    m_SquaresButtonsMatrix[i, j].Coord = new Coord(i, j);
                    m_SquaresButtonsMatrix[i, j].Font = new Font(m_SquaresButtonsMatrix[i, j].Font.FontFamily, 20, FontStyle.Bold);

                    if ((i % 2 == 0) && (j % 2 == 0) || (i % 2 != 0) && (j % 2 != 0))
                    {
                        m_SquaresButtonsMatrix[i, j].Enabled = false;
                        m_SquaresButtonsMatrix[i, j].BackColor = Color.Black;

                    }
                }
            }
        }

        private void createSquaresMatrix()
        {
            int boardSize = m_Game.GetBoardSize();
            m_SquaresButtonsMatrix = new BoardSquareButton[boardSize, boardSize];
        }

        private void updatePlayersLabels()
        {
            eColors playingColor = m_Game.GetCurrentPlayerColor();

            if (playingColor == eColors.Black)
            {
                m_Player2StatsLabel.BackColor = Color.LightBlue;
                m_Player1StatsLabel.BackColor = Color.Transparent;
            }

            else
            {
                m_Player1StatsLabel.BackColor = Color.LightBlue;
                m_Player2StatsLabel.BackColor = Color.Transparent;
            }

            var namesAndScores = m_Game.GetCurrentMatchScoresAndUpdateOverall();

            m_Player1StatsLabel.Text = namesAndScores.Item1.Item1 + ": " + namesAndScores.Item1.Item2.ToString();
            string player2Name = namesAndScores.Item2.Item1 ?? "CPU";
            m_Player2StatsLabel.Text = player2Name + ": " + namesAndScores.Item2.Item2.ToString();

            if (m_Game.IsCPUCurrentlyPlaying())
            {
                m_Player2StatsLabel.Text += "     (Click me!)";
            }
        }

        private void updateSquaresArray()
        {
            Checker?[,] logicalBoard = m_Game.GetBoard();

            eColors playingColor = m_Game.GetCurrentPlayerColor();
            
            for (int i = 0; i < m_Game.GetBoardSize(); i++)
            {
                for (int j = 0; j < m_Game.GetBoardSize(); j++)
                {
                    if (!m_Game.IsCPUCurrentlyPlaying())
                    {
                        if (logicalBoard[i, j]?.Color == playingColor)
                        {
                            m_SquaresButtonsMatrix[i, j].Enabled = true;
                        }

                        else if (logicalBoard[i, j] == null && m_SquaresButtonsMatrix[i, j].BackColor != Color.Black)
                        {
                            m_SquaresButtonsMatrix[i, j].Enabled = true;
                        }

                        else
                        {
                            m_SquaresButtonsMatrix[i, j].Enabled = false;
                        }
                    }

                    else
                    {
                        m_SquaresButtonsMatrix[i, j].Enabled = false;
                    }

                    if (logicalBoard[i, j]?.Color == eColors.White)
                    {
                        if (logicalBoard[i, j]?.IsKing() ?? false)
                        {
                            m_SquaresButtonsMatrix[i, j].Text = "U";
                        }

                        else
                        {
                            m_SquaresButtonsMatrix[i, j].Text = "O";
                        }
                    }

                    else if (logicalBoard[i, j]?.Color == eColors.Black)
                    {
                        if (logicalBoard[i, j]?.IsKing() ?? false)
                        {
                            m_SquaresButtonsMatrix[i, j].Text = "K";
                        }

                        else
                        {
                            m_SquaresButtonsMatrix[i, j].Text = "X";
                        }
                    }

                    else
                    {
                        m_SquaresButtonsMatrix[i, j].Text = string.Empty;
                    }
                }
            }
        }

        private void square_Click(object sender, EventArgs e)
        {
            BoardSquareButton selectedButton = sender as BoardSquareButton;

            if (!selectedButton.Activated)
            {
                m_FirstSelectedSquare = null;
            }

            else
            {
                if (m_FirstSelectedSquare == null)
                {
                    m_FirstSelectedSquare = selectedButton;
                }

                else
                {
                    m_SecondSelectedSquare = selectedButton;

                    if (tryMakeMove())
                    {
                        UpdateGameForm();
                    }

                    else
                    {
                        MessageBox.Show("Invalid move! Try again.", "Damka", MessageBoxButtons.OK);
                    }

                    deactivateSelectedMoveSquares();
                }
            }
        }

        private void deactivateSelectedMoveSquares()
        {
            m_FirstSelectedSquare.ToggleActivation();
            m_SecondSelectedSquare.ToggleActivation();
            m_FirstSelectedSquare = null;
            m_SecondSelectedSquare = null;

        }

        private bool tryMakeMove()
        {
            bool validMove = false;

            if (m_FirstSelectedSquare != null && m_SecondSelectedSquare != null)
            {
                Coord source = m_FirstSelectedSquare.Coord;
                Coord dest = m_SecondSelectedSquare.Coord;

                Move move = new Move(source, dest, null);

                if (m_Game.IsMoveAllowed(move))
                {
                    m_Game.ExecuteMove();
                    validMove = true;
                }
            }

            return validMove;
        }

        private void playerLabel_Click(object sender, EventArgs e)
        {
            if (m_Game.IsCPUCurrentlyPlaying())
            {
                m_Game.ExecutePlayForCPU();
            }

            UpdateGameForm();
        }

        public void Start()
        {
            this.ShowDialog();
        }

        public void UpdateGameForm()
        {
            updateSquaresArray();

            updatePlayersLabels();

            m_Game.CheckIfMatchEnded();

            if (m_Game.GetMatchStatus() != eMatchStatus.OnGoing)
            {
                concludeMatch();
            }
        }

        private void concludeMatch()
        {
            DialogResult result = DialogResult.OK;

            if (m_Game.GetMatchStatus() == eMatchStatus.Tie)
            {
                result = MessageBox.Show("Tie!\nAnother Round?", "Damka", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }

            else if (m_Game.GetMatchStatus() == eMatchStatus.Winner)
            {
                string winner = m_Game.GetWinnerName() ?? "CPU";
                result = MessageBox.Show($"{winner} Won! \nAnother Round?", "Damka", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }

            if (result == DialogResult.No)
            {
                Environment.Exit(0);
            }

            else
            {
                m_Game.RestartMatch();
                UpdateGameForm();
            }
        }
    }
}
