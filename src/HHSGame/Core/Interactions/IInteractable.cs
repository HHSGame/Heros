using HHSGame.Core.Items;

namespace HHSGame.Core.Interactions
{
    /// <summary>
    /// Represents an interactive object on the map that players can examine or interact with.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Unique identifier for this interactable instance.</summary>
        string Id { get; }

        /// <summary>Display name shown in the game.</summary>
        string Name { get; }

        /// <summary>Short description shown when examining the object.</summary>
        string Description { get; }

        /// <summary>Map glyph representing the object.</summary>
        char Glyph { get; }

        /// <summary>Position on the map.</summary>
        Coordinate Position { get; }

        /// <summary>Whether this object has been interacted with already.</summary>
        bool IsInteracted { get; }

        /// <summary>Whether this object can currently be interacted with.</summary>
        bool CanInteract { get; }

        /// <summary>Examine the object to receive a description or lore.</summary>
        InteractionResult Examine(Player player);

        /// <summary>Primary interaction (open chest, read book, pull lever, etc.).</summary>
        InteractionResult Interact(Player player);
    }

    /// <summary>Result of an interaction attempt.</summary>
    public sealed record InteractionResult
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public List<Item>? ItemsGiven { get; init; }
    }
}