using System;
using SDL2;

namespace Platformer_Game
{
    public class GameLoop
    {
        private Game game;

        public GameLoop(Game game)
        {
            this.game = game;
        }

        public void Run()
        {
            SDL.SDL_Event e;
            float deltaTime = 0;
            uint lastTime = SDL.SDL_GetTicks();

            while (true)
            {
                while (SDL.SDL_PollEvent(out e) != 0)
                {
                    if (e.type == SDL.SDL_EventType.SDL_QUIT)
                    {
                        return;
                    }
                }

                uint currentTime = SDL.SDL_GetTicks();
                deltaTime = (currentTime - lastTime) / 1000.0f;
                lastTime = currentTime;

                game.Update(deltaTime); // Eksik argümanı ekledik
                game.Render();
            }
        }
    }
}
