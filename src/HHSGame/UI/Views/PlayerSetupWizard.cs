
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using HHSGame.Core.Classes;
using HHSGame.Core.Stats;
using HHSGame.Core.Map;
using System.Collections.ObjectModel;

namespace HHSGame.UI.Views
{
    public class PlayerSetupWizard : Wizard
    {
        private const int MinAttributeValue = 1;
        private const int MaxAttributeValue = 10;
        private const int DefaultAttributeValue = 5;

        private readonly Label _pointsLabel;
        private readonly Dictionary<AttributeType, (Label valueLabel, Button plusBtn, Button minusBtn)> _attributeControls = [];
        private readonly ListView _customMapList;
        private readonly ComboBox _mapStyleCombo;
        private readonly RadioGroup _mapTypeRadio;
        private readonly List<string> _availableMaps;
        private readonly ListView _classList;
        private readonly List<ClassConfig> _availableClasses;
        private int _availablePoints = 0;
        private int _customAvailablePoints = 0;
        private bool _useCustomCharacter;
        private readonly Attributes _customAttributes = new()
        {
            Strength = DefaultAttributeValue,
            Perception = DefaultAttributeValue,
            Agility = DefaultAttributeValue,
            Charisma = DefaultAttributeValue,
            Intelligence = DefaultAttributeValue
        };
        private readonly Skills _customSkills = new();

        public Attributes SelectedAttributes { get; } = new()
        {
            Strength = DefaultAttributeValue,
            Perception = DefaultAttributeValue,
            Agility = DefaultAttributeValue,
            Charisma = DefaultAttributeValue,
            Intelligence = DefaultAttributeValue
        };
        public Skills SelectedSkills { get; private set; } = new();
        public ClassConfig? SelectedClass { get; private set; }
        public bool UseCustomCharacter => _useCustomCharacter;

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

            // Class selection step
            WizardStep classStep = new()
            {
                Title = "Class Selection",
                HelpText = "Choose a class or customize your character attributes.",
                NextButtonText = "Next",
            };

            FrameView classContainer = new()
            {
                Title = "Character Options",
                X = 1,
                Y = 1,
                Width = Dim.Fill() - 2,
                Height = Dim.Fill() - 2
            };

            RadioGroup classModeRadio = new()
            {
                X = 1,
                Y = 1,
                Width = Dim.Fill() - 2,
                Height = 3,
                RadioLabels = new[] { "Use Class", "Custom Character" },
                SelectedItem = 0
            };
            classContainer.Add(classModeRadio);

            Label classListLabel = new()
            {
                Text = "Available Classes:",
                X = 1,
                Y = 5,
                Width = Dim.Fill() - 2
            };
            classContainer.Add(classListLabel);

            ListView classList = new()
            {
                X = 1,
                Y = 7,
                Width = Dim.Fill() - 2,
                Height = Dim.Fill() - 8
            };

            _availableClasses =
            [
                Classes.Unemployed,
                Classes.Warrior,
                Classes.Thief,
                Classes.Alchemist
            ];
            classList.SetSource<string>(new ObservableCollection<string>(_availableClasses.Select(c => c.Name).ToList()));
            classList.SelectedItem = 0;

            classContainer.Add(classList);
            classStep.Add(classContainer);
            AddStep(classStep);

            _classList = classList;
            SelectedClass = _availableClasses[0];

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

            classModeRadio.SelectedItemChanged += (sender, args) =>
            {
                bool useCustom = args.SelectedItem == 1;
                SetCharacterMode(useCustom);
            };

            classList.SelectedItemChanged += (sender, args) =>
            {
                if (!_useCustomCharacter)
                {
                    ApplySelectedClass();
                }
            };

            _useCustomCharacter = true;
            SetCharacterMode(false);

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
            if (!_useCustomCharacter)
            {
                return;
            }

            int currentValue = GetAttributeValue(attrType);
            int newValue = currentValue + delta;

            if (newValue is < MinAttributeValue or > MaxAttributeValue)
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
                if (_attributeControls.TryGetValue(attrType, out var controls))
                {
                    int currentValue = GetAttributeValue(attrType);

                    if (!_useCustomCharacter)
                    {
                        controls.minusBtn.Enabled = false;
                        controls.plusBtn.Enabled = false;
                        continue;
                    }

                    controls.minusBtn.Enabled = currentValue > 1;
                    controls.plusBtn.Enabled = _availablePoints > 0 && currentValue < 10;
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
                _ => DefaultAttributeValue
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

        private void SetCharacterMode(bool useCustom)
        {
            if (_useCustomCharacter == useCustom)
            {
                return;
            }

            if (useCustom)
            {
                _useCustomCharacter = true;
                _classList.Enabled = false;
                _availablePoints = _customAvailablePoints;
                SelectedClass = Classes.Unemployed;
                ApplyAttributes(_customAttributes);
                SelectedSkills = _customSkills.Clone();
                SetAttributeControlsEnabled(true);
            }
            else
            {
                _useCustomCharacter = false;
                _customAvailablePoints = _availablePoints;
                CaptureCustomAttributes();
                _availablePoints = 0;
                _classList.Enabled = true;
                ApplySelectedClass();
                SetAttributeControlsEnabled(false);
            }

            _pointsLabel.Text = $"Points Remaining: {_availablePoints}";
            UpdateButtonStates();
        }

        private void ApplySelectedClass()
        {
            if (_classList.SelectedItem is int index && index >= 0 && index < _availableClasses.Count)
            {
                SelectedClass = _availableClasses[index];
            }
            SelectedClass ??= Classes.Unemployed;

            ApplyAttributes(SelectedClass.Attributes);
            SelectedSkills = SelectedClass.Skills.Clone();
        }

        private void ApplyAttributes(Attributes source)
        {
            SelectedAttributes.Strength = source.Strength;
            SelectedAttributes.Perception = source.Perception;
            SelectedAttributes.Agility = source.Agility;
            SelectedAttributes.Charisma = source.Charisma;
            SelectedAttributes.Intelligence = source.Intelligence;
            UpdateAttributeLabels();
        }

        private void CaptureCustomAttributes()
        {
            _customAttributes.Strength = SelectedAttributes.Strength;
            _customAttributes.Perception = SelectedAttributes.Perception;
            _customAttributes.Agility = SelectedAttributes.Agility;
            _customAttributes.Charisma = SelectedAttributes.Charisma;
            _customAttributes.Intelligence = SelectedAttributes.Intelligence;
        }

        private void UpdateAttributeLabels()
        {
            foreach (AttributeType attrType in Enum.GetValues<AttributeType>().Where(type => type != AttributeType.Karma))
            {
                if (_attributeControls.TryGetValue(attrType, out var controls))
                {
                    controls.valueLabel.Text = GetAttributeValue(attrType).ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
            }
        }

        private void SetAttributeControlsEnabled(bool enabled)
        {
            foreach (var controls in _attributeControls.Values)
            {
                controls.plusBtn.Enabled = enabled;
                controls.minusBtn.Enabled = enabled;
            }
        }
    }
}
