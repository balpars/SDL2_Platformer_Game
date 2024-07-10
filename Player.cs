using System;
using System.Collections.Generic;
using SDL2;

namespace Platformer_Game
{
    public enum PlayerState
    {
        Idle,
        Running,
        Jumping,
        JumpFall,
        Falling,
        Attacking,
        Attacking2,
        AttackCombo,
        Crouching,
        CrouchWalking,
        Rolling,
        Sliding // Yeni eklenen durum
    }

    public class Player
    {
        private Dictionary<PlayerState, IntPtr> spritesheets;
        private PlayerState currentState;
        private int currentFrame;
        public int FrameWidth { get; private set; }
        public int FrameHeight { get; private set; }
        private Dictionary<PlayerState, int> frameCounts;
        private float animationSpeed = 0.1f;
        private float animationTimer;
        private bool facingLeft;
        public float PositionX { get; private set; }
        public float PositionY { get; private set; }
        private bool isJumping;
        private float jumpSpeed;
        private const float Gravity = 0.3f; // Yerçekimi biraz azaltıldı
        private const float JumpSpeed = 8f; // Zıplama hızı azaltıldı
        private const float MoveSpeed = 3f; // Hareket hızı artırıldı
        private const float RollSpeed = 2f; // Yuvarlanma hızı
        private const float SlideSpeed = 2f; // Kayma hızı

        public Player()
        {
            currentState = PlayerState.Idle;
            currentFrame = 0;
            FrameWidth = 120;
            FrameHeight = 80;
            spritesheets = new Dictionary<PlayerState, IntPtr>();
            frameCounts = new Dictionary<PlayerState, int>();
            facingLeft = false;
            PositionX = 0;
            PositionY = 0;
            isJumping = false;
            jumpSpeed = 0;
        }

        public void LoadContent(IntPtr renderer)
        {
            // Spritesheetleri yükle
            spritesheets[PlayerState.Idle] = LoadTexture(renderer, "Assets/_Idle.png");
            spritesheets[PlayerState.Running] = LoadTexture(renderer, "Assets/_Run.png");
            spritesheets[PlayerState.Jumping] = LoadTexture(renderer, "Assets/_Jump.png");
            spritesheets[PlayerState.JumpFall] = LoadTexture(renderer, "Assets/_JumpFallInbetween.png");
            spritesheets[PlayerState.Falling] = LoadTexture(renderer, "Assets/_Fall.png");
            spritesheets[PlayerState.Attacking] = LoadTexture(renderer, "Assets/_Attack.png");
            spritesheets[PlayerState.Attacking2] = LoadTexture(renderer, "Assets/_Attack2.png");
            spritesheets[PlayerState.AttackCombo] = LoadTexture(renderer, "Assets/_AttackCombo.png");
            spritesheets[PlayerState.Crouching] = LoadTexture(renderer, "Assets/_CrouchFull.png");
            spritesheets[PlayerState.CrouchWalking] = LoadTexture(renderer, "Assets/_CrouchWalk.png");
            spritesheets[PlayerState.Rolling] = LoadTexture(renderer, "Assets/_Roll.png");
            spritesheets[PlayerState.Sliding] = LoadTexture(renderer, "Assets/_SlideFull.png"); // Yeni animasyon

            // Frame count hesapla
            CalculateFrameCounts();
        }

        private void CalculateFrameCounts()
        {
            foreach (var state in spritesheets.Keys)
            {
                SDL.SDL_QueryTexture(spritesheets[state], out _, out _, out int textureWidth, out _);
                frameCounts[state] = textureWidth / FrameWidth;
                Console.WriteLine($"{state} spritesheet has {frameCounts[state]} frames.");
            }
        }

        private IntPtr LoadTexture(IntPtr renderer, string filePath)
        {
            IntPtr texture = SDL_image.IMG_LoadTexture(renderer, filePath);
            if (texture == IntPtr.Zero)
            {
                Console.WriteLine($"Failed to load texture {filePath}! SDL_Error: {SDL.SDL_GetError()}");
            }
            return texture;
        }

        public void Update(float deltaTime)
        {
            animationTimer += deltaTime;
            if (animationTimer >= animationSpeed)
            {
                currentFrame = (currentFrame + 1) % frameCounts[currentState];
                animationTimer = 0f;

                // Attack, crouch, roll ve slide animasyonları tamamlandığında tekrar idle duruma geç
                if ((currentState == PlayerState.Attacking || currentState == PlayerState.Attacking2 || currentState == PlayerState.AttackCombo || currentState == PlayerState.Crouching || currentState == PlayerState.CrouchWalking || currentState == PlayerState.Rolling || currentState == PlayerState.Sliding) && currentFrame == 0)
                {
                    currentState = PlayerState.Idle;
                }
            }

            // Durum güncellemeleri ve geçişleri
            HandleInput(deltaTime);
            UpdatePosition(deltaTime);
        }

