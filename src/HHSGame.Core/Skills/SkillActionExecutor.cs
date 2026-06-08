using HHSGame.Core.Combat;
using HHSGame.Core.Enemies;
using HHSGame.Core.Interactions;
using HHSGame.Core.Items;
using HHSGame.Core.Map;
using HHSGame.Core.Stats;
using HHSGame.Core.Rendering;

namespace HHSGame.Core.SkillSystem
{
    /// <summary>
    /// Executes skill actions for a player within a game context.
    /// Extracted from Game.cs to reduce its size and isolate skill action logic.
    /// </summary>
    public sealed class SkillActionExecutor
    {
        private readonly GameContext context;

        public SkillActionExecutor(GameContext context)
        {
            this.context = context;
        }

        public void Execute(SkillActionDefinition action, SkillActionTarget target, Player player)
        {
            switch (action.Id)
            {
                case SkillActionId.Inspect:
                    ExecuteInspect(player);
                    break;
                case SkillActionId.Search:
                    ExecuteSearch(player);
                    break;
                case SkillActionId.Sneak:
                    ExecuteSneak(player);
                    break;
                case SkillActionId.Pickpocket:
                    ExecutePickpocket(player, target.Npc);
                    break;
                case SkillActionId.Inspire:
                    ExecuteInspire(player);
                    break;
                case SkillActionId.Shove:
                    ExecuteShove(player, target.Enemy);
                    break;
                case SkillActionId.Leap:
                    ExecuteLeap(player, target.Position);
                    break;
                case SkillActionId.Forage:
                    ExecuteForage(player);
                    break;
                case SkillActionId.Aim:
                    ExecuteAim(player);
                    break;
                case SkillActionId.SteadyMind:
                    ExecuteSteadyMind(player);
                    break;
                case SkillActionId.PickLock:
                    ExecutePickLock(player, target.Position);
                    break;
                case SkillActionId.Appraise:
                    ExecuteAppraise(player);
                    break;
                case SkillActionId.Demoralize:
                    ExecuteDemoralize(player, target.Enemy);
                    break;
                case SkillActionId.TreatWounds:
                    ExecuteTreatWounds(player, target.Player ?? player);
                    break;
                case SkillActionId.Bless:
                    ExecuteBless(player);
                    break;
                case SkillActionId.Examine:
                    ExecuteExamine(player, target.Position);
                    break;
                case SkillActionId.UseObject:
                    ExecuteUseObject(player, target.Position);
                    break;
                default:
                    break;
            }
        }

        public int GetEffectiveSkillValue(Player player, SkillType skill)
        {
            return context.PartyState.GetEffectiveSkillValue(player, skill);
        }

        private void ExecuteInspect(Player player)
        {
            int skillValue = GetEffectiveSkillValue(player, SkillType.Awareness);
            int radius = Math.Clamp(1 + (skillValue / 25), 1, 4);
            List<HiddenFeature> revealed = context.MapState.RevealHiddenFeatures(player.Position, radius);
            if (revealed.Count == 0)
            {
                Events.RaiseGameMessage("No hidden features detected.");
                return;
            }

            Events.RaiseGameMessage($"Discovered {revealed.Count} hidden feature(s).");
            foreach (HiddenFeature feature in revealed)
            {
                if (!string.IsNullOrWhiteSpace(feature.Description))
                {
                    Events.RaiseGameMessage(feature.Description);
                }
            }
        }

        private void ExecuteSearch(Player player)
        {
            int skillValue = GetEffectiveSkillValue(player, SkillType.Knowledge);
            int radius = Math.Clamp(1 + (skillValue / 40), 1, 2);
            List<Item> revealed = context.ItemManager.RevealHiddenItems(player.Position, radius);
            List<Item> items = context.ItemManager.GetItemsInRadius(player.Position, radius, includeHidden: false);

            if (revealed.Count == 0 && items.Count == 0)
            {
                Events.RaiseGameMessage("You find nothing of interest.");
                return;
            }

            if (revealed.Count > 0)
            {
                string foundNames = string.Join(", ", revealed.Select(item => item.Name));
                Events.RaiseGameMessage($"Revealed hidden items: {foundNames}.");
            }

            if (items.Count > 0)
            {
                string itemNames = string.Join(", ", items.Select(item => item.Name));
                Events.RaiseGameMessage($"Items nearby: {itemNames}.");
            }
        }

