using SDL2;

namespace Platformer_Game
{
    public class MovementManager
    {
        private const float Gravity = 0.5f; // Yerçekimi daha yavaş
        private const float JumpSpeed = 12f; // Zıplama hızı
        private const float MoveSpeed = 3f; // Hareket hızı
        private const float RollSpeed = 5f; // Yuvarlanma hızı
        private const float SlideSpeed = 5f; // Kayma hızı

        public void HandleInput(byte[] keyState, ref SDL.SDL_Rect rect, ref PlayerState currentState, ref bool facingLeft, ref bool isJumping, ref float jumpSpeed, float deltaTime, CollisionManager collisionManager)
        {
            bool movingHorizontally = false;
            bool crouching = keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_DOWN] == 1;

            if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_LEFT] == 1)
            {
                facingLeft = true;
                if (currentState != PlayerState.Rolling && currentState != PlayerState.Sliding)
                {
                    MoveHorizontally(-MoveSpeed * deltaTime * 100, ref rect, collisionManager);
                    movingHorizontally = true;
                }
            }
            else if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_RIGHT] == 1)
            {
                facingLeft = false;
                if (currentState != PlayerState.Rolling && currentState != PlayerState.Sliding)
                {
                    MoveHorizontally(MoveSpeed * deltaTime * 100, ref rect, collisionManager);
                    movingHorizontally = true;
                }
            }

            if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_UP] == 1 && !isJumping)
            {
                isJumping = true;
                jumpSpeed = -JumpSpeed;
                currentState = PlayerState.Jumping;
            }

            if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_X] == 1 && currentState != PlayerState.Attacking && currentState != PlayerState.Attacking2 && currentState != PlayerState.AttackCombo)
            {
                currentState = PlayerState.Attacking;
            }

            if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_C] == 1 && currentState != PlayerState.Attacking && currentState != PlayerState.Attacking2 && currentState != PlayerState.AttackCombo)
            {
                currentState = PlayerState.Attacking2;
            }

            if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_V] == 1 && currentState != PlayerState.Attacking && currentState != PlayerState.Attacking2 && currentState != PlayerState.AttackCombo)
            {
                currentState = PlayerState.AttackCombo;
            }

            if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_B] == 1 && currentState != PlayerState.Rolling)
            {
                currentState = PlayerState.Rolling;
            }

            if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_N] == 1 && currentState != PlayerState.Sliding)
            {
                currentState = PlayerState.Sliding;
            }

            if (crouching)
            {
                if (movingHorizontally)
                {
                    currentState = PlayerState.CrouchWalking;
                }
                else
                {
                    currentState = PlayerState.Crouching;
                }
            }
            else if (isJumping)
            {
                if (jumpSpeed > 0)
                {
                    currentState = PlayerState.JumpFall;
                }
            }
            else if (currentState != PlayerState.Attacking && currentState != PlayerState.Attacking2 && currentState != PlayerState.AttackCombo && currentState != PlayerState.Rolling && currentState != PlayerState.Sliding)
            {
                if (movingHorizontally)
                {
                    currentState = PlayerState.Running;
                }
                else
                {
                    currentState = PlayerState.Idle;
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

        public void UpdatePosition(float deltaTime, ref SDL.SDL_Rect rect, ref PlayerState currentState, ref bool isJumping, ref float jumpSpeed, bool facingLeft, CollisionManager collisionManager)
        {
            if (isJumping)
            {
                SDL.SDL_Rect newRect = rect;
                newRect.y += (int)(jumpSpeed * deltaTime * 100);
                jumpSpeed += Gravity * 100 * deltaTime;

                if (jumpSpeed > 0)
                {
                    currentState = PlayerState.JumpFall;
                }

                if (!collisionManager.CheckCollisions(newRect))
                {
                    rect.y = newRect.y;
                }
                else
                {
                    isJumping = false;
                    jumpSpeed = 0;
                    currentState = PlayerState.Idle;
                }
            }

            if (currentState == PlayerState.Rolling)
            {
                MoveHorizontally(facingLeft ? -RollSpeed * deltaTime * 100 : RollSpeed * deltaTime * 100, ref rect, collisionManager);
            }

            if (currentState == PlayerState.Sliding)
            {
                MoveHorizontally(facingLeft ? -SlideSpeed * deltaTime * 100 : SlideSpeed * deltaTime * 100, ref rect, collisionManager);
            }
        }
    }
}
