using SDL2;
using System;
using System.Collections.Generic;

namespace Platformer_Game
{
    public enum PlayerState
    {
        Idle,
        Running
    }

    public class Player
    {
        private Dictionary<PlayerState, IntPtr> spritesheets;
        private PlayerState currentState;
        private int currentFrame;
        public int FrameWidth { get; private set; }
        public int FrameHeight { get; private set; }
        private int frameCount;
        private float animationSpeed = 0.1f;
        private float animationTimer;
        private bool facingLeft;
        public int PositionX { get; private set; } // Oyuncunun pozisyonunu takip etmek için ekledik

        public Player()
        {
            currentState = PlayerState.Idle;
            currentFrame = 0;
            FrameWidth = 120; // Frame genişliği
            FrameHeight = 80; // Frame yüksekliği
            spritesheets = new Dictionary<PlayerState, IntPtr>();
            facingLeft = false;
            PositionX = 0; // Başlangıç pozisyonu
        }

        public void LoadContent(IntPtr renderer)
        {
            // Spritesheetleri yükle
            spritesheets[PlayerState.Idle] = LoadTexture(renderer, "Assets/_Idle.png");
            spritesheets[PlayerState.Running] = LoadTexture(renderer, "Assets/_Run.png");

            // Frame count hesapla
            CalculateFrameCounts();
        }

        private void CalculateFrameCounts()
        {
            SDL.SDL_QueryTexture(spritesheets[PlayerState.Idle], out _, out _, out int idleTextureWidth, out _);
            frameCount = idleTextureWidth / FrameWidth; // Idle durumunda kaç frame olduğunu hesapla
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
                currentFrame = (currentFrame + 1) % frameCount;
                animationTimer = 0f;
            }

            // Durum güncellemeleri ve geçişleri
            HandleInput();
        }

        private void HandleInput()
        {
            var keystatePtr = SDL.SDL_GetKeyboardState(out _);
            byte[] keystate = new byte[512]; // SDL_NUM_SCANCODES yerine sabit 512
            System.Runtime.InteropServices.Marshal.Copy(keystatePtr, keystate, 0, keystate.Length);

            if (keystate[(int)SDL.SDL_Scancode.SDL_SCANCODE_LEFT] == 1)
            {
                currentState = PlayerState.Running;
                facingLeft = true;
                PositionX -= 1; // Sol hareket ederken pozisyonu güncelle
            }
            else if (keystate[(int)SDL.SDL_Scancode.SDL_SCANCODE_RIGHT] == 1)
            {
                currentState = PlayerState.Running;
                facingLeft = false;
                PositionX += 1; // Sağ hareket ederken pozisyonu güncelle
            }
            else
            {
                currentState = PlayerState.Idle;
            }
        }

        public void Render(IntPtr renderer, int x, int y)
        {
            IntPtr texture = spritesheets[currentState];
            SDL.SDL_QueryTexture(texture, out _, out _, out int textureWidth, out _);

            int stateFrameCount = textureWidth / FrameWidth;

            SDL.SDL_Rect srcRect = new SDL.SDL_Rect
            {
                x = currentFrame * FrameWidth,
                y = 0,
                w = FrameWidth,
                h = FrameHeight
            };
            SDL.SDL_Rect dstRect = new SDL.SDL_Rect
            {
                x = x,
                y = y,
                w = FrameWidth,
                h = FrameHeight
            };

            SDL.SDL_RendererFlip flip = facingLeft ? SDL.SDL_RendererFlip.SDL_FLIP_HORIZONTAL : SDL.SDL_RendererFlip.SDL_FLIP_NONE;
            SDL.SDL_RenderCopyEx(renderer, texture, ref srcRect, ref dstRect, 0, IntPtr.Zero, flip);
        }
    }
}
