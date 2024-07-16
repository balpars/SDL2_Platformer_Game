// CollisionManager.cs
using SDL2;
using System.Collections.Generic;

namespace Platformer_Game
{
    public class CollisionManager
    {
        private List<SDL.SDL_Rect> collisionRectangles;
        private List<SDL.SDL_Rect> climbingRectangles;

        public CollisionManager(List<SDL.SDL_Rect> collisionRectangles, List<SDL.SDL_Rect> climbingRectangles)
        {
            this.collisionRectangles = collisionRectangles;
            this.climbingRectangles = climbingRectangles;
        }

        public bool CheckCollisions(SDL.SDL_Rect newRect)
        {
            //newRect.w = (newRect.w / 10);
            //newRect.x += 52;
            //newRect.h = newRect.h / 3;
            //newRect.y += 52;


            //Console.WriteLine($"newRect.w = {newRect.w} newRect.h = {newRect.h}" +
            //    $"newRect.x = {newRect.x} newRect.y = {newRect.y}");
            foreach (var rect in collisionRectangles)
            {
                if (CheckCollision(newRect, rect))
                {

                    return true;
                }
            }
            return false;
        }


        public bool CheckClimbingLayer(SDL.SDL_Rect newRect)
        {
            foreach (var rect in climbingRectangles)
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

        public void RenderDebug(IntPtr renderer, Camera camera)
        {
            SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255); // Siyah renk

            foreach (var rect in collisionRectangles)
            {
                SDL.SDL_Rect renderRect = camera.GetRenderRect(rect);
                SDL.SDL_RenderDrawRect(renderer, ref renderRect);
            }

            foreach (var rect in climbingRectangles)
            {
                SDL.SDL_Rect renderRect = camera.GetRenderRect(rect);
                SDL.SDL_RenderDrawRect(renderer, ref renderRect);
            }
        }
    }
}
