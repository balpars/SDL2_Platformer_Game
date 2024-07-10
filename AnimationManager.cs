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

            CalculateFrameCounts();
        }

        private void CalculateFrameCounts()
        {
            foreach (var state in spritesheets.Keys)
            {
                SDL.SDL_QueryTexture(spritesheets[state], out _, out _, out int textureWidth, out _);
                frameCounts[state] = textureWidth / 120; // FrameWidth
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
                currentFrame = (currentFrame + 1) % frameCounts[currentState];
                animationTimer = 0f;

                if (currentFrame == frameCounts[currentState] - 1)
                {
                    animationEnded = true;
                }
            }
        }

        public int GetCurrentFrame()
        {
            return currentFrame;
        }

        public IntPtr GetCurrentTexture(PlayerState state)
        {
            return spritesheets[state];
        }
    }
}
