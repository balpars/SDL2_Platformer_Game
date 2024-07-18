using SDL2;

namespace Platformer_Game
{
    public class MovementManager
    {
        private const float Gravity = 0.3f; // Yerçekimi daha yavaş
        private const float JumpSpeed = 7f; // Zıplama hızı
        private const float MoveSpeed = 2f; // Hareket hızı
        private const float RollSpeed = 2f; // Yuvarlanma hızı
        private const float SlideSpeed = 2f; // Kayma hızı
        private const float ClimbSpeed = 1f; // Tırmanma hızı

        private float jumpCooldownTimer = 0.0f; // Timer to track jump cooldown

        public void HandleInput(byte[] keyState, ref SDL.SDL_Rect rect, ref PlayerState currentState, ref bool facingLeft, ref bool isJumping, ref float jumpSpeed, float deltaTime, CollisionManager collisionManager, AnimationManager animationManager)
        {
            bool movingHorizontally = false;
            bool crouching = keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_DOWN] == 1;

            // Update the jump cooldown timer
            if (jumpCooldownTimer > 0)
            {
                jumpCooldownTimer -= deltaTime;
            }

            if (currentState == PlayerState.Climbing)
            {
                bool moved = false;

                if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_UP] == 1)
                {
                    MoveVertically(-ClimbSpeed * deltaTime * 100, ref rect);
                    moved = true;
                }
                else if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_DOWN] == 1)
                {
                    MoveVertically(ClimbSpeed * deltaTime * 100, ref rect);
                    moved = true;
                }

                if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_LEFT] == 1)
                {
                    facingLeft = true;
                    isJumping = true;
                    jumpSpeed = -JumpSpeed;
                    MoveHorizontally(-MoveSpeed * deltaTime * 100, ref rect, collisionManager);
                    currentState = PlayerState.Idle;
                    animationManager.ResetAnimation();
                    return;
                }
                else if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_RIGHT] == 1)
                {
                    facingLeft = false;
                    isJumping = true;
                    jumpSpeed = -JumpSpeed;
                    MoveHorizontally(MoveSpeed * deltaTime * 100, ref rect, collisionManager);
                    currentState = PlayerState.Idle;
                    animationManager.ResetAnimation();
                    return;
                }

                if (!moved)
                {
                    // Yukarı veya aşağı tuşlarına basılmadığında karakter olduğu yerde kalsın
                    currentState = PlayerState.Climbing;
                }

                return;
            }

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

            if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_UP] == 1 && !isJumping && jumpCooldownTimer <= 0)
            {
                isJumping = true;
                jumpSpeed = -JumpSpeed;
                currentState = PlayerState.Jumping;
                jumpCooldownTimer = 0.2f; // Set cooldown to 0.2 seconds
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

        private void MoveVertically(float amount, ref SDL.SDL_Rect rect)
        {
            rect.y += (int)amount;
        }

        public void UpdatePosition(float deltaTime, ref SDL.SDL_Rect rect, ref PlayerState currentState, ref bool isJumping, ref float jumpSpeed, bool facingLeft, CollisionManager collisionManager)
        {
            if (currentState == PlayerState.Climbing)
            {
                // Tırmanma durumunda çarpışma kontrolü yapalım
                SDL.SDL_Rect newRect = rect;
                newRect.y -= (int)(ClimbSpeed * deltaTime * 100);
                if (!collisionManager.CheckClimbingLayer(newRect))
                {
                    currentState = PlayerState.Idle;
                    facingLeft = true;
                    isJumping = true;
                    jumpSpeed = -JumpSpeed;
                }
                else
                {
                    rect.y = newRect.y;
                }
                return;
            }

            SDL.SDL_Rect newRectJumping = rect;
            newRectJumping.y += (int)(jumpSpeed * deltaTime * 100);
            jumpSpeed += Gravity * 100 * deltaTime;

            if (isJumping)
            {
                if (jumpSpeed > 0)
                {
                    currentState = PlayerState.JumpFall;
                }
            }

            if (!collisionManager.CheckCollisions(newRectJumping))
            {
                rect.y = newRectJumping.y;
            }
            else
            {
                isJumping = false;
                jumpSpeed = 0;
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
