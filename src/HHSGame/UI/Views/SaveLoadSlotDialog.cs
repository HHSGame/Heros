using System.Collections.ObjectModel;
using HHSGame.Core.Save;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Terminal.Gui.Input;
using Terminal.Gui.Drivers;

namespace HHSGame.UI.Views
{
    /// <summary>
    /// Modal dialog for save/load slot selection.
    /// Displays 3 slots with save info. Supports Enter to confirm, D to delete, Esc to cancel.
    /// </summary>
    public class SaveLoadSlotDialog : Window
    {
        private enum DialogMode { Save, Load }

        private readonly SaveManager saveManager;
        private readonly LoadManager loadManager;
        private readonly ListView listView;
        private readonly ObservableCollection<string> slotLabels = [];
        private SaveSlotInfo[] slots;
        private DialogMode mode;

        public event EventHandler<int>? SlotSelected;   // fires with slot number (1-based)
        public event EventHandler<int>? SlotDeleted;    // fires with slot number (1-based)
        public event EventHandler? Cancelled;

        public SaveLoadSlotDialog(SaveManager saveManager, LoadManager loadManager, MapFrame mapWindow)
        {
            this.saveManager = saveManager;
            this.loadManager = loadManager;

            Title = "Save Game";
            X = Pos.Center();
            Y = Pos.Center();
            Width = 60;
            Height = 14;
            Visible = false;

            Label helpLabel = new()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Text = "Enter: Select | D: Delete | Esc: Cancel"
            };

            listView = new ListView
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill() - 1,
                AllowsMarking = false,
                AllowsMultipleSelection = false
            };

            Add(helpLabel, listView);

            slots = [];
        }

        public void ShowForSave()
        {
            mode = DialogMode.Save;
            Title = "Save Game";
            RefreshSlots();
            Visible = true;
            SetFocus();
        }

        public void ShowForLoad()
        {
            mode = DialogMode.Load;
            Title = "Load Game";
            RefreshSlots();
            Visible = true;
            SetFocus();
        }

        private void RefreshSlots()
        {
            slots = saveManager.ListSlots();
            List<string> display = [];
            foreach (SaveSlotInfo slot in slots)
            {
                if (slot.IsOccupied)
                {
                    string time = slot.SaveTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                    display.Add($"Slot {slot.SlotNumber}: {slot.PlayerName} Lv{slot.PlayerLevel} | Turn {slot.TurnNumber} | {time}");
                }
                else
                {
                    display.Add($"Slot {slot.SlotNumber}: --- Empty ---");
                }
            }
            slotLabels.Clear();
            foreach (string line in display)
            {
                slotLabels.Add(line);
            }
            if (slots.Length > 0)
            {
                listView.SelectedItem = 0;
            }
        }

        public bool HandleKeyEvent(Key key)
        {
            switch (key.KeyCode)
            {
                case KeyCode.CursorUp:
                    listView.MoveUp();
                    return true;
                case KeyCode.CursorDown:
                    listView.MoveDown();
                    return true;
                case KeyCode.Enter:
                    ConfirmSelection();
                    return true;
                case KeyCode.D:
                    DeleteSelectedSlot();
                    return true;
                case KeyCode.Esc:
                    Cancel();
                    return true;
                default:
                    return false;
            }
        }

        private void ConfirmSelection()
        {
            int index = listView.SelectedItem;
            if (index < 0 || index >= slots.Length)
            {
                return;
            }

            SaveSlotInfo slot = slots[index];

            if (mode == DialogMode.Load && !slot.IsOccupied)
            {
                // Cannot load an empty slot
                return;
            }

            Visible = false;
            SlotSelected?.Invoke(this, slot.SlotNumber);
        }

        private void DeleteSelectedSlot()
        {
            int index = listView.SelectedItem;
            if (index < 0 || index >= slots.Length)
            {
                return;
            }

            SaveSlotInfo slot = slots[index];
            if (!slot.IsOccupied)
            {
                return;
            }

            bool deleted = loadManager.DeleteSlot(slot.SlotNumber);
            if (deleted)
            {
                RefreshSlots();
                SlotDeleted?.Invoke(this, slot.SlotNumber);
            }
        }

        private void Cancel()
        {
            Visible = false;
            Cancelled?.Invoke(this, EventArgs.Empty);
        }
    }
}