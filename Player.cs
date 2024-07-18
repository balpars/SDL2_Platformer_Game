using SDL2;
using System.Numerics;

namespace Platformer_Game
{
    public class Player : ITarget
    {
        private SDL.SDL_Rect rect;
        private bool facingLeft;
        private PlayerState currentState;
        private PlayerState previousState; // Önceki durumu tutmak için
        private AnimationManager animationManager;
        private MovementManager movementManager;
        private IntPtr renderer;
        private bool animationEnded;
        private bool isJumping;
        private float jumpSpeed;
        private SoundManager soundManager;
        private int originalY; // Orijinal y pozisyonunu tutmak için
        private bool flag = false;

        public Vector2 Position => new Vector2(rect.x, rect.y);

        public SDL.SDL_Rect Rect
        {
            get => rect;
            internal set => rect = value;
        }

        private const int CharacterWidth = 20;
        private const int CharacterHeight = 40;
        private const int CrouchHeight = 20;

        public Player(int x, int y, int width, int height, IntPtr renderer, SoundManager soundManager)
        {
            rect = new SDL.SDL_Rect { x = x, y = y, w = CharacterWidth, h = CharacterHeight };
            this.renderer = renderer;
            animationManager = new AnimationManager();
            movementManager = new MovementManager();
            facingLeft = false;
            currentState = PlayerState.Idle;
            previousState = PlayerState.Idle; // Başlangıçta previousState'i Idle olarak ayarla
            animationEnded = false;
            isJumping = false;
            jumpSpeed = 0f;
            this.soundManager = soundManager;
            originalY = y + 42;
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

            movementManager.HandleInput(keyState, ref rect, ref currentState, ref facingLeft, ref isJumping, ref jumpSpeed, deltaTime, collisionManager, animationManager);
            movementManager.UpdatePosition(deltaTime, ref rect, ref currentState, ref isJumping, ref jumpSpeed, facingLeft, collisionManager);

            if (animationEnded)
            {
                HandleSpecialStateTransitions(keyState);
            }

            if (previousState != currentState)
            {
                animationManager.ResetAnimation();
            }

            AdjustCollisionHeight();

            //Console.WriteLine($"Current State: {currentState}");

            HandleSoundEffects(keyState, previousState);

            if (collisionManager.CheckClimbingLayer(rect))
            {
                currentState = PlayerState.Climbing;
            }

            CheckCoinCollision(tileLoader);

            if (CheckFlagCollision(tileLoader.FlagRectangles))
            {
                // Do nothing for now, handled in the main loop
            }

            // Mevcut durumu önceki durum olarak kaydet
            previousState = currentState;
        }

        private void AdjustCollisionHeight()
        {
            if (currentState == PlayerState.Crouching || currentState == PlayerState.CrouchWalking)
            {
                rect.h = CrouchHeight;
                rect.y = originalY + (CharacterHeight - CrouchHeight); // Y pozisyonunu sabit tut
                flag = true;
            }
            else if (currentState == PlayerState.Jumping || currentState == PlayerState.JumpFall)
            {
                // Zıplama durumunda y pozisyonunu zıplama hızına göre ayarla
                rect.y += (int)jumpSpeed;
                rect.h = CharacterHeight; // Zıplarken karakterin tam yüksekliğini kullan
            }
            else 
            {
                rect.h = CharacterHeight;
                if (flag)
                {
                    rect.y = originalY; // Sadece crouch durumundan idle durumuna geçerken Y pozisyonunu orijinal haline getir
                    flag = false;
                }
                
                
            }
        }

        private void CheckCoinCollision(TileLoader tileLoader)
        {
            for (int i = 0; i < tileLoader.CoinRectangles.Count; i++)
            {
                SDL.SDL_Rect coinRect = tileLoader.CoinRectangles[i];
                if (SDL.SDL_HasIntersection(ref rect, ref coinRect) == SDL.SDL_bool.SDL_TRUE)
                {
                    tileLoader.CollectedCoinPositions.Add((coinRect.x / tileLoader.TileWidth, coinRect.y / tileLoader.TileHeight));
                    tileLoader.RemoveCoinAt(i); // Remove the coin at index
                    soundManager.PlaySound("coin");
                }
            }
        }

        private bool CheckFlagCollision(List<SDL.SDL_Rect> flagRectangles)
        {
            foreach (var rect in flagRectangles)
            {
                SDL.SDL_Rect flagRect = rect;
                if (SDL.SDL_HasIntersection(ref this.rect, ref flagRect) == SDL.SDL_bool.SDL_TRUE)
                {
                    return true;
                }
            }
            return false;
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

        public void Render(Camera camera)
        {
            IntPtr texture = animationManager.GetCurrentTexture(currentState);
            SDL.SDL_Rect srcRect = animationManager.GetSourceRect();
            SDL.SDL_Rect dstRect = animationManager.GetDestinationRect(rect);

            if (currentState == PlayerState.Crouching || currentState == PlayerState.CrouchWalking)
            {
                dstRect.y -= (CharacterHeight - CrouchHeight); // Crouch veya CrouchWalking durumunda yukarı kaydır
            }

            dstRect = camera.GetRenderRect(dstRect);

            SDL.SDL_RendererFlip flip = facingLeft ? SDL.SDL_RendererFlip.SDL_FLIP_HORIZONTAL : SDL.SDL_RendererFlip.SDL_FLIP_NONE;
            SDL.SDL_RenderCopyEx(renderer, texture, ref srcRect, ref dstRect, 0, IntPtr.Zero, flip);
        }

        public void RenderDebug(IntPtr renderer, Camera camera)
        {
            SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);

            SDL.SDL_Rect renderRect = camera.GetRenderRect(rect);
            SDL.SDL_RenderDrawRect(renderer, ref renderRect);
        }
    }
}
