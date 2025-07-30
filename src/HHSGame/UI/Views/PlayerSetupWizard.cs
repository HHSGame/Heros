
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using HHSGame.Core.Stats;

namespace HHSGame.UI.Views
{
    public class PlayerSetupWizard : Wizard
    {
        private readonly Label _pointsLabel;
        private readonly Dictionary<CoreAttributeType, (Label valueLabel, Button plusBtn, Button minusBtn)> _attributeControls = [];
        private int _availablePoints = 10;

        public CoreAttributes SelectedAttributes { get; } = new()
        {
            Strength = 5,
            Perception = 5,
            Agility = 5,
            Charisma = 5,
            Intelligence = 5
        };

        public PlayerSetupWizard()
        {
            Title = "Player Setup Wizard";
            X = Pos.Percent(25);
            Y = Pos.Percent(25);
            Width = Dim.Percent(50);
            Height = Dim.Percent(60);

            // Attributes step
            WizardStep attributesStep = new()
            {
                Title = "Attributes",
                HelpText = "Allocate points to your character's core attributes.",
                NextButtonText = "Next",
            };

            FrameView container = new()
            {
                Title = "Character Attributes",
                X = 1,
                Y = 1,
                Width = Dim.Fill() - 2,
                Height = Dim.Fill() - 2
            };

            // Points remaining
            _pointsLabel = new Label
            {
                Text = $"Points Remaining: {_availablePoints}",
                X = 1,
                Y = 1,
                Width = Dim.Fill()
            };
            container.Add(_pointsLabel);

            int yPos = 3;
            foreach (CoreAttributeType attrType in Enum.GetValues<CoreAttributeType>())
            {
                CreateAttributeRow(container, attrType, ref yPos);
                yPos += 2;
            }

            attributesStep.Add(container);
            AddStep(attributesStep);

            // Skills step (placeholder)
            WizardStep skillsStep = new()
            {
                Title = "Skills",
                HelpText = "Skills will be derived from your attributes.",
                BackButtonText = "Back"
            };

            Label skillsLabel = new()
            {
                Text = "Skills are automatically calculated based on your attributes.",
                X = Pos.Center(),
                Y = Pos.Center(),
                Width = Dim.Fill()
            };
            skillsStep.Add(skillsLabel);
            AddStep(skillsStep);

            this.Finished += (sender, args) =>
            {
                this.Visible = false;
            };
        }

        private void CreateAttributeRow(FrameView container, CoreAttributeType attrType, ref int yPos)
        {
            Label label = new()
            {
                Text = attrType.ToString(),
                X = 1,
                Y = yPos,
                Width = 12
            };
            container.Add(label);

            Label valueLabel = new()
            {
                Text = GetAttributeValue(attrType).ToString(),
                X = 14,
                Y = yPos,
                Width = 3
            };
            container.Add(valueLabel);

            Button minusBtn = new()
            {
                Text = "-",
                X = 18,
                Y = yPos,
                Width = 3
            };
            minusBtn.Accepting += (sender, e) => ModifyAttribute(attrType, -1, e);
            container.Add(minusBtn);

            Button plusBtn = new()
            {
                Text = "+",
                X = 22,
                Y = yPos,
                Width = 3
            };
            plusBtn.Accepting += (sender, e) => ModifyAttribute(attrType, 1, e);
            container.Add(plusBtn);

            _attributeControls[attrType] = (valueLabel, plusBtn, minusBtn);
            UpdateButtonStates();
        }

        private void ModifyAttribute(CoreAttributeType attrType, int delta, Terminal.Gui.Input.CommandEventArgs args)
        {
            int currentValue = GetAttributeValue(attrType);
            int newValue = currentValue + delta;

            if (newValue is < 1 or > 10)
            {
                return;
            }

            if (delta > 0 && _availablePoints <= 0)
            {
                return;
            }

            if (delta < 0 && currentValue <= 1)
            {
                return;
            }

            SetAttributeValue(attrType, newValue);
            _availablePoints -= delta;

            _pointsLabel.Text = $"Points Remaining: {_availablePoints}";
            args.Handled = true;
            _attributeControls[attrType].valueLabel.Text = newValue.ToString();
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            foreach (CoreAttributeType attrType in Enum.GetValues<CoreAttributeType>())
            {
                if (_attributeControls.TryGetValue(attrType, out (Label _, Button plusBtn, Button minusBtn) value))
                {
                    (_, Button plusBtn, Button minusBtn) = value;
                    int currentValue = GetAttributeValue(attrType);

                    minusBtn.Enabled = currentValue > 1;
                    plusBtn.Enabled = _availablePoints > 0 && currentValue < 10;
                }
            }
        }

        private int GetAttributeValue(CoreAttributeType type)
        {
            return type switch
            {
                CoreAttributeType.Strength => SelectedAttributes.Strength,
                CoreAttributeType.Perception => SelectedAttributes.Perception,
                CoreAttributeType.Agility => SelectedAttributes.Agility,
                CoreAttributeType.Charisma => SelectedAttributes.Charisma,
                CoreAttributeType.Intelligence => SelectedAttributes.Intelligence,
                _ => 5
            };
        }

        private void SetAttributeValue(CoreAttributeType type, int value)
        {
            switch (type)
            {
                case CoreAttributeType.Strength:
                    SelectedAttributes.Strength = value;
                    break;
                case CoreAttributeType.Perception:
                    SelectedAttributes.Perception = value;
                    break;
                case CoreAttributeType.Agility:
                    SelectedAttributes.Agility = value;
                    break;
                case CoreAttributeType.Charisma:
                    SelectedAttributes.Charisma = value;
                    break;
                case CoreAttributeType.Intelligence:
                    SelectedAttributes.Intelligence = value;
                    break;
            }
        }
    }
}