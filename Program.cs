using SDL2;
using System;

namespace Platformer_Game
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var (window, renderer, tileLoader, mapData, player, camera, collisionManager) = Initializer.Init();

                // Main loop
                bool running = true;

                // Fixed timestep variables
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

                    // Process fixed update steps
                    while (accumulator >= fixedDeltaTime)
                    {
                        // Get key states
                        IntPtr keyStatePtr = SDL.SDL_GetKeyboardState(out int numKeys);
                        byte[] keyState = new byte[numKeys];
                        System.Runtime.InteropServices.Marshal.Copy(keyStatePtr, keyState, 0, numKeys);

                        // Update game logic here
                        player.Update(fixedDeltaTime, keyState, collisionManager);
                        camera.Update(fixedDeltaTime);

                        accumulator -= fixedDeltaTime;
                    }

                    // Set background color to black
                    SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
                    SDL.SDL_RenderClear(renderer);

                    // Render the map with camera offset
                    tileLoader.RenderMap(mapData, renderer, camera);

                    // Render the player with camera offset
                    player.Render(camera);

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
