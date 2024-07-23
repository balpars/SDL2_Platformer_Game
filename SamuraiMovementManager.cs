using SDL2;
using System.Numerics;

namespace Platformer_Game
{
    public class SamuraiMovementManager
    {
        private const float Gravity = 0.3f;
        private const float JumpSpeed = 6f;
        private const float MoveSpeed = 1.5f;
        private float verticalSpeed = 0;
        private bool isJumping = false;

        public void MoveSamurai(float deltaTime, ref SDL.SDL_Rect rect, Vector2 playerPosition, ref bool facingLeft, CollisionManager collisionManager)
        {
            float horizontalMovement = MoveSpeed * deltaTime * 100;

            if (rect.x < playerPosition.X)
            {
                MoveHorizontally(horizontalMovement, ref rect, collisionManager);
                facingLeft = false;
            }
            else if (rect.x > playerPosition.X)
            {
                MoveHorizontally(-horizontalMovement, ref rect, collisionManager);
                facingLeft = true;
            }

            if (rect.y > playerPosition.Y+5 && !isJumping)
            {
                isJumping = true;
                verticalSpeed = -JumpSpeed;
            }
        }

        public void UpdatePosition(float deltaTime, ref SDL.SDL_Rect rect, CollisionManager collisionManager)
        {
            SDL.SDL_Rect newRect = rect;

            if (isJumping)
            {
                verticalSpeed += Gravity * 100 * deltaTime;
                newRect.y += (int)(verticalSpeed * deltaTime * 100);

                if (collisionManager.CheckCollisions(newRect))
                {
                    isJumping = false;
                    verticalSpeed = 0;
                }
                else
                {
                    rect.y = newRect.y;
                }
            }
            else
            {
                verticalSpeed += Gravity * 100 * deltaTime;
                newRect.y += (int)(verticalSpeed * deltaTime * 100);

                if (collisionManager.CheckCollisions(newRect))
                {
                    verticalSpeed = 0;
                }
                else
                {
                    rect.y = newRect.y;
                }
            }
        }

        private void MoveHorizontally(float amount, ref SDL.SDL_Rect rect, CollisionManager collisionManager)
        {
            SDL.SDL_Rect newRect = rect;
            newRect.x += (int)amount;

            if (!collisionManager.CheckCollisions(newRect))
            {
                rect.x = newRect.x;
            }
        }
    }
}
