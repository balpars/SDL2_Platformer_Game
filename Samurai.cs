using SDL2;
using System.Numerics;
using System;

namespace Platformer_Game
{
    public class Samurai : ITarget
    {
        private SDL.SDL_Rect rect;
        private bool facingLeft;
        private SamuraiState currentState;
        private SamuraiAnimationManager animationManager;
        private SamuraiMovementManager movementManager; 
        private IntPtr renderer;
        private bool animationEnded;
        private SoundManager soundManager;
        private int health;  

        public Vector2 Position => new Vector2(rect.x, rect.y);

        public SDL.SDL_Rect Rect
        {
            get => rect;
            internal set => rect = value;
        }

        private const int SamuraiWidth = 20;
        private const int SamuraiHeight = 35;

        public Samurai(int x, int y, int width, int height, IntPtr renderer, SoundManager soundManager)
        {
            rect = new SDL.SDL_Rect { x = x, y = y, w = SamuraiWidth, h = SamuraiHeight };
            this.renderer = renderer;
            animationManager = new SamuraiAnimationManager();
            movementManager = new SamuraiMovementManager(); 
            facingLeft = true;
            currentState = SamuraiState.Running; 
            animationEnded = false;
            this.soundManager = soundManager;
            health = 100; 
        }

        public void TakeDamage(int amount)
        {
            health -= amount;
            Console.WriteLine($"Samurai took {amount} damage, health is now {health}");
            if (health <= 0)
            {
                Console.WriteLine("Samurai is dead!");
            }
        }

        public void LoadContent()
        {
            animationManager.LoadContent(renderer);
        }

        public void Update(float deltaTime, CollisionManager collisionManager, Player player)
        {
            animationEnded = false;
            animationManager.UpdateAnimation(currentState, deltaTime, ref animationEnded);

            float distanceToPlayer = Vector2.Distance(Position, player.Position);

            if (distanceToPlayer <= 40)
            {
                if (Position.X < player.Position.X)
                {
                    facingLeft = false;
                }
                else
                {
                    facingLeft = true;
                }

                currentState = SamuraiState.Attacking;
            }
            else
            {
                currentState = SamuraiState.Running;
                movementManager.MoveSamurai(deltaTime, ref rect, player.Position, ref facingLeft, collisionManager);
            }

            movementManager.UpdatePosition(deltaTime, ref rect, collisionManager);

            if (currentState == SamuraiState.Attacking)
            {
                SDL.SDL_Rect attackRect = GetAttackRect();
                SDL.SDL_Rect playerRect = player.Rect;
                if (SDL.SDL_HasIntersection(ref attackRect, ref playerRect) == SDL.SDL_bool.SDL_TRUE)
                {
                    player.TakeDamage(1); 
                }
            }

            if (animationEnded)
            {
                animationManager.ResetAnimation();
                currentState = SamuraiState.Running; 
            }
        }

        public void Render(Camera camera)
        {
            IntPtr texture = animationManager.GetCurrentTexture(currentState);
            SDL.SDL_Rect srcRect = animationManager.GetSourceRect();
            SDL.SDL_Rect dstRect = animationManager.GetDestinationRect(rect);

            dstRect = camera.GetRenderRect(dstRect);

            SDL.SDL_RendererFlip flip = facingLeft ? SDL.SDL_RendererFlip.SDL_FLIP_HORIZONTAL : SDL.SDL_RendererFlip.SDL_FLIP_NONE;
            SDL.SDL_RenderCopyEx(renderer, texture, ref srcRect, ref dstRect, 0, IntPtr.Zero, flip);

            RenderHealthBar(camera);
        }

        public void RenderDebug(IntPtr renderer, Camera camera)
        {
            SDL.SDL_SetRenderDrawColor(renderer, 255, 0, 0, 255);

            SDL.SDL_Rect renderRect = camera.GetRenderRect(rect);
            SDL.SDL_RenderDrawRect(renderer, ref renderRect);
        }

        private void RenderHealthBar(Camera camera)
        {
            // Constants
            const int MaxHealth = 100; 
            int healthBarWidth = rect.w; 
            int healthBarHeight = 4; 
            int healthBarX = rect.x; 
            int healthBarY = rect.y - healthBarHeight - 2; 

            int clampedHealth = Math.Max(0, Math.Min(health, MaxHealth));

            SDL.SDL_Rect healthBarBackground = new SDL.SDL_Rect
            {
                x = healthBarX,
                y = healthBarY,
                w = healthBarWidth,
                h = healthBarHeight
            };

            SDL.SDL_Rect healthBarForeground = new SDL.SDL_Rect
            {
                x = healthBarX,
                y = healthBarY,
                w = (int)(healthBarWidth * (clampedHealth / (float)MaxHealth)),
                h = healthBarHeight
            };

            healthBarBackground = camera.GetRenderRect(healthBarBackground);
            healthBarForeground = camera.GetRenderRect(healthBarForeground);

            SDL.SDL_SetRenderDrawColor(renderer, 255, 0, 0, 255); 
            SDL.SDL_RenderFillRect(renderer, ref healthBarBackground);

            SDL.SDL_SetRenderDrawColor(renderer, 0, 255, 0, 255); 
            SDL.SDL_RenderFillRect(renderer, ref healthBarForeground);
        }

        public SDL.SDL_Rect GetAttackRect()
        {
            int attackWidth = 30; 
            int attackHeight = 40; 
            int offsetX = facingLeft ? -attackWidth : rect.w; 

            return new SDL.SDL_Rect
            {
                x = rect.x + offsetX,
                y = rect.y,
                w = attackWidth,
                h = attackHeight
            };
        }
        public void Reset(int x, int y)
        {
            rect.x = x;
            rect.y = y;
            facingLeft = true;
            currentState = SamuraiState.Running;
            animationEnded = false;
            health = 100; 
        }

    }

    public enum SamuraiState
    {
        Idle,
        Running,
        Attacking
    }
}
