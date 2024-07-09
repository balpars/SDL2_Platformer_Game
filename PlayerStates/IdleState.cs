namespace Platformer_Game
{
    class IdleState : IPlayerState
    {
        private Player player;

        public IdleState(Player player)
        {
            this.player = player;
        }

        public void HandleInput()
        {
            // Handle input for idle state
        }

        public void Update()
        {
            // Update logic for idle state
        }
    }
}
