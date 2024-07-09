namespace Platformer_Game
{
    class RunningState : IPlayerState
    {
        private Player player;

        public RunningState(Player player)
        {
            this.player = player;
        }

        public void HandleInput()
        {
            // Handle input for running state
        }

        public void Update()
        {
            // Update logic for running state
        }
    }
}
