using SDL2;
using System.Numerics;

namespace Platformer_Game
{
    public class Camera
    {
        public Vector2 Position { get; private set; }
        public float Smoothing { get; set; } = 1.0f;
        public float Zoom { get; set; } = 1.0f; // Default zoom level

        private ITarget target;
        private float screenWidth;
        private float screenHeight;

        public Camera(float screenWidth, float screenHeight)
        {
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
        }

        public void SetTarget(ITarget newTarget)
        {
            target = newTarget;
            CenterOnTarget();
        }

        private void CenterOnTarget()
        {
            if (target != null)
            {
                Position = target.Position - new Vector2(screenWidth / 2, screenHeight / 2) / Zoom;
            }
        }

        public void Update(float deltaTime)
        {
            if (target != null)
            {
                Vector2 targetPosition = target.Position - new Vector2(screenWidth / 2, screenHeight / 2) / Zoom;
                Position = Vector2.Lerp(Position, targetPosition, Smoothing * deltaTime);
            }
        }

        public Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            return screenPosition / Zoom + Position;
        }

        public SDL.SDL_Rect GetRenderRect(SDL.SDL_Rect rect)
        {
            return new SDL.SDL_Rect
            {
                x = (int)((rect.x - Position.X) * Zoom),
                y = (int)((rect.y - Position.Y) * Zoom),
                w = (int)(rect.w * Zoom),
                h = (int)(rect.h * Zoom)
            };
        }
    }

    public interface ITarget
    {
        Vector2 Position { get; }
    }
}
