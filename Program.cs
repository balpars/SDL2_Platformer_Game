using System;
using SDL2;

namespace Platformer_Game
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var (window, renderer, tileLoader, mapData, player, camera, collisionManager) = Initializer.Init();

                bool running = true;

                const float targetFps = 60.0f;
                const float fixedDeltaTime = 1.0f / targetFps;
                float accumulator = 0.0f;
                uint previousTime = SDL.SDL_GetTicks();

                Console.WriteLine("Entering main loop...");
                while (running)
                {
                    uint currentTime = SDL.SDL_GetTicks();
                    float deltaTime = (currentTime - previousTime) / 1000.0f;
                    previousTime = currentTime;
                    accumulator += deltaTime;

                    SDL.SDL_Event e;
                    while (SDL.SDL_PollEvent(out e) != 0)
                    {
                        if (e.type == SDL.SDL_EventType.SDL_QUIT)
                        {
                            running = false;
                        }
                    }

                    while (accumulator >= fixedDeltaTime)
                    {
                        IntPtr keyStatePtr = SDL.SDL_GetKeyboardState(out int numKeys);
                        byte[] keyState = new byte[numKeys];
                        System.Runtime.InteropServices.Marshal.Copy(keyStatePtr, keyState, 0, numKeys);

                        player.Update(fixedDeltaTime, keyState, collisionManager); // Update metodu düzeltildi
                        camera.Update(fixedDeltaTime);

                        accumulator -= fixedDeltaTime;
                    }

                    SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
                    SDL.SDL_RenderClear(renderer);

                    tileLoader.RenderMap(mapData, renderer, camera);
                    player.Render(renderer, camera);

                    SDL.SDL_RenderPresent(renderer);
                }

                Console.WriteLine("Cleaning up...");
                SDL.SDL_DestroyRenderer(renderer);
                SDL.SDL_DestroyWindow(window);
                SDL.SDL_Quit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhandled exception: {ex.Message}");
            }
        }
    }
}
