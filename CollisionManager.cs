using SDL2;
using System.Collections.Generic;

namespace Platformer_Game
{
    public class CollisionManager
    {
        private List<SDL.SDL_Rect> collisionRectangles;

        public CollisionManager(List<SDL.SDL_Rect> collisionRectangles)
        {
            this.collisionRectangles = collisionRectangles;
        }

        public bool CheckCollisions(SDL.SDL_Rect newRect)
        {
            foreach (var rect in collisionRectangles)
            {
                if (CheckCollision(newRect, rect))
                {
                    return true;
                }
            }
            return false;
        }

        private bool CheckCollision(SDL.SDL_Rect a, SDL.SDL_Rect b)
        {
            return (a.x < b.x + b.w &&
                    a.x + a.w > b.x &&
                    a.y < b.y + b.h &&
                    a.h + a.y > b.y);
        }
    }
}
