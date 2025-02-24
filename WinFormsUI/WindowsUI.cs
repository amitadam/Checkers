namespace WindowsUI
{
    internal class WindowsUI
    {
        Game? m_Game;

        public void SetGameSettings()
        {
            SettingsForm settingsForm = new SettingsForm();
            m_Game = settingsForm.GetGame();
        }

        public void Play()
        {
            if (m_Game != null)
            {
                GameForm gameForm = new GameForm(m_Game);
                gameForm.FormClosing += gameForm_Closing;
                gameForm.Start();
            }    
        }

        public void gameForm_Closing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("This round has finished. Do you wish to quit it and start a new round?",
                                                 "Damka",
                                                 MessageBoxButtons.YesNoCancel,
                                                 MessageBoxIcon.Question);

            GameForm gameForm = (GameForm) sender;

            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
            }

            else if (result == DialogResult.Yes)
            {
                m_Game.RestartMatch();
                gameForm.UpdateGameForm();
                e.Cancel = true;
            }

            else
            {
                e.Cancel = false;
            }
        }
    }
}
