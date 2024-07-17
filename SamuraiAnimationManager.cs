// SamuraiAnimationManager.cs
using System;
using System.Collections.Generic;
using SDL2;

namespace Platformer_Game
{
    public class SamuraiAnimationManager
    {
        private Dictionary<SamuraiState, IntPtr> spritesheets;
        private Dictionary<SamuraiState, int> frameCounts;
        private float animationSpeed = 0.1f;
        private float animationTimer;
        private int currentFrame;

        private const int OriginalFrameWidth = 93;
        private const int OriginalFrameHeight = 50;

        private const int FrameWidth = 93;
        private const int FrameHeight = 50;

        public SamuraiAnimationManager()
        {
            spritesheets = new Dictionary<SamuraiState, IntPtr>();
            frameCounts = new Dictionary<SamuraiState, int>();
            currentFrame = 0;
        }

        public void LoadContent(IntPtr renderer)
        {
            spritesheets[SamuraiState.Idle] = LoadTexture(renderer, "Assets/Samurai/idle_s.png");
            spritesheets[SamuraiState.Running] = LoadTexture(renderer, "Assets/Samurai/run_s.png");
            spritesheets[SamuraiState.Attacking] = LoadTexture(renderer, "Assets/Samurai/attack_s.png");

            CalculateFrameCounts();
        }

        private void CalculateFrameCounts()
        {
            foreach (var state in spritesheets.Keys)
            {
                SDL.SDL_QueryTexture(spritesheets[state], out _, out _, out int textureWidth, out _);
                frameCounts[state] = textureWidth / OriginalFrameWidth;
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

        public void UpdateAnimation(SamuraiState currentState, float deltaTime, ref bool animationEnded)
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

        public IntPtr GetCurrentTexture(SamuraiState state)
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
                x = destRect.x - 37,
                y = destRect.y,
                w = FrameWidth,
                h = FrameHeight
            };
        }
    }
}
