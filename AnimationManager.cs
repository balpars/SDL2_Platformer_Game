// AnimationManager.cs
using System;
using System.Collections.Generic;
using SDL2;

namespace Platformer_Game
{
    public class AnimationManager
    {
        private Dictionary<PlayerState, IntPtr> spritesheets;
        private Dictionary<PlayerState, int> frameCounts;
        private float animationSpeed = 0.1f;
        private float animationTimer;
        private int currentFrame;

        // Karakterin orijinal frame boyutları
        private const int OriginalFrameWidth = 120;
        private const int OriginalFrameHeight = 80;

        // Karakterin ölçeklenmiş boyutları
        private const int FrameWidth = 120;
        private const int FrameHeight = 80;

        public AnimationManager()
        {
            spritesheets = new Dictionary<PlayerState, IntPtr>();
            frameCounts = new Dictionary<PlayerState, int>();
            currentFrame = 0;
        }

        public void LoadContent(IntPtr renderer)
        {
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
            spritesheets[PlayerState.Sliding] = LoadTexture(renderer, "Assets/_SlideFull.png");
            spritesheets[PlayerState.Climbing] = LoadTexture(renderer, "Assets/_WallClimbNoMovement.png"); // Yeni tırmanma animasyonu
            spritesheets[PlayerState.Death] = LoadTexture(renderer, "Assets/_Death.png");

            CalculateFrameCounts();
        }

        private void CalculateFrameCounts()
        {
            foreach (var state in spritesheets.Keys)
            {
                SDL.SDL_QueryTexture(spritesheets[state], out _, out _, out int textureWidth, out _);
                frameCounts[state] = textureWidth / OriginalFrameWidth; // Her bir resmin orijinal genişliği
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

        public void UpdateAnimation(PlayerState currentState, float deltaTime, ref bool animationEnded)
        {
            animationTimer += deltaTime;
            if (animationTimer >= animationSpeed)
            {
                currentFrame++;
                if (currentFrame >= frameCounts[currentState])
                {
                    currentFrame = 0;
                    animationEnded = true;
                }
                animationTimer = 0f;
            }
        }

        public void ResetAnimation()
        {
            currentFrame = 0;
            animationTimer = 0f;
        }

        public int GetCurrentFrame()
        {
            return currentFrame;
        }

        public IntPtr GetCurrentTexture(PlayerState state)
        {
            return spritesheets[state];
        }

        public SDL.SDL_Rect GetSourceRect()
        {
            return new SDL.SDL_Rect
            {
                x = currentFrame * OriginalFrameWidth,
                y = 0,
                w = OriginalFrameWidth,
                h = OriginalFrameHeight
            };
        }

        public SDL.SDL_Rect GetDestinationRect(SDL.SDL_Rect destRect)
        {
            return new SDL.SDL_Rect
            {
                x = destRect.x-45,
                y = destRect.y-40,
                w = FrameWidth,
                h = FrameHeight
            };
        }
    }
}
