
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
        private const int DefaultAttributePointPool = 25;

        private readonly Label _pointsLabel;
        private readonly Dictionary<AttributeType, (Label valueLabel, Button plusBtn, Button minusBtn)> _attributeControls = [];
        private readonly ListView _customMapList;
        private readonly ComboBox _mapStyleCombo;
        private readonly RadioGroup _mapTypeRadio;
        private readonly List<string> _availableMaps;
        private readonly ListView _classList;
        private readonly List<ClassConfig> _availableClasses;
        private readonly Label _classPreviewLabel;
        private readonly Label _skillPointsLabel;
        private readonly Dictionary<SkillType, (Label valueLabel, Button plusBtn, Button minusBtn)> _skillControls = [];
        private int _availablePoints = 0;
        private int _customAvailablePoints = DefaultAttributePointPool;
        private int _availableSkillPoints = 0;
        private int _customSkillPointsRemaining = 0;
        private int _customSkillPointsTotal = 0;
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
                Width = Dim.Percent(50) - 2
            };
            classContainer.Add(classListLabel);

            ListView classList = new()
            {
                X = 1,
                Y = 7,
                Width = Dim.Percent(50) - 2,
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

            FrameView classPreview = new()
            {
                Title = "Class Preview",
                X = Pos.Right(classList) + 1,
                Y = 5,
                Width = Dim.Fill() - 2,
                Height = Dim.Fill() - 6
            };

            Label classPreviewLabel = new()
            {
                X = 1,
                Y = 1,
                Width = Dim.Fill() - 2,
                Height = Dim.Fill() - 2
            };
            classPreview.Add(classPreviewLabel);
            classContainer.Add(classPreview);

            classStep.Add(classContainer);
            AddStep(classStep);

            _classList = classList;
            _classPreviewLabel = classPreviewLabel;
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
                HelpText = "Allocate points to your character's skills.",
                BackButtonText = "Back"
            };

            FrameView skillsContainer = new()
            {
                Title = "Character Skills",
                X = 1,
                Y = 1,
                Width = Dim.Fill() - 2,
                Height = Dim.Fill() - 2
            };

            _skillPointsLabel = new Label
            {
                Text = $"Skill Points Remaining: {_availableSkillPoints}",
                X = 1,
                Y = 1,
                Width = Dim.Fill()
            };
            skillsContainer.Add(_skillPointsLabel);

            View leftSkillsColumn = new()
            {
                X = 1,
                Y = 3,
                Width = Dim.Percent(50) - 2,
                Height = Dim.Fill() - 4
            };
            View rightSkillsColumn = new()
            {
                X = Pos.Right(leftSkillsColumn) + 1,
                Y = 3,
                Width = Dim.Fill() - 2,
                Height = Dim.Fill() - 4
            };
            skillsContainer.Add(leftSkillsColumn);
            skillsContainer.Add(rightSkillsColumn);

            SkillType[] skillTypes = Enum.GetValues<SkillType>();
            int rowsPerColumn = (int)Math.Ceiling(skillTypes.Length / 2d);
            for (int i = 0; i < skillTypes.Length; i++)
            {
                SkillType skillType = skillTypes[i];
                bool leftColumn = i < rowsPerColumn;
                int rowIndex = leftColumn ? i : i - rowsPerColumn;
                CreateSkillRow(leftColumn ? leftSkillsColumn : rightSkillsColumn, skillType, rowIndex);
            }

            skillsStep.Add(skillsContainer);
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

        private void CreateSkillRow(View container, SkillType skillType, int rowIndex)
        {
            const int labelWidth = 12;
            const int valueWidth = 3;
            const int buttonWidth = 3;
            const int valueX = labelWidth + 1;
            const int minusX = valueX + valueWidth + 1;
            const int plusX = minusX + buttonWidth + 1;
            int yPos = rowIndex;

            Label label = new()
            {
                Text = skillType.ToString(),
                X = 0,
                Y = yPos,
                Width = labelWidth
            };
            container.Add(label);

            Label valueLabel = new()
            {
                Text = GetSkillRank(skillType).ToString(System.Globalization.CultureInfo.InvariantCulture),
                X = valueX,
                Y = yPos,
                Width = valueWidth
            };
            container.Add(valueLabel);

            Button minusBtn = new()
            {
                Text = "-",
                X = minusX,
                Y = yPos,
                Width = buttonWidth
            };
            minusBtn.Accepting += (sender, e) => ModifySkill(skillType, -1, e);
            container.Add(minusBtn);

            Button plusBtn = new()
            {
                Text = "+",
                X = plusX,
                Y = yPos,
                Width = buttonWidth
            };
            plusBtn.Accepting += (sender, e) => ModifySkill(skillType, 1, e);
            container.Add(plusBtn);

            _skillControls[skillType] = (valueLabel, plusBtn, minusBtn);
            UpdateSkillButtonStates();
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
            RecalculateSkillPoints();
            UpdateClassPreview();
        }

        private void ModifySkill(SkillType skillType, int delta, Terminal.Gui.Input.CommandEventArgs args)
        {
            if (!_useCustomCharacter)
            {
                return;
            }

            int currentValue = GetSkillRank(skillType);
            int newValue = currentValue + delta;

            if (newValue < 0 || newValue > ProgressionRules.HardSkillCap)
            {
                return;
            }

            if (delta > 0 && _availableSkillPoints <= 0)
            {
                return;
            }

            SetSkillRank(skillType, newValue);
            _availableSkillPoints -= delta;
            _customSkillPointsRemaining = _availableSkillPoints;

            _skillPointsLabel.Text = $"Skill Points Remaining: {_availableSkillPoints}";
            args.Handled = true;
            _skillControls[skillType].valueLabel.Text = newValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            UpdateSkillButtonStates();
            UpdateClassPreview();
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

                    controls.minusBtn.Enabled = currentValue > MinAttributeValue;
                    controls.plusBtn.Enabled = _availablePoints > 0 && currentValue < MaxAttributeValue;
                }
            }
        }

        private void UpdateSkillButtonStates()
        {
            foreach (SkillType skillType in Enum.GetValues<SkillType>())
            {
                if (_skillControls.TryGetValue(skillType, out var controls))
                {
                    int currentValue = GetSkillRank(skillType);

                    if (!_useCustomCharacter)
                    {
                        controls.minusBtn.Enabled = false;
                        controls.plusBtn.Enabled = false;
                        continue;
                    }

                    controls.minusBtn.Enabled = currentValue > 0;
                    controls.plusBtn.Enabled = _availableSkillPoints > 0 && currentValue < ProgressionRules.HardSkillCap;
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

        private int GetSkillRank(SkillType type)
        {
            return SelectedSkills.GetRank(type);
        }

        private void SetSkillRank(SkillType type, int value)
        {
            SelectedSkills.SetRank(type, value);
        }

        private void RecalculateSkillPoints()
        {
            if (!_useCustomCharacter)
            {
                return;
            }

            int total = CalculateSkillPointPool(SelectedAttributes);
            int spent = GetTotalSkillRanks(SelectedSkills);

            _customSkillPointsTotal = total;
            _availableSkillPoints = Math.Max(0, total - spent);
            _customSkillPointsRemaining = _availableSkillPoints;
            UpdateSkillPointsLabel();
            UpdateSkillButtonStates();
        }

        private void UpdateSkillPointsLabel()
        {
            _skillPointsLabel.Text = $"Skill Points Remaining: {_availableSkillPoints}";
        }

        private void UpdateSkillLabels()
        {
            foreach (SkillType skillType in Enum.GetValues<SkillType>())
            {
                if (_skillControls.TryGetValue(skillType, out var controls))
                {
                    controls.valueLabel.Text = GetSkillRank(skillType).ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
            }
        }

        private static int CalculateSkillPointPool(Attributes attributes)
        {
            return ProgressionRules.SkillPointsPerLevel(attributes);
        }

        private static int GetTotalSkillRanks(Skills skills)
        {
            int total = 0;
            foreach (SkillType skillType in Enum.GetValues<SkillType>())
            {
                total += skills.GetRank(skillType);
            }

            return total;
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
                RecalculateSkillPoints();
                SetAttributeControlsEnabled(true);
                SetSkillControlsEnabled(true);
            }
            else
            {
                _useCustomCharacter = false;
                _customAvailablePoints = _availablePoints;
                CaptureCustomAttributes();
                CaptureCustomSkills();
                _customSkillPointsRemaining = _availableSkillPoints;
                _availablePoints = 0;
                _availableSkillPoints = 0;
                _classList.Enabled = true;
                ApplySelectedClass();
                SetAttributeControlsEnabled(false);
                SetSkillControlsEnabled(false);
            }

            _pointsLabel.Text = $"Points Remaining: {_availablePoints}";
            UpdateButtonStates();
            UpdateSkillPointsLabel();
            UpdateSkillLabels();
            UpdateSkillButtonStates();
            UpdateClassPreview();
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
            UpdateSkillLabels();
            UpdateClassPreview();
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

        private void CaptureCustomSkills()
        {
            foreach (SkillType skillType in Enum.GetValues<SkillType>())
            {
                _customSkills.SetRank(skillType, SelectedSkills.GetRank(skillType));
            }
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

        private void SetSkillControlsEnabled(bool enabled)
        {
            foreach (var controls in _skillControls.Values)
            {
                controls.plusBtn.Enabled = enabled;
                controls.minusBtn.Enabled = enabled;
            }
        }

        private void UpdateClassPreview()
        {
            if (SelectedClass == null)
            {
                _classPreviewLabel.Text = "No class selected.";
                return;
            }

            if (_useCustomCharacter)
            {
                _classPreviewLabel.Text = BuildCustomPreview();
                return;
            }

            _classPreviewLabel.Text = BuildClassPreview(SelectedClass);
        }

        private string BuildCustomPreview()
        {
            string attributes = FormatAttributes(SelectedAttributes);
            string skills = FormatSkills(SelectedSkills, SelectedAttributes);
            string weaponName = SelectedClass?.startupWeapon.Name ?? "Unknown";
            string armorName = SelectedClass?.startupArmor.Name ?? "Unknown";

            return $"Mode: Custom Character\n\nAttributes:\n{attributes}\n\nSkills:\n{skills}\n\nEquipment:\nWeapon: {weaponName}\nArmor: {armorName}";
        }

        private static string BuildClassPreview(ClassConfig classConfig)
        {
            string attributes = FormatAttributes(classConfig.Attributes);
            string skills = FormatSkills(classConfig.Skills, classConfig.Attributes);
            string weaponName = classConfig.startupWeapon.Name;
            string armorName = classConfig.startupArmor.Name;

            return $"Class: {classConfig.Name}\n\nAttributes:\n{attributes}\n\nSkills:\n{skills}\n\nEquipment:\nWeapon: {weaponName}\nArmor: {armorName}";
        }

        private static string FormatAttributes(Attributes attributes)
        {
            return $"Strength: {attributes.Strength}\n" +
                   $"Perception: {attributes.Perception}\n" +
                   $"Agility: {attributes.Agility}\n" +
                   $"Charisma: {attributes.Charisma}\n" +
                   $"Intelligence: {attributes.Intelligence}\n" +
                   $"Karma: {attributes.Karma}";
        }

        private static string FormatSkills(Skills skills, Attributes attributes)
        {
            List<string> entries = [];
            foreach (SkillType skillType in Enum.GetValues<SkillType>())
            {
                int rank = skills.GetRank(skillType);
                if (rank > 0)
                {
                    int value = skills.GetValue(skillType, attributes);
                    entries.Add($"{skillType}: {rank} (Total {value})");
                }
            }

            if (entries.Count == 0)
            {
                return "None";
            }

            return string.Join("\n", entries);
        }
    }
}