        private void ExecuteSneak(Player player)
        {
            player.ToggleSneak();
            Events.RaiseGameMessage(player.IsSneaking ? "You move silently." : "You stop sneaking.");
        }

        private void ExecutePickpocket(Player player, Npc? npc)
        {
            if (npc == null)
            {
                Events.RaiseGameMessage("No target to pickpocket.");
                return;
            }

            if (!IsAdjacent(player.Position, npc.Position))
            {
                Events.RaiseGameMessage("Target is out of reach.");
                return;
            }

            int skillValue = GetEffectiveSkillValue(player, SkillType.Dexterity);
            int detection = npc.Stats.Attributes.Perception
                + npc.Stats.Skills.GetValue(SkillType.Awareness, npc.Stats.Attributes);
            SkillCheckResult attempt = SkillChecks.Roll(skillValue, detection + 10, context.Random);

            if (!attempt.Success)
            {
                SkillCheckResult detected = SkillChecks.Roll(detection, skillValue + 10, context.Random);
                Events.RaiseGameMessage(detected.Success ? "You were spotted!" : "You fail to steal anything.");
                return;
            }

            if (npc.Inventory.Count == 0)
            {
                Events.RaiseGameMessage("Nothing to steal.");
                return;
            }

            int index = context.Random.Next(0, npc.Inventory.Count);
            Item stolen = npc.Inventory.ObservableItems.ElementAt(index);
            npc.Inventory.RemoveItem(stolen);
            player.AddItem(stolen);
            Events.RaiseGameMessage($"Stole {stolen.Name} from {npc.Name}.");
        }

        private void ExecuteInspire(Player player)
        {
            if (context.PartyState.InspireActive)
            {
                Events.RaiseGameMessage("The party is already inspired.");
                return;
            }

            context.PartyState.ApplyInspire(player, 1, 1);
            player.SetSkillLocked(true);
            Events.RaiseGameMessage($"{player.Name} inspires the team. Party checks +1 today.");
        }

        private void ExecuteShove(Player player, Enemy? enemy)
        {
            if (enemy == null)
            {
                Events.RaiseGameMessage("No target to shove.");
                return;
            }

            if (!IsAdjacent(player.Position, enemy.Position))
            {
                Events.RaiseGameMessage("Target is out of reach.");
                return;
            }

            int skillValue = GetEffectiveSkillValue(player, SkillType.Melee);
            int defense = enemy.Stats.Attributes.Strength
                + enemy.Stats.Skills.GetValue(SkillType.Athletics, enemy.Stats.Attributes);
            SkillCheckResult check = SkillChecks.Roll(skillValue, defense + 10, context.Random);
            if (!check.Success)
            {
                Events.RaiseGameMessage("Shove failed.");
                return;
            }

            int dx = Math.Sign(enemy.X - player.X);
            int dy = Math.Sign(enemy.Y - player.Y);
            Coordinate destination = enemy.Position.Target(dx, dy);
            if (!context.CollisionSystem.CanMoveTo(destination.X, destination.Y, enemy))
            {
                Events.RaiseGameMessage("No room to push the target.");
                return;
            }

            enemy.X = destination.X;
            enemy.Y = destination.Y;
            Events.RaiseGameMessage($"You shove {enemy.Name} back.");
        }

        private void ExecuteLeap(Player player, Coordinate? targetPosition)
        {
            if (targetPosition == null)
            {
                Events.RaiseGameMessage("No direction chosen.");
                return;
            }

            Coordinate target = targetPosition;
            int dx = target.X - player.X;
            int dy = target.Y - player.Y;
            if (!IsValidLeap(dx, dy))
            {
                Events.RaiseGameMessage("Invalid leap direction.");
                return;
            }

            int skillValue = GetEffectiveSkillValue(player, SkillType.Athletics);
            SkillCheckResult check = SkillChecks.Roll(skillValue, 18, context.Random);
            if (!check.Success)
            {
                Events.RaiseGameMessage("You fail to leap.");
                return;
            }

            Direction? direction = ToDirection(dx, dy);
            if (!direction.HasValue)
            {
                Events.RaiseGameMessage("Invalid leap direction.");
                return;
            }

            Move.Forward step = new(direction.Value);
            Coordinate start = player.Position;
            player.Move(step);
            if (player.Position.Equals(start))
            {
                Events.RaiseGameMessage("Leap blocked.");
                return;
            }

            Coordinate mid = player.Position;
            player.Move(step);
            if (player.Position.Equals(mid))
            {
                Events.RaiseGameMessage("Leap blocked.");
            }
        }

