namespace HHSGame.Core
{
    public sealed class PartyState
    {
        private IReadOnlyList<Player> players = Array.Empty<Player>();

        public IReadOnlyList<Player> Players => players;
        public Player? ActivePlayer { get; private set; }
        public int ActivePlayerIndex { get; private set; } = -1;

        public void SetPlayers(IReadOnlyList<Player> players, Player activePlayer)
        {
            this.players = players;
            SetActivePlayer(activePlayer);
        }

        public void SetActivePlayer(Player activePlayer)
        {
            ActivePlayer = activePlayer;
            ActivePlayerIndex = FindPlayerIndex(activePlayer);
        }

        private int FindPlayerIndex(Player player)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (ReferenceEquals(players[i], player))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