        private void HandleInput(float deltaTime)
        {
            var keystatePtr = SDL.SDL_GetKeyboardState(out _);
            byte[] keystate = new byte[512];
            System.Runtime.InteropServices.Marshal.Copy(keystatePtr, keystate, 0, keystate.Length);

            bool movingHorizontally = false;
            bool crouching = keystate[(int)SDL.SDL_Scancode.SDL_SCANCODE_DOWN] == 1;

            if (keystate[(int)SDL.SDL_Scancode.SDL_SCANCODE_LEFT] == 1)
            {
                facingLeft = true;
                if (currentState != PlayerState.Rolling && currentState != PlayerState.Sliding) // Roll veya Slide sırasında hareket etmeyi önlemek için
                {
                    PositionX -= MoveSpeed * deltaTime * 100; // deltaTime ile çarpma
                    movingHorizontally = true;
                }
            }
            else if (keystate[(int)SDL.SDL_Scancode.SDL_SCANCODE_RIGHT] == 1)
            {
                facingLeft = false;
                if (currentState != PlayerState.Rolling && currentState != PlayerState.Sliding) // Roll veya Slide sırasında hareket etmeyi önlemek için
                {
                    PositionX += MoveSpeed * deltaTime * 100; // deltaTime ile çarpma
                    movingHorizontally = true;
                }
            }

            if (keystate[(int)SDL.SDL_Scancode.SDL_SCANCODE_UP] == 1 && !isJumping)
            {
                isJumping = true;
                jumpSpeed = -JumpSpeed; // Yukarı zıplama hızı
                currentState = PlayerState.Jumping;
            }

            if (keystate[(int)SDL.SDL_Scancode.SDL_SCANCODE_X] == 1 && currentState != PlayerState.Attacking && currentState != PlayerState.Attacking2 && currentState != PlayerState.AttackCombo)
            {
                currentState = PlayerState.Attacking;
                currentFrame = 0; // Attack animasyonunu başlat
            }

            if (keystate[(int)SDL.SDL_Scancode.SDL_SCANCODE_C] == 1 && currentState != PlayerState.Attacking && currentState != PlayerState.Attacking2 && currentState != PlayerState.AttackCombo)
            {
                currentState = PlayerState.Attacking2;
                currentFrame = 0; // Attack2 animasyonunu başlat
            }

            if (keystate[(int)SDL.SDL_Scancode.SDL_SCANCODE_V] == 1 && currentState != PlayerState.Attacking && currentState != PlayerState.Attacking2 && currentState != PlayerState.AttackCombo)
            {
                currentState = PlayerState.AttackCombo;
                currentFrame = 0; // AttackCombo animasyonunu başlat
            }

            if (keystate[(int)SDL.SDL_Scancode.SDL_SCANCODE_B] == 1 && currentState != PlayerState.Rolling)
            {
                currentState = PlayerState.Rolling;
                currentFrame = 0; // Roll animasyonunu başlat
            }

            if (keystate[(int)SDL.SDL_Scancode.SDL_SCANCODE_N] == 1 && currentState != PlayerState.Sliding)
            {
                currentState = PlayerState.Sliding;
                currentFrame = 0; // Slide animasyonunu başlat
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
                currentFrame = 0; // Crouch veya CrouchWalk animasyonunu başlat
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

        private void UpdatePosition(float deltaTime)
        {
            if (isJumping)
            {
                PositionY += jumpSpeed * deltaTime * 100; // deltaTime ile çarpma
                jumpSpeed += Gravity * 100 * deltaTime; // deltaTime ile çarpma

                if (PositionY > 0) // Yere değdiğinde sıfırlama
                {
                    PositionY = 0;
                    isJumping = false;
                    currentState = PlayerState.Idle;
                }
            }

            if (currentState == PlayerState.Rolling)
            {
                if (facingLeft)
                {
                    PositionX -= RollSpeed * deltaTime * 100; // deltaTime ile çarpma
                }
                else
                {
                    PositionX += RollSpeed * deltaTime * 100; // deltaTime ile çarpma
                }
            }

            if (currentState == PlayerState.Sliding)
            {
                if (facingLeft)
                {
                    PositionX -= SlideSpeed * deltaTime * 100; // deltaTime ile çarpma
                }
                else
                {
                    PositionX += SlideSpeed * deltaTime * 100; // deltaTime ile çarpma
                }
            }
        }

        public void Render(IntPtr renderer, int x, int y)
        {
            IntPtr texture = spritesheets[currentState];
            SDL.SDL_QueryTexture(texture, out _, out _, out int textureWidth, out _);

            SDL.SDL_Rect srcRect = new SDL.SDL_Rect
            {
                x = currentFrame * FrameWidth,
                y = 0,
                w = FrameWidth,
                h = FrameHeight
            };
            SDL.SDL_Rect dstRect = new SDL.SDL_Rect
            {
                x = (int)PositionX,
                y = y + (int)PositionY, // Pozisyonu düzeltme
                w = FrameWidth,
                h = FrameHeight
            };

            SDL.SDL_RendererFlip flip = facingLeft ? SDL.SDL_RendererFlip.SDL_FLIP_HORIZONTAL : SDL.SDL_RendererFlip.SDL_FLIP_NONE;
            SDL.SDL_RenderCopyEx(renderer, texture, ref srcRect, ref dstRect, 0, IntPtr.Zero, flip);
        }
    }
}