        private void ExecuteForage(Player player)
        {
            int skillValue = GetEffectiveSkillValue(player, SkillType.Survival);
            Cell cell = context.MapState.GetCell(player.X, player.Y);
            int difficulty = cell.Character is '*' or '.' ? GameConstants.Skills.LockpickDifficultyEasy : GameConstants.Skills.LockpickDifficultyHard;
            SkillCheckResult check = SkillChecks.Roll(skillValue, difficulty, context.Random);
            if (!check.Success)
            {
                Events.RaiseGameMessage("You fail to find usable supplies.");
                return;
            }

            if (!context.ItemCatalog.TryCreateItem("HealthPotion", out Item item))
            {
                Events.RaiseGameMessage("You find nothing useful.");
                return;
            }

            player.AddItem(item);
            Events.RaiseGameMessage("You forage a health potion.");
        }

        private void ExecuteAim(Player player)
        {
            if (!WeaponRules.IsRanged(player.EquippedWeapon.WeaponType))
            {
                Events.RaiseGameMessage("You need a ranged weapon to aim.");
                return;
            }

            int skillValue = GetEffectiveSkillValue(player, SkillType.Firearms);
            int bonus = Math.Max(1, skillValue / 20);
            player.SetRangedAccuracyBonus(bonus);
            Events.RaiseGameMessage($"You steady your aim (+{bonus} accuracy).");
        }

        private void ExecuteSteadyMind(Player player)
        {
            int skillValue = GetEffectiveSkillValue(player, SkillType.Resolution);
            int restore = Math.Max(1, skillValue / 10);
            int removed = player.RemoveEffects<StunEffect>();
            player.Stats.RestoreSanity(restore);

            string message = $"You regain {restore} sanity.";
            if (removed > 0)
            {
                message += " The stun wears off.";
            }
            Events.RaiseGameMessage(message);
        }

        private void ExecutePickLock(Player player, Coordinate? targetPosition)
        {
            if (targetPosition == null)
            {
                Events.RaiseGameMessage("No door selected.");
                return;
            }

            Coordinate target = targetPosition;
            if (!context.MapState.IsInBounds(target))
            {
                Events.RaiseGameMessage("No door selected.");
                return;
            }

            Cell cell = context.MapState.GetCell(target.X, target.Y);
            if (cell.Character != '+')
            {
                Events.RaiseGameMessage("That is not a locked door.");
                return;
            }

            int skillValue = GetEffectiveSkillValue(player, SkillType.Mechanics);
            SkillCheckResult check = SkillChecks.Roll(skillValue, 20, context.Random);
            if (!check.Success)
            {
                Events.RaiseGameMessage("Lockpicking failed.");
                return;
            }

            context.MapState.SetCell(target.X, target.Y, new Cell
            {
                Character = '.',
                Attribute = TilePresets.GetTerrainColor('.')
            });
            Events.RaiseGameMessage("The lock clicks open.");
        }

        private void ExecuteAppraise(Player player)
        {
            List<Item> groundItems = context.ItemManager.GetItemsAt(player.X, player.Y, includeHidden: false);
            List<Item> inventoryItems = player.Inventory.GetItems().ToList();

            if (groundItems.Count == 0 && inventoryItems.Count == 0)
            {
                Events.RaiseGameMessage("You have nothing to appraise.");
                return;
            }

            if (groundItems.Count > 0)
            {
                string groundSummary = string.Join(", ", groundItems.Select(item => $"{item.Name}({item.Value})"));
                Events.RaiseGameMessage($"On the ground: {groundSummary}.");
            }

            if (inventoryItems.Count > 0)
            {
                string inventorySummary = string.Join(", ", inventoryItems.Select(item => $"{item.Name}({item.Value})"));
                Events.RaiseGameMessage($"Inventory: {inventorySummary}.");
            }
        }

