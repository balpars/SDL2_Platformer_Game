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

        public Vector2 Position => new Vector2(rect.x, rect.y);

        public SDL.SDL_Rect Rect
        {
            get => rect;
            internal set => rect = value;
        }

        public Player(int x, int y, int width, int height, IntPtr renderer)
        {
            rect = new SDL.SDL_Rect { x = x, y = y, w = width, h = height };
            this.renderer = renderer;
            animationManager = new AnimationManager();
            movementManager = new MovementManager();
            facingLeft = false;
            currentState = PlayerState.Idle;
            animationEnded = false;
            isJumping = false;
            jumpSpeed = 0f;
        }

        public void LoadContent()
        {
            animationManager.LoadContent(renderer);
        }

        public void Update(float deltaTime, byte[] keyState, CollisionManager collisionManager)
        {
            animationEnded = false;
            animationManager.UpdateAnimation(currentState, deltaTime, ref animationEnded);
            movementManager.HandleInput(keyState, ref rect, ref currentState, ref facingLeft, ref isJumping, ref jumpSpeed, deltaTime, collisionManager);
            movementManager.UpdatePosition(deltaTime, ref rect, ref currentState, ref isJumping, ref jumpSpeed, facingLeft, collisionManager);

            if (animationEnded && (currentState == PlayerState.Attacking || currentState == PlayerState.Attacking2 || currentState == PlayerState.AttackCombo || currentState == PlayerState.Rolling || currentState == PlayerState.Sliding || currentState == PlayerState.Crouching || currentState == PlayerState.CrouchWalking))
            {
                currentState = PlayerState.Idle;
            }

            DebugPosition();
        }

        private void DebugPosition()
        {
            Console.WriteLine($"Player Position: X={rect.x}, Y={rect.y}");
        }

        public void Render(Camera camera)
        {
            IntPtr texture = animationManager.GetCurrentTexture(currentState);
            SDL.SDL_QueryTexture(texture, out _, out _, out int textureWidth, out _);

            SDL.SDL_Rect srcRect = new SDL.SDL_Rect
            {
                x = animationManager.GetCurrentFrame() * rect.w,
                y = 0,
                w = rect.w,
                h = rect.h
            };
            SDL.SDL_Rect dstRect = new SDL.SDL_Rect
            {
                x = rect.x,
                y = rect.y,
                w = rect.w,
                h = rect.h
            };

            dstRect = camera.GetRenderRect(dstRect);

            SDL.SDL_RendererFlip flip = facingLeft ? SDL.SDL_RendererFlip.SDL_FLIP_HORIZONTAL : SDL.SDL_RendererFlip.SDL_FLIP_NONE;
            SDL.SDL_RenderCopyEx(renderer, texture, ref srcRect, ref dstRect, 0, IntPtr.Zero, flip);
        }
    }
}
