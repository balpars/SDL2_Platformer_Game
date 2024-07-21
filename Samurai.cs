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
        private SamuraiMovementManager movementManager; // Add movement manager
        private IntPtr renderer;
        private bool animationEnded;
        private SoundManager soundManager;
        private int health; // Add health field

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
            movementManager = new SamuraiMovementManager(); // Initialize movement manager
            facingLeft = true;
            currentState = SamuraiState.Running; // Start in running state
            animationEnded = false;
            this.soundManager = soundManager;
            health = 100; // Initialize health
        }

        public void TakeDamage(int amount)
        {
            health -= amount;
            Console.WriteLine($"Samurai took {amount} damage, health is now {health}");
            if (health <= 0)
            {
                // Handle samurai death (e.g., play death animation, remove from game, etc.)
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

            // Always run towards the player
            currentState = SamuraiState.Running;

            movementManager.MoveSamurai(deltaTime, ref rect, player.Position, ref facingLeft);
            movementManager.UpdatePosition(deltaTime, ref rect, collisionManager);

            if (currentState == SamuraiState.Attacking)
            {
                SDL.SDL_Rect attackRect = GetAttackRect();
                SDL.SDL_Rect playerRect = player.Rect;
                if (SDL.SDL_HasIntersection(ref attackRect, ref playerRect) == SDL.SDL_bool.SDL_TRUE)
                {
                    player.TakeDamage(1); // Adjust the damage amount as needed
                }
            }

            if (animationEnded)
            {
                animationManager.ResetAnimation();
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
            const int MaxHealth = 100; // Maximum health
            int healthBarWidth = rect.w; // Width of the health bar
            int healthBarHeight = 4; // Height of the health bar
            int healthBarX = rect.x; // X position of the health bar
            int healthBarY = rect.y - healthBarHeight - 2; // Y position of the health bar

            // Clamp health to ensure it's between 0 and MaxHealth
            int clampedHealth = Math.Max(0, Math.Min(health, MaxHealth));

            // Create health bar background rectangle
            SDL.SDL_Rect healthBarBackground = new SDL.SDL_Rect
            {
                x = healthBarX,
                y = healthBarY,
                w = healthBarWidth,
                h = healthBarHeight
            };

            // Create health bar foreground rectangle based on current health
            SDL.SDL_Rect healthBarForeground = new SDL.SDL_Rect
            {
                x = healthBarX,
                y = healthBarY,
                w = (int)(healthBarWidth * (clampedHealth / (float)MaxHealth)),
                h = healthBarHeight
            };

            // Get render rectangles for camera
            healthBarBackground = camera.GetRenderRect(healthBarBackground);
            healthBarForeground = camera.GetRenderRect(healthBarForeground);

            // Render health bar background
            SDL.SDL_SetRenderDrawColor(renderer, 255, 0, 0, 255); // Red for the background
            SDL.SDL_RenderFillRect(renderer, ref healthBarBackground);

            // Render health bar foreground
            SDL.SDL_SetRenderDrawColor(renderer, 0, 255, 0, 255); // Green for the foreground
            SDL.SDL_RenderFillRect(renderer, ref healthBarForeground);
        }

        public SDL.SDL_Rect GetAttackRect()
        {
            int attackWidth = 30; // Width of the attack area
            int attackHeight = 40; // Height of the attack area
            int offsetX = facingLeft ? -attackWidth : rect.w; // Offset for attack direction

            return new SDL.SDL_Rect
            {
                x = rect.x + offsetX,
                y = rect.y,
                w = attackWidth,
                h = attackHeight
            };
        }
    }

    public enum SamuraiState
    {
        Idle,
        Running,
        Attacking
    }
}
