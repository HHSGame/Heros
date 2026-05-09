using System.Collections.ObjectModel;
using HHSGame.Core.CharacterCreation;
using HHSGame.Core.Classes;
using HHSGame.Core.Stats;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Terminal.Gui.Input;
using Terminal.Gui.Drivers;

namespace HHSGame.UI.Views
{
    /// <summary>
    /// Dialogue-based character creation UI.
    /// Walks the player through: Welcome → Map → Playstyle → Name → Dice Roll → Class → Complete.
    /// </summary>
    public class CharacterCreationDialog : Window
    {
        private readonly CharacterCreationManager manager;
        private readonly ListView listView;
        private readonly Label titleLabel;
        private readonly Label contentLabel;
        private readonly Label helpLabel;
        private readonly TextField nameField;

        public event EventHandler? Completed;
        public event EventHandler? Cancelled;

        public CharacterCreationManager Manager => manager;

        public CharacterCreationDialog(CharacterCreationManager manager)
        {
            this.manager = manager;

            Title = "Character Creation";
            X = 0;
            Y = 0;
            Width = Dim.Fill();
            Height = Dim.Fill();
            Visible = false;

            titleLabel = new Label
            {
                X = 1,
                Y = 1,
                Width = Dim.Fill() - 2,
                Text = string.Empty
            };
            Add(titleLabel);

            contentLabel = new Label
            {
                X = 1,
                Y = 3,
                Width = Dim.Fill() - 2,
                Height = Dim.Fill() - 5,
                Text = string.Empty
            };
            Add(contentLabel);

            listView = new ListView
            {
                X = 1,
                Y = 3,
                Width = Dim.Fill() - 2,
                Height = Dim.Fill() - 5,
                AllowsMarking = false,
                AllowsMultipleSelection = false,
                Visible = false
            };
            Add(listView);

            nameField = new TextField
            {
                X = 1,
                Y = 5,
                Width = 30,
                Text = string.Empty,
                Visible = false
            };
            Add(nameField);

            helpLabel = new Label
            {
                X = 1,
                Y = Pos.Bottom(this) - 2,
                Width = Dim.Fill() - 2,
                Text = "↑↓: Select | Enter: Confirm | R: Re-roll | Esc: Cancel"
            };
            Add(helpLabel);
        }

        public void Start()
        {
            manager.Initialize();
            Visible = true;
            RenderCurrentStep();
        }

        public bool HandleKeyEvent(Key key)
        {
            switch (manager.CurrentStep)
            {
                case CreationStep.Welcome:
                    return HandleWelcomeKey(key);
                case CreationStep.MapSelection:
                    return HandleListSelectionKey(key, OnMapSelected);
                case CreationStep.PlaystyleChoice:
                    return HandleListSelectionKey(key, OnPlaystyleSelected);
                case CreationStep.NameInput:
                    return HandleNameKey(key);
                case CreationStep.DiceRoll:
                    return HandleDiceKey(key);
                case CreationStep.ClassConfirmation:
                    return HandleListSelectionKey(key, OnClassSelected);
                default:
                    return false;
            }
        }

        private bool HandleWelcomeKey(Key key)
        {
            if (key.KeyCode == KeyCode.Enter || key.KeyCode == KeyCode.Space)
            {
                manager.AdvanceFromWelcome();
                RenderCurrentStep();
                return true;
            }
            if (key.KeyCode == KeyCode.Esc)
            {
                Visible = false;
                Cancelled?.Invoke(this, EventArgs.Empty);
                return true;
            }
            return false;
        }

        private bool HandleListSelectionKey(Key key, Action<int> onConfirm)
        {
            if (key.KeyCode == KeyCode.CursorUp)
            {
                listView.MoveUp();
                return true;
            }
            if (key.KeyCode == KeyCode.CursorDown)
            {
                listView.MoveDown();
                return true;
            }
            if (key.KeyCode == KeyCode.Enter)
            {
                onConfirm(listView.SelectedItem);
                return true;
            }
            if (key.KeyCode == KeyCode.Esc)
            {
                Visible = false;
                Cancelled?.Invoke(this, EventArgs.Empty);
                return true;
            }
            return false;
        }

        private bool HandleNameKey(Key key)
        {
            if (key.KeyCode == KeyCode.Enter)
            {
                manager.ConfirmName(nameField.Text?.ToString() ?? string.Empty);
                RenderCurrentStep();
                return true;
            }
            if (key.KeyCode == KeyCode.Esc)
            {
                Visible = false;
                Cancelled?.Invoke(this, EventArgs.Empty);
                return true;
            }
            // Let the TextField handle normal typing
            return false;
        }

        private bool HandleDiceKey(Key key)
        {
            if (key.KeyCode == KeyCode.R)
            {
                manager.RerollDice();
                RenderCurrentStep();
                return true;
            }
            if (key.KeyCode == KeyCode.Enter)
            {
                manager.ConfirmDice();
                RenderCurrentStep();
                return true;
            }
            if (key.KeyCode == KeyCode.Esc)
            {
                Visible = false;
                Cancelled?.Invoke(this, EventArgs.Empty);
                return true;
            }
            return false;
        }

        private void OnMapSelected(int index)
        {
            if (index < 0) return;

            // 0 = Generated, 1+ = custom maps
            if (index == 0)
            {
                manager.SelectMap("Cave", false);
            }
            else
            {
                int mapIndex = index - 1;
                if (mapIndex < manager.AvailableMaps.Count)
                {
                    manager.SelectMap(manager.AvailableMaps[mapIndex], true);
                }
            }
            RenderCurrentStep();
        }

