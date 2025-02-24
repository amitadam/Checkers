namespace WindowsUI
{
    internal class SettingsForm : Form
    {
        private Game? m_Game;

        private RadioButton m_6x6Button;
        private RadioButton m_8x8Button;
        private RadioButton m_10x10Button;
        private TextBox m_Player1NameTextBox;
        private CheckBox m_Player2CheckBox;
        private TextBox m_Player2NameTextBox;

        public SettingsForm()
        {
            initializeSettingsForm();
        }

        private void initializeSettingsForm()
        {
            this.Text = "Game Settings";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(400, 250);

            Label boardSizeLabel = new Label
            {
                Text = "Board Size:",
                Location = new Point(20, 20)
            };
            this.Controls.Add(boardSizeLabel);

            m_6x6Button = new RadioButton { Text = "6x6", Location = new Point(50, 40) };
            m_8x8Button = new RadioButton { Text = "8x8", Location = new Point(170, 40) };
            m_10x10Button = new RadioButton { Text = "10x10", Location = new Point(290, 40) };
            m_6x6Button.Checked = true;
            this.Controls.Add(m_6x6Button);
            this.Controls.Add(m_8x8Button);
            this.Controls.Add(m_10x10Button);

            Label playersLabel = new Label
            {
                Text = "Players:",
                Location = new Point(20, 70)
            };
            this.Controls.Add(playersLabel);

            Label player1Label = new Label { Text = "Player 1:", Location = new Point(36, 100) };
            player1Label.AutoSize = false;
            player1Label.Size = new Size(75, 20);
            m_Player1NameTextBox = new TextBox { Location = new Point(120, 100) };
            this.Controls.Add(player1Label);
            this.Controls.Add(m_Player1NameTextBox);

            m_Player2CheckBox = new CheckBox { Text = "Player 2:", Location = new Point(20, 130) };
            m_Player2CheckBox.AutoSize = false;
            m_Player2CheckBox.Size = new Size(75, 20);
            m_Player2NameTextBox = new TextBox { Location = new Point(120, 130), Enabled = false };
            m_Player2CheckBox.CheckedChanged += (s, e) => m_Player2NameTextBox.Enabled = m_Player2CheckBox.Checked;
            this.Controls.Add(m_Player2CheckBox);
            this.Controls.Add(m_Player2NameTextBox);

            Button doneButton = new Button
            {
                Text = "Done",
                Location = new Point(280, 170)
            };
            doneButton.Click += doneButton_Click;
            this.Controls.Add(doneButton);
        }

        private void doneButton_Click(object? sender, EventArgs e)
        {
            createGame();
            this.Close();
        }

        private void createGame()
        {
            string mainPlayerName = m_Player1NameTextBox.Text;
            string? secondPlayerName = m_Player2CheckBox.Checked ? m_Player2NameTextBox.Text : null;

            int boardSize = m_6x6Button.Checked ? 6 : (m_8x8Button.Checked ? 8 : 10);

            GameType gameType = m_Player2CheckBox.Checked ? GameType.PlayerVsPlayer : GameType.PlayerVsComp;

            GameConfig config = new GameConfig(boardSize, gameType);
            m_Game = new Game(mainPlayerName, secondPlayerName, config);
        }

        public Game GetGame()
        {
            this.ShowDialog();
            return m_Game;
        }
    }
}
