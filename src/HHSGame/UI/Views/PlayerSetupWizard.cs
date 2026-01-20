
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using HHSGame.Core.Stats;
using HHSGame.Core.Map;
using System.Collections.ObjectModel;

namespace HHSGame.UI.Views
{
    public class PlayerSetupWizard : Wizard
    {
        private readonly Label _pointsLabel;
        private readonly Dictionary<AttributeType, (Label valueLabel, Button plusBtn, Button minusBtn)> _attributeControls = [];
        private readonly ListView _customMapList;
        private readonly ComboBox _mapStyleCombo;
        private readonly RadioGroup _mapTypeRadio;
        private readonly List<string> _availableMaps;
        private int _availablePoints = 0;

        public Attributes SelectedAttributes { get; } = new()
        {
            Strength = 5,
            Perception = 5,
            Agility = 5,
            Charisma = 5,
            Intelligence = 5
        };

        public string? SelectedMap { get; private set; }
        public bool UseCustomMap { get; private set; }

        public PlayerSetupWizard()
        {
            Title = "Player Setup Wizard";
            X = Pos.Percent(25);
            Y = Pos.Percent(25);
            Width = Dim.Percent(50);
            Height = Dim.Percent(60);

            // Map selection step
            WizardStep mapStep = new()
            {
                Title = "Map Selection",
                HelpText = "Choose between generated maps or load a custom map from file.",
                NextButtonText = "Next",
            };

            FrameView mapContainer = new()
            {
                Title = "Map Options",
                X = 1,
                Y = 1,
                Width = Dim.Fill() - 2,
                Height = Dim.Fill() - 2
            };

            RadioGroup mapTypeRadio = new()
            {
                X = 1,
                Y = 1,
                Width = Dim.Fill() - 2,
                Height = 3,
                RadioLabels = new[] { "Generated Map", "Custom Map" },
                SelectedItem = 0
            };
            mapContainer.Add(mapTypeRadio);

            ComboBox mapStyleCombo = new()
            {
                X = 1,
                Y = 5,
                Width = Dim.Fill() - 2,
                Height = 1,
                ReadOnly = true,
                Text = "Cave"
            };
            mapStyleCombo.SetSource<string>(new ObservableCollection<string>(Enum.GetValues<MapStyle>().Select(s => s.ToString()).ToList()));
            mapContainer.Add(mapStyleCombo);

            Label customMapLabel = new()
            {
                Text = "Available Maps:",
                X = 1,
                Y = 7,
                Width = Dim.Fill()
            };
            mapContainer.Add(customMapLabel);

            ListView customMapList = new()
            {
                X = 1,
                Y = 9,
                Width = Dim.Fill() - 2,
                Height = Dim.Fill() - 10
            };

            _availableMaps = MapLoader.GetAvailableMaps("data/maps");
            _customMapList = customMapList;
            _mapStyleCombo = mapStyleCombo;
            _mapTypeRadio = mapTypeRadio;

            customMapList.SetSource<string>(new ObservableCollection<string>(_availableMaps));

            if (_availableMaps.Count > 0)
            {
                customMapList.SelectedItem = 0;
            }
            else
            {
                customMapList.SetSource<string>(new ObservableCollection<string>(new[] { "No maps found" }));
                customMapList.Enabled = false;
            }

            mapContainer.Add(customMapList);

            // Toggle custom map controls based on radio selection
            mapTypeRadio.SelectedItemChanged += (sender, args) =>
            {
                bool useCustom = args.SelectedItem == 1;
                mapStyleCombo.Enabled = !useCustom;
                customMapList.Enabled = useCustom && _availableMaps.Count > 0;
            };

            // Initial state
            mapStyleCombo.Enabled = true;
            customMapList.Enabled = false;

            mapStep.Add(mapContainer);
            AddStep(mapStep);

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
            foreach (AttributeType attrType in Enum.GetValues<AttributeType>().Where(type => type != AttributeType.Karma))
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
                // Capture map selection
                UseCustomMap = _mapTypeRadio.SelectedItem == 1;

                if (UseCustomMap && _availableMaps.Count > 0 && _customMapList.SelectedItem is int selectedIndex && selectedIndex >= 0 && selectedIndex < _availableMaps.Count)
                {
                    SelectedMap = _availableMaps[selectedIndex];
                }
                else
                {
                    // Use generated map style
                    SelectedMap = _mapStyleCombo.Text;
                }

                this.Visible = false;
            };
        }

        private void CreateAttributeRow(FrameView container, AttributeType attrType, ref int yPos)
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
                Text = GetAttributeValue(attrType).ToString(System.Globalization.CultureInfo.InvariantCulture),
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

        private void ModifyAttribute(AttributeType attrType, int delta, Terminal.Gui.Input.CommandEventArgs args)
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
            _attributeControls[attrType].valueLabel.Text = newValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            foreach (AttributeType attrType in Enum.GetValues<AttributeType>().Where(type => type != AttributeType.Karma))
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

        private int GetAttributeValue(AttributeType type)
        {
            return type switch
            {
                AttributeType.Strength => SelectedAttributes.Strength,
                AttributeType.Perception => SelectedAttributes.Perception,
                AttributeType.Agility => SelectedAttributes.Agility,
                AttributeType.Charisma => SelectedAttributes.Charisma,
                AttributeType.Intelligence => SelectedAttributes.Intelligence,
                _ => 5
            };
        }

        private void SetAttributeValue(AttributeType type, int value)
        {
            switch (type)
            {
                case AttributeType.Strength:
                    SelectedAttributes.Strength = value;
                    break;
                case AttributeType.Perception:
                    SelectedAttributes.Perception = value;
                    break;
                case AttributeType.Agility:
                    SelectedAttributes.Agility = value;
                    break;
                case AttributeType.Charisma:
                    SelectedAttributes.Charisma = value;
                    break;
                case AttributeType.Intelligence:
                    SelectedAttributes.Intelligence = value;
                    break;
            }
        }
    }
}
