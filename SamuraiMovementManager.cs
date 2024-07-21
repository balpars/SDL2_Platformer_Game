using SDL2;
using System.Numerics;

namespace Platformer_Game
{
    public class SamuraiMovementManager
    {
        private const float RunSpeed = 150f; // Speed of the samurai when running
        private const float Gravity = 0.3f; // Gravity

        public void MoveSamurai(float deltaTime, ref SDL.SDL_Rect rect, Vector2 playerPosition, ref bool facingLeft)
        {
            float movementAmount = RunSpeed * deltaTime;

            if (rect.x < playerPosition.X)
            {
                rect.x += (int)movementAmount; // Move right towards the player
                facingLeft = false;
            }
            else if (rect.x > playerPosition.X)
            {
                rect.x -= (int)movementAmount; // Move left towards the player
                facingLeft = true;
            }
        }

        public void UpdatePosition(float deltaTime, ref SDL.SDL_Rect rect, CollisionManager collisionManager)
        {
            // Apply gravity if necessary (example logic)
            SDL.SDL_Rect newRect = rect;
            newRect.y += (int)(Gravity * 100 * deltaTime);

            if (!collisionManager.CheckCollisions(newRect))
            {
                rect.y = newRect.y;
            }
        }
    }
}
