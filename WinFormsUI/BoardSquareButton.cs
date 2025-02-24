namespace WindowsUI
{
    internal class BoardSquareButton : Button
    {
        public Coord Coord { get; set; }
        private bool activated;

        public bool Activated
        {
            get
            {
                return activated;
            }
        }

        public void ToggleActivation()
        {
            if (activated)
            {
                activated = false;
                unmarkBlue();
            }

            else
            {
                activated = true;
                markBlue();
            }
        }

        private void markBlue()
        {
            this.BackColor = Color.LightBlue;
        }

        private void unmarkBlue()
        {
            this.BackColor = Color.White;
        }

        protected override void OnClick(EventArgs e)
        {
            ToggleActivation();
            base.OnClick(e);
        }
    }
}