        private void OnPlaystyleSelected(int index)
        {
            if (index < 0 || index >= PlaystyleCatalog.All.Count) return;
            manager.SelectPlaystyle(PlaystyleCatalog.All[index]);
            RenderCurrentStep();
        }

        private void OnClassSelected(int index)
        {
            IReadOnlyList<ClassConfig> classes = manager.GetAvailableClasses();
            if (index < 0 || index >= classes.Count)
            {
                manager.ConfirmClass();
            }
            else
            {
                manager.SelectClass(classes[index]);
            }
            RenderCurrentStep();
        }

        private void RenderCurrentStep()
        {
            switch (manager.CurrentStep)
            {
                case CreationStep.Welcome:
                    RenderWelcome();
                    break;
                case CreationStep.MapSelection:
                    RenderMapSelection();
                    break;
                case CreationStep.PlaystyleChoice:
                    RenderPlaystyleChoice();
                    break;
                case CreationStep.NameInput:
                    RenderNameInput();
                    break;
                case CreationStep.DiceRoll:
                    RenderDiceRoll();
                    break;
                case CreationStep.ClassConfirmation:
                    RenderClassConfirmation();
                    break;
                case CreationStep.Complete:
                    Visible = false;
                    Completed?.Invoke(this, EventArgs.Empty);
                    break;
            }
        }

        private void RenderWelcome()
        {
            Title = "欢迎，冒险者";
            ShowContent(true);
            contentLabel.Text = "欢迎来到沦陷区的地下世界！\n\n" +
                "让我们通过一系列选择来创建你的角色。\n" +
                "你将选择地图、偏好风格、\n" +
                "掷骰子生成属性，并确认职业。\n\n" +
                "按回车键开始你的旅程。";
            helpLabel.Text = "回车: 开始 | Esc: 取消";
            nameField.Visible = false;
            listView.Visible = false;
        }

        private void RenderMapSelection()
        {
            Title = "第一步：选择地图";
            ShowContent(false);

            List<string> mapOptions = ["Generated Map (Cave)"];
            mapOptions.AddRange(manager.AvailableMaps.Select(m => $"Custom: {m}"));

            listView.SetSource<string>(new ObservableCollection<string>(mapOptions));
            listView.SelectedItem = 0;
            listView.Visible = true;

            helpLabel.Text = "↑↓: 选择 | 回车: 确认 | Esc: 取消";
        }

        private void RenderPlaystyleChoice()
        {
            Title = "第二步：选择身份偏好";
            ShowContent(false);

            List<string> options = PlaystyleCatalog.All
                .Select(p => $"{p.Name}: {p.Description}")
                .ToList();

            listView.SetSource<string>(new ObservableCollection<string>(options));
            listView.SelectedItem = 0;
            listView.Visible = true;

            helpLabel.Text = "↑↓: 选择 | 回车: 确认 | Esc: 取消";
        }

        private void RenderNameInput()
        {
            Title = "第三步：为你的角色命名";
            ShowContent(true);
            contentLabel.Text = $"建议名称：{manager.PlayerName}\n\n" +
                "输入自定义名称或按回车接受建议。";
            nameField.Text = manager.PlayerName;
            nameField.Visible = true;
            nameField.SetFocus();
            listView.Visible = false;

            helpLabel.Text = "回车: 确认名称 | Esc: 取消";
        }

        private void RenderDiceRoll()
        {
            Title = "第四步：掷骰子（4d6 取最高3）";
            ShowContent(true);

            string summary = manager.GetDiceSummary();
            string attrs = FormatAttributes(manager.RolledAttributes);
            string playstyle = manager.SelectedPlaystyle != null
                ? $"\n身份加成：{manager.SelectedPlaystyle.Name}"
                : "";

            contentLabel.Text = $"你的掷骰结果：{playstyle}\n\n{summary}\n\n" +
                $"最终属性：\n{attrs}\n\n" +
                "按 R 重新掷骰，回车接受。";
            nameField.Visible = false;
            listView.Visible = false;

            helpLabel.Text = "R: 重新掷骰 | 回车: 接受 | Esc: 取消";
        }

        private void RenderClassConfirmation()
        {
            Title = "第五步：确认职业";
            ShowContent(false);

            List<ClassConfig> classes = manager.GetAvailableClasses().ToList();
            List<string> options = classes.Select(c => c.Name).ToList();

            listView.SetSource<string>(new ObservableCollection<string>(options));

            // Pre-select the suggested class
            if (manager.SelectedClass != null)
            {
                int idx = classes.FindIndex(c => c.Name == manager.SelectedClass.Name);
                if (idx >= 0) listView.SelectedItem = idx;
            }
            else
            {
                listView.SelectedItem = 0;
            }
            listView.Visible = true;

            helpLabel.Text = "↑↓: 选择 | 回车: 确认 | Esc: 取消";
        }

        private void ShowContent(bool showContent)
        {
            contentLabel.Visible = showContent;
            listView.Visible = !showContent;
            nameField.Visible = false;
        }

        private static string FormatAttributes(Attributes? attrs)
        {
            if (attrs == null) return "N/A";
            return $"  STR: {attrs.Strength}  PER: {attrs.Perception}  AGI: {attrs.Agility}  CHA: {attrs.Charisma}  INT: {attrs.Intelligence}";
        }
    }
}