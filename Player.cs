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
        private bool isGameOver;
        private GameOverScreen gameOverScreen;

        // Health properties
        private int maxHealth;
        private int currentHealth;
        public int CoinCount { get; set; }
        private IntPtr font;


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
            previousState = PlayerState.Idle; 
            animationEnded = false;
            isJumping = false;
            jumpSpeed = 0f;
            this.soundManager = soundManager;
            originalY = y + 42;

            // Initialize health
            maxHealth = 150;
            currentHealth = 150;

            CoinCount = 0;


            isGameOver = false;
            gameOverScreen = new GameOverScreen(renderer);

            font = SDL_ttf.TTF_OpenFont("C:\\Users\\ebrar\\source\\repos\\SDL2_Platformer_Game\\Assets\\Fonts\\GreenBerry.ttf", 16);
            if (font == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load font.");
            }
        }

        public void LoadContent()
        {
            animationManager.LoadContent(renderer);
        }

        public void Update(float deltaTime, byte[] keyState, CollisionManager collisionManager, TileLoader tileLoader, Samurai samurai)
        {
            if (isGameOver)
            {
                gameOverScreen.Update(keyState);
                return;
            }

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

            if (currentState == PlayerState.Attacking || currentState == PlayerState.Attacking2 || currentState == PlayerState.AttackCombo)
            {
                SDL.SDL_Rect attackRect = GetAttackRect();
                SDL.SDL_Rect samuraiRect = samurai.Rect; // Copy the property to a local variable
                if (SDL.SDL_HasIntersection(ref attackRect, ref samuraiRect) == SDL.SDL_bool.SDL_TRUE)
                {
                    samurai.TakeDamage(1);
                }
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
                for (int i = 0; i < tileLoader.CoinRectangles.Count; i++)
                {
                    SDL.SDL_Rect coinRect = tileLoader.CoinRectangles[i];
                    if (SDL.SDL_HasIntersection(ref rect, ref coinRect) == SDL.SDL_bool.SDL_TRUE)
                    {
                        tileLoader.CollectedCoinPositions.Add((coinRect.x / tileLoader.TileWidth, coinRect.y / tileLoader.TileHeight));
                        tileLoader.RemoveCoinAt(i); // Remove the coin at index
                        CoinCount++; // Increment the coin counter
                        soundManager.PlaySound("coin");
                    }
                }
            }

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
                    CoinCount++; 
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
            if (isGameOver)
            {
                gameOverScreen.Render();
                return;
            }

            IntPtr texture = animationManager.GetCurrentTexture(currentState);
            SDL.SDL_Rect srcRect = animationManager.GetSourceRect();
            SDL.SDL_Rect dstRect = animationManager.GetDestinationRect(rect);

            if (currentState == PlayerState.Crouching || currentState == PlayerState.CrouchWalking)
            {
                dstRect.y -= (CharacterHeight - CrouchHeight); 
            }

            dstRect = camera.GetRenderRect(dstRect);

            SDL.SDL_RendererFlip flip = facingLeft ? SDL.SDL_RendererFlip.SDL_FLIP_HORIZONTAL : SDL.SDL_RendererFlip.SDL_FLIP_NONE;
            SDL.SDL_RenderCopyEx(renderer, texture, ref srcRect, ref dstRect, 0, IntPtr.Zero, flip);

            // Render health bar
            RenderHealthBar(camera);

            RenderCoinCount(camera);

        }
        private void RenderCoinCount(Camera camera)
        {
            if (font == IntPtr.Zero)
            {
                Console.WriteLine("Font is not loaded.");
                return;
            }

            // Convert coin count to string
            string coinCountText = $"Coins: {CoinCount}";

            SDL.SDL_Color textColor = new SDL.SDL_Color { r = 255, g = 255, b = 255, a = 255 };

            // Create surface and texture for text
            IntPtr textSurface = SDL_ttf.TTF_RenderText_Solid(font, coinCountText, textColor);
            if (textSurface == IntPtr.Zero)
            {
                Console.WriteLine("Failed to create text surface.");
                return;
            }

            IntPtr textTexture = SDL.SDL_CreateTextureFromSurface(renderer, textSurface);
            SDL.SDL_FreeSurface(textSurface); // Free the surface as it is no longer needed

            if (textTexture == IntPtr.Zero)
            {
                Console.WriteLine("Failed to create text texture.");
                return;
            }

            // Get text dimensions
            SDL.SDL_QueryTexture(textTexture, out _, out _, out int textWidth, out int textHeight);

            // Define fixed screen position for text
            SDL.SDL_Rect textRect = new SDL.SDL_Rect
            {
                x = 10, // Fixed X position on screen
                y = 10, // Fixed Y position on screen
                w = textWidth +10,
                h = textHeight +10
            };

            // Render text at fixed screen position
            SDL.SDL_RenderCopy(renderer, textTexture, IntPtr.Zero, ref textRect);

            // Clean up
            SDL.SDL_DestroyTexture(textTexture);
        }

        private void RenderHealthBar(Camera camera)
        {
            SDL.SDL_Rect healthBarBackground = new SDL.SDL_Rect
            {
                x = rect.x,
                y = rect.y - 10,
                w = CharacterWidth,
                h = 4
            };
            SDL.SDL_Rect healthBarForeground = new SDL.SDL_Rect
            {
                x = rect.x,
                y = rect.y - 10,
                w = (int)(CharacterWidth * ((float)currentHealth / maxHealth)),
                h = 4
            };

            healthBarBackground = camera.GetRenderRect(healthBarBackground);
            healthBarForeground = camera.GetRenderRect(healthBarForeground);

            SDL.SDL_SetRenderDrawColor(renderer, 255, 0, 0, 255); // Red for the background
            SDL.SDL_RenderFillRect(renderer, ref healthBarBackground);

            SDL.SDL_SetRenderDrawColor(renderer, 0, 255, 0, 255); // Green for the foreground
            SDL.SDL_RenderFillRect(renderer, ref healthBarForeground);
        }

        public void RenderDebug(IntPtr renderer, Camera camera)
        {
            SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);

            SDL.SDL_Rect renderRect = camera.GetRenderRect(rect);
            SDL.SDL_RenderDrawRect(renderer, ref renderRect);
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

        // Health management methods
        public void TakeDamage(int amount)
        {
            currentHealth -= amount;
            Console.WriteLine($"Player took {amount} damage, health is now {currentHealth}");

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                currentState = PlayerState.Death;
                animationManager.ResetAnimation();
                isGameOver = true;
                Console.WriteLine("Player is dead!");
            }
        }

        public void Heal(int amount)
        {
            currentHealth += amount;
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }
        }

        public bool IsDead()
        {
            return currentHealth <= 0;
        }

        public void Reset(int x, int y)
        {
            rect.x = x;
            rect.y = y;
            currentHealth = maxHealth;
            currentState = PlayerState.Idle;
            previousState = PlayerState.Idle;
            animationEnded = false;
            isJumping = false;
            jumpSpeed = 0f;
            isGameOver = false;
        }

    }
}
