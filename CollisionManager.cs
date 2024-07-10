using System;
using System.Collections.Generic;
using SDL2;

namespace Platformer_Game
{
    public class CollisionManager
    {
        private List<SDL.SDL_Rect> collisionRectangles;

        public CollisionManager(List<SDL.SDL_Rect> collisionRectangles)
        {
            this.collisionRectangles = collisionRectangles;
        }

        public bool CheckCollisions(Player player, SDL.SDL_Rect newRect)
        {
            foreach (var rect in collisionRectangles)
            {
                if (CheckCollision(newRect, rect))
                {
                    if (newRect.y + newRect.h > rect.y && player.Rect.y + player.Rect.h <= rect.y)
                    {
                        Console.WriteLine($"Collision detected at coordinates ({rect.x}, {rect.y})");
                        newRect.y = rect.y - player.Rect.h;
                        player.Rect = newRect;
                        return true;
                    }
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
