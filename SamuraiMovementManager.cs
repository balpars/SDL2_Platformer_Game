using SDL2;

namespace Platformer_Game
{
    public class SamuraiMovementManager
    {
        private const float RunSpeed = 2f; // Speed of the samurai when running
        private const float Gravity = 0.3f; // Gravity

        public void MoveSamurai(float deltaTime, ref SDL.SDL_Rect rect, SamuraiState currentState)
        {
            if (currentState == SamuraiState.Running)
            {
                float movementAmount = RunSpeed * deltaTime;
                rect.x -= (int)movementAmount; // Move left while running
            }
        }

        public void UpdatePosition(float deltaTime, ref SDL.SDL_Rect rect, ref SamuraiState currentState, CollisionManager collisionManager)
        {
            // Implement any additional logic here for updating the position
            // For example, handle gravity or other state-specific movements
            if (currentState == SamuraiState.Running)
            {
                // Apply gravity if necessary (example logic)
                SDL.SDL_Rect newRect = rect;
                newRect.y += (int)(Gravity * 100 * deltaTime);

                if (!collisionManager.CheckCollisions(newRect))
                {
                    rect.y = newRect.y;
                }
                else
                {
                    // Handle collision logic (e.g., stop falling)
                    currentState = SamuraiState.Idle;
                }
            }
        }
    }
}
