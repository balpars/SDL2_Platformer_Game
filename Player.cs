// Player.cs
using SDL2;
using System.Numerics;

namespace Platformer_Game
{
    public class Player : ITarget
    {
        private SDL.SDL_Rect rect;
        private bool facingLeft;
        private PlayerState currentState;
        private AnimationManager animationManager;
        private MovementManager movementManager;
        private IntPtr renderer;
        private bool animationEnded;
        private bool isJumping;
        private float jumpSpeed;
        private SoundManager soundManager;

        public Vector2 Position => new Vector2(rect.x, rect.y);

        public SDL.SDL_Rect Rect
        {
            get => rect;
            internal set => rect = value;
        }

        // Karakterin boyutlarını burada tanımlayın
        private const int CharacterWidth = 20; // Güncellenen genişlik
        private const int CharacterHeight = 40; // Güncellenen yükseklik

        public Player(int x, int y, int width, int height, IntPtr renderer, SoundManager soundManager)
        {
            rect = new SDL.SDL_Rect { x = x, y = y, w = CharacterWidth, h = CharacterHeight }; // Güncellenen boyutlar
            this.renderer = renderer;
            animationManager = new AnimationManager();
            movementManager = new MovementManager();
            facingLeft = false;
            currentState = PlayerState.Idle;
            animationEnded = false;
            isJumping = false;
            jumpSpeed = 0f;
            this.soundManager = soundManager;
        }

        public void LoadContent()
        {
            animationManager.LoadContent(renderer);
        }

        public void Update(float deltaTime, byte[] keyState, CollisionManager collisionManager, TileLoader tileLoader)
        {
            animationEnded = false;
            animationManager.UpdateAnimation(currentState, deltaTime, ref animationEnded);

            var previousState = currentState;

            // Always handle movement input
            movementManager.HandleInput(keyState, ref rect, ref currentState, ref facingLeft, ref isJumping, ref jumpSpeed, deltaTime, collisionManager, animationManager);
            movementManager.UpdatePosition(deltaTime, ref rect, ref currentState, ref isJumping, ref jumpSpeed, facingLeft, collisionManager);

            // Handle special state transitions only if the current animation has ended
            if (animationEnded)
            {
                HandleSpecialStateTransitions(keyState);
            }

            // If state changes, reset animation
            if (previousState != currentState)
            {
                animationManager.ResetAnimation();
            }

            // Print the current state to the console for debugging
            //Console.WriteLine($"Current State: {currentState}");

            // Play sound based on state changes
            HandleSoundEffects(keyState, previousState);

            // Check collision with coins
            CheckCoinCollision(tileLoader);

            // ClimbingLayer kontrolü
            if (collisionManager.CheckClimbingLayer(rect))
            {
                currentState = PlayerState.Climbing;
            }
        }

        private void HandleSpecialStateTransitions(byte[] keyState)
        {
            if (animationEnded && (currentState == PlayerState.Attacking || currentState == PlayerState.Attacking2 || currentState == PlayerState.AttackCombo || currentState == PlayerState.Rolling || currentState == PlayerState.Sliding))
            {
                currentState = PlayerState.Idle;
            }

            if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_X] == 1 && currentState != PlayerState.Attacking)
            {
                currentState = PlayerState.Attacking;
            }
            else if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_C] == 1 && currentState != PlayerState.Attacking2)
            {
                currentState = PlayerState.Attacking2;
            }
            else if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_V] == 1 && currentState != PlayerState.AttackCombo)
            {
                currentState = PlayerState.AttackCombo;
            }
            else if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_B] == 1 && currentState != PlayerState.Rolling)
            {
                currentState = PlayerState.Rolling;
            }
            else if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_N] == 1 && currentState != PlayerState.Sliding)
            {
                currentState = PlayerState.Sliding;
            }
        }

        private void HandleSoundEffects(byte[] keyState, PlayerState previousState)
        {
            if (currentState == PlayerState.Running && (previousState != PlayerState.Running || previousState != PlayerState.Jumping))
            {
                if (!soundManager.IsSoundPlaying("walk"))
                {
                    soundManager.PlaySound("walk");
                }
            }
            else if (currentState != PlayerState.Running && previousState == PlayerState.Running)
            {
                soundManager.StopSound("walk");
            }

            if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_X] == 1 && !soundManager.IsSoundPlaying("sword"))
            {
                soundManager.PlaySound("sword");
            }
            else if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_C] == 1 && !soundManager.IsSoundPlaying("sword"))
            {
                soundManager.PlaySound("sword");
            }
            else if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_V] == 1 && !soundManager.IsSoundPlaying("sword"))
            {
                soundManager.PlaySound("sword");
            }
        }

        private void CheckCoinCollision(TileLoader tileLoader)
        {
            foreach (var coinRect in tileLoader.CoinRectangles.ToArray())
            {
                if (CheckCollision(rect, coinRect))
                {
                    tileLoader.CoinRectangles.Remove(coinRect);
                    tileLoader.CollectedCoinPositions.Add((coinRect.x / tileLoader.TileWidth, coinRect.y / tileLoader.TileHeight)); // Add position to collected coins
                    soundManager.PlaySound("coin"); // Play coin sound
                    Console.WriteLine("Coin collected!");
                }
            }
        }

        private bool CheckCollision(SDL.SDL_Rect a, SDL.SDL_Rect b)
        {
            return (a.x < b.x + b.w &&
                    a.x + a.w > b.x &&
                    a.y < b.y + b.h &&
                    a.h + a.y > b.y);
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
            SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255); // Siyah renk

            SDL.SDL_Rect renderRect = camera.GetRenderRect(rect);
            SDL.SDL_RenderDrawRect(renderer, ref renderRect);
        }
    }
}
