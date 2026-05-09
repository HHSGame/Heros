using HHSGame.Core.Items;
using HHSGame.Core.SkillActions;
using HHSGame.Core.Stats;

namespace HHSGame.Core.Interactions
{
    /// <summary>
    /// A treasure chest that can be opened (with lockpick or forced) to yield items.
    /// </summary>
    public sealed class TreasureChest : IInteractable
    {
        public string Id { get; }
        public string Name { get; } = "Treasure Chest";
        public string Description { get; }
        public char Glyph { get; } = '=';
        public Coordinate Position { get; }
        public bool IsInteracted { get; private set; }
        public bool CanInteract => !IsInteracted;

        private readonly List<string> lootItemIds;
        private readonly int lockDifficulty;
        private readonly GameContext context;

        public TreasureChest(string id, Coordinate position, List<string> lootItemIds,
            int lockDifficulty, string description, GameContext context)
        {
            Id = id;
            Position = position;
            this.lootItemIds = lootItemIds;
            this.lockDifficulty = lockDifficulty;
            this.context = context;
            Description = description ?? "A sturdy wooden chest with iron fittings.";
        }

        public InteractionResult Examine(Player player)
        {
            string lockStatus = IsInteracted ? "The lock is broken and the chest is empty."
                : lockDifficulty > 0 ? "The chest has a sturdy lock." : "The chest appears unlocked.";
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
                return new InteractionResult { Success = false, Message = "The chest is already empty." };
            }

            // Try lockpicking if locked
            if (lockDifficulty > 0)
            {
                int skillValue = context.PartyState.GetEffectiveSkillValue(player, SkillType.Mechanics);
                SkillCheckResult check = SkillChecks.Roll(skillValue, lockDifficulty, context.Random);
                if (!check.Success)
                {
                    return new InteractionResult
                    {
                        Success = false,
                        Message = "You fail to open the chest. The lock holds firm."
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
                : "nothing of value";

            return new InteractionResult
            {
                Success = true,
                Message = $"You open the chest and find: {itemNames}.",
                ItemsGiven = items
            };
        }
    }

    /// <summary>
    /// A book, scroll, or inscription that provides lore when read.
    /// </summary>
    public sealed class BookInscription : IInteractable
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public char Glyph { get; } = '?';
        public Coordinate Position { get; }
        public bool IsInteracted { get; private set; }
        public bool CanInteract => true; // Can always read

        private readonly string loreText;

        public BookInscription(string id, string name, Coordinate position, string description, string loreText)
        {
            Id = id;
            Name = name;
            Position = position;
            Description = description ?? $"A weathered {name.ToLowerInvariant()}.";
            this.loreText = loreText;
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
                Message = $"You read the {Name.ToLowerInvariant()}:\n\n\"{loreText}\""
            };
        }
    }

    /// <summary>
    /// A lever or switch that toggles a map state (e.g., opens a hidden passage).
    /// </summary>
    public sealed class Lever : IInteractable
    {
        public string Id { get; }
        public string Name { get; } = "Lever";
        public string Description { get; }
        public char Glyph { get; } = '/';
        public Coordinate Position { get; }
        public bool IsInteracted { get; private set; }
        public bool CanInteract => true; // Can always toggle

        private readonly Coordinate targetCell;
        private readonly char toggledGlyph;
        private readonly GameContext context;
        private bool isToggled;

        public Lever(string id, Coordinate position, Coordinate targetCell, char toggledGlyph,
            string description, GameContext context)
        {
            Id = id;
            Position = position;
            this.targetCell = targetCell;
            this.toggledGlyph = toggledGlyph;
            this.context = context;
            Description = description ?? "A rusty lever mounted on the wall.";
        }

        public InteractionResult Examine(Player player)
        {
            string state = isToggled ? "It is currently pulled." : "It is in the default position.";
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

            string action = isToggled ? "pull" : "release";
            return new InteractionResult
            {
                Success = true,
                Message = $"You {action} the lever. You hear a mechanical grinding sound nearby."
            };
        }
    }

    /// <summary>
    /// A statue that can provide a buff or trigger an event when interacted with.
    /// </summary>
    public sealed class Statue : IInteractable
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

        public Statue(string id, string name, Coordinate position, string description,
            int sanityRestore, string buffMessage, GameContext context)
        {
            Id = id;
            Name = name;
            Position = position;
            Description = description ?? $"A weathered statue of {name}.";
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
                    Message = "The statue has already bestowed its blessing."
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