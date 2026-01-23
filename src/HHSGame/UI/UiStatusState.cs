namespace HHSGame.UI
{
    public sealed class UiStatusState
    {
        public string? ModeOverride { get; private set; }
        public string? DetailOverride { get; private set; }

        public event EventHandler? Changed;

        public void SetOverride(string mode, string detail = "")
        {
            ModeOverride = mode;
            DetailOverride = detail;
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public void ClearOverride()
        {
            if (ModeOverride == null && DetailOverride == null)
            {
                return;
            }

            ModeOverride = null;
            DetailOverride = null;
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
