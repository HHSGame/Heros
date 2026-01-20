using HHSGame.Core;
using HHSGame.Core.Combat;
using HHSGame.Core.Items;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Terminal.Gui.Input;
using Terminal.Gui.Drivers;

namespace HHSGame.UI.Views
{
    public class UtilityWindow : Window
    {
        private readonly InventoryManager inventoryManager;
        private readonly Game game;
        private readonly Lazy<Player> playerHolder;
        private readonly ListView listView;
        private readonly MapFrame mapWindow;

        public UtilityWindow(InventoryManager inventoryManager, GameContext context, MapFrame mapWindow, Game game)
        {
            Title = GUISettings.UtilityWindowTitle;
            X = Pos.Right(mapWindow);
            Y = 0;
            Height = Dim.Fill()! - (GUISettings.MessageWindowHeight + GUISettings.StatusBarHeight);
            Visible = false;
            playerHolder = new(() => context.Player);
            this.inventoryManager = inventoryManager;
            this.mapWindow = mapWindow;
            this.game = game;

            listView = new ListView
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                AllowsMarking = false,
                AllowsMultipleSelection = false
            };

            listView.SetSource(inventoryManager.GetItems());

            Add(listView);

            listView.OpenSelectedItem += HandleOpenSelectedItem;

            Events.OnInventoryChange += HandleInventoryChange;
        }

        private void HandleOpenSelectedItem(object? sender, ListViewItemEventArgs args)
        {
            int index = args.Item;
            Item item = inventoryManager.ObservableItems.ElementAt(index);

            if (game.IsCombatActive())
            {
                Player player = playerHolder.Value;
                bool queued = game.TryQueuePlayerAction(
                    $"Use {item.Name}",
                    ActionCosts.Inventory,
                    () =>
                    {
                        item.Use(player);
                        inventoryManager.RemoveItem(item);
                    });

                if (!queued)
                {
                    Events.RaiseGameMessage("Not enough AP to queue item use.");
                }

                return;
            }

            game.PerformPlayerAction(() =>
            {
                item.Use(playerHolder.Value);
                inventoryManager.RemoveItem(item);
            }, ActionCosts.Inventory, false, GameStateType.Inventory);
        }

        private void HandleInventoryChange(object? sender, InventoryChangeEventArgs e)
        {
            listView.SetSource(inventoryManager.GetItems());
        }

        public void ToggleUtilityWindow()
        {
            if (!Visible)
            {
                Visible = true;
                mapWindow.Width = Dim.Percent(66);
                mapWindow.SetNeedsDraw();
                Width = Dim.Percent(34);
                SetNeedsDraw();
            }
            else
            {
                Visible = false;
                mapWindow.Width = Dim.Fill()! - 20;
                mapWindow.SetNeedsDraw();
                SetNeedsDraw();
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
                case KeyCode.Esc:
                    ToggleUtilityWindow();
                    return true;
                case KeyCode.Enter:
                    return listView.OnOpenSelectedItem();
                default:
                    break;
            }
            return false;
        }
    }
}