        private void ExecuteDemoralize(Player player, Enemy? enemy)
        {
            if (enemy == null)
            {
                Events.RaiseGameMessage("No target to demoralize.");
                return;
            }

            if (!IsAdjacent(player.Position, enemy.Position))
            {
                Events.RaiseGameMessage("Target is out of reach.");
                return;
            }

            int skillValue = GetEffectiveSkillValue(player, SkillType.Persuasion);
            int defense = enemy.Stats.Skills.GetValue(SkillType.Resolution, enemy.Stats.Attributes)
                + enemy.Stats.Attributes.Charisma;
            SkillCheckResult check = SkillChecks.Roll(skillValue, defense + 10, context.Random);
            if (!check.Success)
            {
                Events.RaiseGameMessage("Demoralize failed.");
                return;
            }

            enemy.SkipNextTurns(1);
            Events.RaiseGameMessage($"{enemy.Name} hesitates.");
        }

        private void ExecuteTreatWounds(Player player, Player target)
        {
            if (!IsAdjacent(player.Position, target.Position))
            {
                Events.RaiseGameMessage("Target is out of reach.");
                return;
            }

            int skillValue = GetEffectiveSkillValue(player, SkillType.Medicine);
            int heal = Math.Max(1, skillValue / 8);
            target.Heal(heal);
            Events.RaiseGameMessage($"{target.Name} recovers {heal} HP.");
        }

        private void ExecuteBless(Player player)
        {
            int skillValue = GetEffectiveSkillValue(player, SkillType.Religion);
            int restore = Math.Max(1, skillValue / 10);
            int affected = 0;
            bool removed = false;

            foreach (Player ally in context.PartyState.Players)
            {
                if (!IsAdjacent(player.Position, ally.Position))
                {
                    continue;
                }

                ally.Stats.RestoreSanity(restore);
                if (ally.RemoveEffects<PoisonEffect>() > 0)
                {
                    removed = true;
                }
                affected++;
            }

            if (affected == 0)
            {
                Events.RaiseGameMessage("No one nearby to bless.");
                return;
            }

            string message = $"Blessing restores {restore} sanity.";
            if (removed)
            {
                message += " A toxin fades.";
            }
            Events.RaiseGameMessage(message);
        }

        private static bool IsAdjacent(Coordinate origin, Coordinate target)
        {
            int dx = Math.Abs(origin.X - target.X);
            int dy = Math.Abs(origin.Y - target.Y);
            return dx <= 1 && dy <= 1;
        }

        private static bool IsValidLeap(int dx, int dy)
        {
            if (dx != 0 && dy != 0)
            {
                return false;
            }

            return Math.Abs(dx) == 2 || Math.Abs(dy) == 2;
        }

        private void ExecuteExamine(Player player, Coordinate? targetPosition)
        {
            if (targetPosition == null)
            {
                Events.RaiseGameMessage("Nothing to examine here.");
                return;
            }

            IInteractable? interactable = context.InteractableManager.GetAt(targetPosition);
            if (interactable == null)
            {
                Events.RaiseGameMessage("There is nothing noteworthy here.");
                return;
            }

            InteractionResult result = interactable.Examine(player);
            Events.RaiseGameMessage(result.Message);
        }

        private void ExecuteUseObject(Player player, Coordinate? targetPosition)
        {
            if (targetPosition == null)
            {
                Events.RaiseGameMessage("No object to interact with.");
                return;
            }

            IInteractable? interactable = context.InteractableManager.GetAt(targetPosition);
            if (interactable == null)
            {
                Events.RaiseGameMessage("There is nothing to interact with here.");
                return;
            }

            if (!interactable.CanInteract)
            {
                Events.RaiseGameMessage($"The {interactable.Name} cannot be used again.");
                return;
            }

            InteractionResult result = interactable.Interact(player);
            Events.RaiseGameMessage(result.Message);

            // If items were given, they're already added to player inventory by the interactable
        }

        private static Direction? ToDirection(int dx, int dy)
        {
            return (dx, dy) switch
            {
                (0, -2) => Direction.Up,
                (0, 2) => Direction.Down,
                (-2, 0) => Direction.Left,
                (2, 0) => Direction.Right,
                _ => null
            };
        }
    }
}