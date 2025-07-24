
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace HHSGame.UI.Views
{
    public class PlayerSetupWizard : Wizard
    {
        public PlayerSetupWizard()
        {
            Title = "Player Setup Wizard";
            X = Pos.Percent(25);
            Y = Pos.Percent(25);
            Width = Dim.Percent(50);
            Height = Dim.Percent(50);

            WizardStep wizardStep = new()
            {
                Title = "Attributes",
            };
            AddStep(wizardStep);

            AddStep(new()
            {
                Title = "Skils",
                BackButtonText = "上一步",
            });

            this.Finished += (sender, args) =>
            {
                this.Visible = false;
            };
        }
    }
}