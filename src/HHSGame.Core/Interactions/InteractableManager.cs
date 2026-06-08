namespace HHSGame.Core.Interactions
{
    /// <summary>
    /// Manages all interactive objects on the map.
    /// </summary>
    public sealed class InteractableManager
    {
        private readonly List<IInteractable> interactables = [];

        public IReadOnlyList<IInteractable> All => interactables;

        public void Add(IInteractable interactable)
        {
            interactables.Add(interactable);
        }

        public void Clear()
        {
            interactables.Clear();
        }

        /// <summary>
        /// Gets the interactable at a specific position, or null if none exists.
        /// </summary>
        public IInteractable? GetAt(Coordinate position)
        {
            return interactables.FirstOrDefault(i => i.Position.Equals(position));
        }

        /// <summary>
        /// Gets all interactables within a radius of the given position.
        /// </summary>
        public List<IInteractable> GetNearby(Coordinate position, int radius)
        {
            List<IInteractable> result = [];
            foreach (IInteractable interactable in interactables)
            {
                int dx = Math.Abs(interactable.Position.X - position.X);
                int dy = Math.Abs(interactable.Position.Y - position.Y);
                if (dx <= radius && dy <= radius)
                {
                    result.Add(interactable);
                }
            }
            return result;
        }

        /// <summary>
        /// Gets interactables that can be interacted with at the player's position or adjacent.
        /// </summary>
        public List<IInteractable> GetInteractableNearPlayer(Coordinate playerPosition)
        {
            List<IInteractable> result = [];
            foreach (IInteractable interactable in interactables)
            {
                int dx = Math.Abs(interactable.Position.X - playerPosition.X);
                int dy = Math.Abs(interactable.Position.Y - playerPosition.Y);
                if (dx <= 1 && dy <= 1 && interactable.CanInteract)
                {
                    result.Add(interactable);
                }
            }
            return result;
        }

        /// <summary>
        /// Attempts to examine the nearest interactable to the player.
        /// </summary>
        public InteractionResult? ExamineAt(Coordinate position, Player player)
        {
            IInteractable? interactable = GetAt(position);
            return interactable?.Examine(player);
        }

        /// <summary>
        /// Attempts to interact with the nearest interactable to the player.
        /// </summary>
        public InteractionResult? InteractAt(Coordinate position, Player player)
        {
            IInteractable? interactable = GetAt(position);
            if (interactable == null)
            {
                return null;
            }

            InteractionResult result = interactable.Interact(player);
            return result;
        }
    }
}