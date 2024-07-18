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

        public Vector2 Position => new Vector2(rect.x, rect.y);

        public SDL.SDL_Rect Rect
        {
            get => rect;
            internal set => rect = value;
        }

        private const int SamuraiWidth = 20;
        private const int SamuraiHeight = 35;

        private float stateTimer; // Timer to handle state transitions

        public Samurai(int x, int y, int width, int height, IntPtr renderer, SoundManager soundManager)
        {
            rect = new SDL.SDL_Rect { x = x, y = y, w = SamuraiWidth, h = SamuraiHeight };
            this.renderer = renderer;
            animationManager = new SamuraiAnimationManager();
            movementManager = new SamuraiMovementManager(); // Initialize movement manager
            facingLeft = true;
            currentState = SamuraiState.Idle;
            animationEnded = false;
            this.soundManager = soundManager;
            stateTimer = 0f;
        }

        public void LoadContent()
        {
            animationManager.LoadContent(renderer);
        }

        public void Update(float deltaTime, CollisionManager collisionManager)
        {
            animationEnded = false;
            animationManager.UpdateAnimation(currentState, deltaTime, ref animationEnded);

            stateTimer += deltaTime;

            // Handle state transitions
            if (stateTimer <= 3.0f)
            {
                currentState = SamuraiState.Idle;
            }
            else if (stateTimer > 3.0f && stateTimer <= 5.0f)
            {
                currentState = SamuraiState.Running;
            }
            else if (stateTimer > 5.0f && stateTimer <= 10.0f)
            {
                currentState = SamuraiState.Attacking;
            }
            else
            {
                currentState = SamuraiState.Idle;
                stateTimer = 0f; // Reset the state timer
            }

            movementManager.MoveSamurai(deltaTime, ref rect, currentState);
            movementManager.UpdatePosition(deltaTime, ref rect, ref currentState, collisionManager);

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
        }

        public void RenderDebug(IntPtr renderer, Camera camera)
        {
            SDL.SDL_SetRenderDrawColor(renderer, 255, 0, 0, 255);

            SDL.SDL_Rect renderRect = camera.GetRenderRect(rect);
            SDL.SDL_RenderDrawRect(renderer, ref renderRect);
        }
    }

    public enum SamuraiState
    {
        Idle,
        Running,
        Attacking
    }
}
