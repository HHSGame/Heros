using HHSGame.Core.Items;
using HHSGame.Core.SkillActions;
using HHSGame.Core.Stats;

namespace HHSGame.Core.Interactions
{
    /// <summary>
    /// A supply crate that can be opened (with lockpick or forced) to yield items.
    /// </summary>
    public sealed class SupplyCrate : IInteractable
    {
        public string Id { get; }
        public string Name { get; } = "Supply Crate";
        public string Description { get; }
        public char Glyph { get; } = '=';
        public Coordinate Position { get; }
        public bool IsInteracted { get; private set; }
        public bool CanInteract => !IsInteracted;

        private readonly List<string> lootItemIds;
        private readonly int lockDifficulty;
        private readonly GameContext context;

        public SupplyCrate(string id, Coordinate position, List<string> lootItemIds,
            int lockDifficulty, string description, GameContext context)
        {
            Id = id;
            Position = position;
            this.lootItemIds = lootItemIds;
            this.lockDifficulty = lockDifficulty;
            this.context = context;
            Description = description ?? "一个军用补给箱，上面印有占领军的标记。";
        }

        public InteractionResult Examine(Player player)
        {
            string lockStatus = IsInteracted ? "箱子已经被打开，里面空空如也。"
                : lockDifficulty > 0 ? "箱子上了锁，需要撬开。" : "箱子没有上锁。";
            return new InteractionResult
            {
                Success = true,
                Message = $"{Description} {lockStatus}"
            };
        }

        public InteractionResult Interact(Player player)
        {
            if (IsInteracted)
            {
                return new InteractionResult { Success = false, Message = "箱子已经被搜刮过了。" };
            }

            if (lockDifficulty > 0)
            {
                int skillValue = context.PartyState.GetEffectiveSkillValue(player, SkillType.Mechanics);
                SkillCheckResult check = SkillChecks.Roll(skillValue, lockDifficulty, context.Random);
                if (!check.Success)
                {
                    return new InteractionResult
                    {
                        Success = false,
                        Message = "你没能撬开锁。锁很结实。"
                    };
                }
            }

            IsInteracted = true;
            List<Item> items = [];
            foreach (string itemId in lootItemIds)
            {
                if (context.ItemCatalog.TryCreateItem(itemId, out Item item))
                {
                    items.Add(item);
                }
            }

            foreach (Item item in items)
            {
                player.AddItem(item);
            }

            string itemNames = items.Count > 0
                ? string.Join(", ", items.Select(i => i.Name))
                : "没有任何有用的东西";

            return new InteractionResult
            {
                Success = true,
                Message = $"你打开补给箱，找到了：{itemNames}。",
                ItemsGiven = items
            };
        }
    }

    /// <summary>
    /// An intelligence document or secret file that provides information when read.
    /// </summary>
    public sealed class IntelligenceDocument : IInteractable
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public char Glyph { get; } = '?';
        public Coordinate Position { get; }
        public bool IsInteracted { get; private set; }
        public bool CanInteract => true;

        private readonly string content;

        public IntelligenceDocument(string id, string name, Coordinate position, string description, string content)
        {
            Id = id;
            Name = name;
            Position = position;
            Description = description ?? $"一份{name}的文件。";
            this.content = content;
        }

        public InteractionResult Examine(Player player)
        {
            return new InteractionResult
            {
                Success = true,
                Message = Description
            };
        }

        public InteractionResult Interact(Player player)
        {
            IsInteracted = true;
            return new InteractionResult
            {
                Success = true,
                Message = $"你阅读了{Name}：\n\n\"{content}\""
            };
        }
    }

    /// <summary>
    /// A switch or mechanism that toggles a map state (e.g., opens a hidden passage).
    /// </summary>
    public sealed class Switch : IInteractable
    {
        public string Id { get; }
        public string Name { get; } = "Switch";
        public string Description { get; }
        public char Glyph { get; } = '/';
        public Coordinate Position { get; }
        public bool IsInteracted { get; private set; }
        public bool CanInteract => true;

        private readonly Coordinate targetCell;
        private readonly char toggledGlyph;
        private readonly GameContext context;
        private bool isToggled;

        public Switch(string id, Coordinate position, Coordinate targetCell, char toggledGlyph,
            string description, GameContext context)
        {
            Id = id;
            Position = position;
            this.targetCell = targetCell;
            this.toggledGlyph = toggledGlyph;
            this.context = context;
            Description = description ?? "墙上的一个老旧开关。";
        }

        public InteractionResult Examine(Player player)
        {
            string state = isToggled ? "开关处于开启状态。" : "开关处于关闭状态。";
            return new InteractionResult
            {
                Success = true,
                Message = $"{Description} {state}"
            };
        }

        public InteractionResult Interact(Player player)
        {
            isToggled = !isToggled;
            IsInteracted = true;

            if (context.MapState.IsInBounds(targetCell))
            {
                if (isToggled)
                {
                    context.MapState.SetCell(targetCell.X, targetCell.Y, new UI.Cell
                    {
                        Character = toggledGlyph,
                        Attribute = Map.TilePresets.GetTerrainColor(toggledGlyph)
                    });
                }
            }

            string action = isToggled ? "打开" : "关闭";
            return new InteractionResult
            {
                Success = true,
                Message = $"你{action}了开关。附近传来一阵机械声。"
            };
        }
    }

    /// <summary>
    /// A war memorial or monument that can provide a buff or trigger an event when interacted with.
    /// </summary>
    public sealed class Memorial : IInteractable
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public char Glyph { get; } = '&';
        public Coordinate Position { get; }
        public bool IsInteracted { get; private set; }
        public bool CanInteract => !IsInteracted;

        private readonly int sanityRestore;
        private readonly string buffMessage;
        private readonly GameContext context;

        public Memorial(string id, string name, Coordinate position, string description,
            int sanityRestore, string buffMessage, GameContext context)
        {
            Id = id;
            Name = name;
            Position = position;
            Description = description ?? $"一座{name}的纪念碑。";
            this.sanityRestore = sanityRestore;
            this.buffMessage = buffMessage;
            this.context = context;
        }

        public InteractionResult Examine(Player player)
        {
            return new InteractionResult
            {
                Success = true,
                Message = Description
            };
        }

        public InteractionResult Interact(Player player)
        {
            if (IsInteracted)
            {
                return new InteractionResult
                {
                    Success = false,
                    Message = "你已经在这里默哀过了。"
                };
            }

            IsInteracted = true;

            if (sanityRestore > 0)
            {
                player.Stats.RestoreSanity(sanityRestore);
            }

            return new InteractionResult
            {
                Success = true,
                Message = buffMessage
            };
        }
    }
}