using System;
using System.Collections.Generic;
using SDL2;
using System.Numerics;

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
        Sliding
    }

    public class Player : ITarget
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
        private const float Gravity = 0.3f;
        private const float JumpSpeed = 3f;
        private const float MoveSpeed = 1f;
        private const float RollSpeed = 1f;
        private const float SlideSpeed = 1f;
        private IntPtr renderer;

        public Vector2 Position => new Vector2(PositionX, PositionY);
        public SDL.SDL_Rect Rect { get; internal set; }

        public Player(int x, int y, int width, int height, float speed, IntPtr renderer)
        {
            Rect = new SDL.SDL_Rect { x = x, y = y, w = width, h = height };
            this.PositionX = x;
            this.PositionY = y;
            this.FrameWidth = width;
            this.FrameHeight = height;
            this.renderer = renderer;
            this.spritesheets = new Dictionary<PlayerState, IntPtr>();
            this.frameCounts = new Dictionary<PlayerState, int>();
            facingLeft = false;
            isJumping = false;
            jumpSpeed = 0;
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

        public void Update(float deltaTime, byte[] keyState, CollisionManager collisionManager)
        {
            animationTimer += deltaTime;
            if (animationTimer >= animationSpeed)
            {
                currentFrame = (currentFrame + 1) % frameCounts[currentState];
                animationTimer = 0f;

                if ((currentState == PlayerState.Attacking || currentState == PlayerState.Attacking2 || currentState == PlayerState.AttackCombo || currentState == PlayerState.Crouching || currentState == PlayerState.CrouchWalking || currentState == PlayerState.Rolling || currentState == PlayerState.Sliding) && currentFrame == 0)
                {
                    currentState = PlayerState.Idle;
                }
            }

            HandleInput(keyState, deltaTime, collisionManager);
            UpdatePosition(deltaTime);
        }

        public void HandleInput(byte[] keyState, float deltaTime, CollisionManager collisionManager)
        {
            bool movingHorizontally = false;
            bool crouching = keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_DOWN] == 1;

            if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_LEFT] == 1)
            {
                facingLeft = true;
                if (currentState != PlayerState.Rolling && currentState != PlayerState.Sliding)
                {
                    PositionX -= MoveSpeed * deltaTime * 100;
                    movingHorizontally = true;
                }
            }
            else if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_RIGHT] == 1)
            {
                facingLeft = false;
                if (currentState != PlayerState.Rolling && currentState != PlayerState.Sliding)
                {
                    PositionX += MoveSpeed * deltaTime * 100;
                    movingHorizontally = true;
                }
            }

            if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_UP] == 1 && !isJumping)
            {
                isJumping = true;
                jumpSpeed = -JumpSpeed;
                currentState = PlayerState.Jumping;
            }

            if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_X] == 1 && currentState != PlayerState.Attacking && currentState != PlayerState.Attacking2 && currentState != PlayerState.AttackCombo)
            {
                currentState = PlayerState.Attacking;
                currentFrame = 0;
            }

            if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_C] == 1 && currentState != PlayerState.Attacking && currentState != PlayerState.Attacking2 && currentState != PlayerState.AttackCombo)
            {
                currentState = PlayerState.Attacking2;
                currentFrame = 0;
            }

            if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_V] == 1 && currentState != PlayerState.Attacking && currentState != PlayerState.Attacking2 && currentState != PlayerState.AttackCombo)
            {
                currentState = PlayerState.AttackCombo;
                currentFrame = 0;
            }

            if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_B] == 1 && currentState != PlayerState.Rolling)
            {
                currentState = PlayerState.Rolling;
                currentFrame = 0;
            }

            if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_N] == 1 && currentState != PlayerState.Sliding)
            {
                currentState = PlayerState.Sliding;
                currentFrame = 0;
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
                currentFrame = 0;
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
                PositionY += jumpSpeed * deltaTime * 100;
                jumpSpeed += Gravity * 100 * deltaTime;

                if (PositionY > 0)
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
                    PositionX -= RollSpeed * deltaTime * 100;
                }
                else
                {
                    PositionX += RollSpeed * deltaTime * 100;
                }
            }

            if (currentState == PlayerState.Sliding)
            {
                if (facingLeft)
                {
                    PositionX -= SlideSpeed * deltaTime * 100;
                }
                else
                {
                    PositionX += SlideSpeed * deltaTime * 100;
                }
            }
        }

        public void Render(IntPtr renderer, Camera camera)
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
                y = (int)PositionY,
                w = FrameWidth,
                h = FrameHeight
            };

            dstRect = camera.GetRenderRect(dstRect);

            SDL.SDL_RendererFlip flip = facingLeft ? SDL.SDL_RendererFlip.SDL_FLIP_HORIZONTAL : SDL.SDL_RendererFlip.SDL_FLIP_NONE;
            SDL.SDL_RenderCopyEx(renderer, texture, ref srcRect, ref dstRect, 0, IntPtr.Zero, flip);
        }
    }
}
