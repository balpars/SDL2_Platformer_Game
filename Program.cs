// Program.cs
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
                var (window, renderer, tileLoader, mapData, player, samurai, camera, collisionManager, soundManager, font) = Initializer.Init();

                bool running = true;
                bool startGame = false;
                bool debugMode = false; // Debug mode

                MainMenu mainMenu = new MainMenu(renderer, font);

                while (running)
                {
                    SDL.SDL_Event e;
                    while (SDL.SDL_PollEvent(out e) != 0)
                    {
                        if (e.type == SDL.SDL_EventType.SDL_QUIT)
                        {
                            running = false;
                        }
                        else if (!startGame)
                        {
                            mainMenu.HandleInput(e, ref running, ref startGame);
                        }
                        else if (e.type == SDL.SDL_EventType.SDL_KEYDOWN && e.key.keysym.sym == SDL.SDL_Keycode.SDLK_p)
                        {
                            debugMode = !debugMode; // Toggle debug mode with P key
                        }
                    }

                    if (!startGame)
                    {
                        mainMenu.Render();
                        continue;
                    }

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

                        while (SDL.SDL_PollEvent(out e) != 0)
                        {
                            if (e.type == SDL.SDL_EventType.SDL_QUIT)
                            {
                                running = false;
                            }
                            else if (e.type == SDL.SDL_EventType.SDL_KEYDOWN && e.key.keysym.sym == SDL.SDL_Keycode.SDLK_p)
                            {
                                debugMode = !debugMode; // Toggle debug mode with P key
                            }
                        }

                        while (accumulator >= fixedDeltaTime)
                        {
                            IntPtr keyStatePtr = SDL.SDL_GetKeyboardState(out int numKeys);
                            byte[] keyState = new byte[numKeys];
                            System.Runtime.InteropServices.Marshal.Copy(keyStatePtr, keyState, 0, numKeys);

                            player.Update(fixedDeltaTime, keyState, collisionManager, tileLoader); // Update Player with TileLoader
                            samurai.Update(fixedDeltaTime); // Update Samurai state
                            camera.Update(fixedDeltaTime);

                            accumulator -= fixedDeltaTime;
                        }

                        SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
                        SDL.SDL_RenderClear(renderer);

                        tileLoader.RenderMap(mapData, renderer, camera);
                        player.Render(camera);
                        samurai.Render(camera); // Render Samurai

                        if (debugMode)
                        {
                            tileLoader.RenderDebug(renderer, camera);
                            collisionManager.RenderDebug(renderer, camera);
                            player.RenderDebug(renderer, camera);
                            samurai.RenderDebug(renderer, camera); // Render Samurai debug info
                        }

                        SDL.SDL_RenderPresent(renderer);
                    }

                    Console.WriteLine("Cleaning up...");
                    soundManager.Cleanup();
                    SDL.SDL_DestroyRenderer(renderer);
                    SDL.SDL_DestroyWindow(window);
                    SDL.SDL_Quit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhandled exception: {ex.Message}");
            }
        }
    }
}
