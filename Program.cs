using SDL2;
using System;
using System.Collections.Generic;

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
                bool debugMode = false; // Debug modu
                bool levelCompleted = false;
                bool levelTransition = false;
                float transitionTimer = 0f;

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
                            debugMode = !debugMode; // P tuşuna basıldığında debug modunu değiştir
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
                                debugMode = !debugMode; // P tuşuna basıldığında debug modunu değiştir
                            }
                        }

                        while (accumulator >= fixedDeltaTime)
                        {
                            IntPtr keyStatePtr = SDL.SDL_GetKeyboardState(out int numKeys);
                            byte[] keyState = new byte[numKeys];
                            System.Runtime.InteropServices.Marshal.Copy(keyStatePtr, keyState, 0, numKeys);

                            if (!levelTransition)
                            {
                                player.Update(fixedDeltaTime, keyState, collisionManager, tileLoader); // Pass tileLoader to player.Update
                                samurai.Update(fixedDeltaTime); // Update Samurai
                                camera.Update(fixedDeltaTime);
                                tileLoader.UpdateCoinAnimation(fixedDeltaTime); // Update coin animation
                                tileLoader.UpdateFlagAnimation(fixedDeltaTime); // Update flag animation

                                HandleCoinCollection(player, tileLoader, soundManager);

                                if (CheckFlagCollision(player, tileLoader.FlagRectangles))
                                {
                                    levelTransition = true;
                                    transitionTimer = 2f; // 2 seconds transition
                                    soundManager.PlaySound("winning");
                                }
                            }
                            else
                            {
                                transitionTimer -= fixedDeltaTime;
                                if (transitionTimer <= 0)
                                {
                                    levelCompleted = true;
                                }
                            }

                            accumulator -= fixedDeltaTime;
                        }

                        SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
                        SDL.SDL_RenderClear(renderer);

                        if (levelCompleted)
                        {
                            RenderLevelComplete(renderer, font);
                        }
                        else
                        {
                            tileLoader.RenderMap(mapData, renderer, camera);
                            player.Render(camera);
                            samurai.Render(camera);

                            if (debugMode)
                            {
                                tileLoader.RenderDebug(renderer, camera);
                                collisionManager.RenderDebug(renderer, camera);
                                player.RenderDebug(renderer, camera);
                                samurai.RenderDebug(renderer, camera);
                            }
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

        private static void HandleCoinCollection(Player player, TileLoader tileLoader, SoundManager soundManager)
        {
            // Create a local copy of the player's rect
            SDL.SDL_Rect playerRect = player.Rect;

            for (int i = 0; i < tileLoader.CoinRectangles.Count; i++)
            {
                var coinRect = tileLoader.CoinRectangles[i];

                if (SDL.SDL_HasIntersection(ref playerRect, ref coinRect) == SDL.SDL_bool.SDL_TRUE)
                {
                    var coinPosition = (coinRect.x / tileLoader.TileWidth, coinRect.y / tileLoader.TileHeight);

                    if (!tileLoader.CollectedCoinPositions.Contains(coinPosition))
                    {
                        tileLoader.CollectedCoinPositions.Add(coinPosition);
                        tileLoader.CoinRectangles.RemoveAt(i); // Remove the coin rectangle
                        soundManager.PlaySound("coin");
                        break; // Exit the loop after removing the coin to avoid invalid index issues
                    }
                }
            }
        }

        private static bool CheckFlagCollision(Player player, List<SDL.SDL_Rect> flagRectangles)
        {
            // Create a local copy of the player's rect
            SDL.SDL_Rect playerRect = player.Rect;

            foreach (var rect in flagRectangles)
            {
                SDL.SDL_Rect flagRect = rect; // Use a local variable
                if (SDL.SDL_HasIntersection(ref playerRect, ref flagRect) == SDL.SDL_bool.SDL_TRUE)
                {
                    return true;
                }
            }
            return false;
        }

        private static void RenderLevelComplete(IntPtr renderer, IntPtr font)
        {
            SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
            SDL.SDL_RenderClear(renderer);

            IntPtr surface = SDL_ttf.TTF_RenderText_Solid(font, "LEVEL 2", new SDL.SDL_Color { r = 255, g = 255, b = 255, a = 255 });
            if (surface == IntPtr.Zero)
            {
                throw new Exception($"Failed to create text surface: {SDL.SDL_GetError()}");
            }

            IntPtr texture = SDL.SDL_CreateTextureFromSurface(renderer, surface);
            SDL.SDL_FreeSurface(surface);

            if (texture == IntPtr.Zero)
            {
                throw new Exception($"Failed to create text texture: {SDL.SDL_GetError()}");
            }

            SDL.SDL_QueryTexture(texture, out _, out _, out int textWidth, out int textHeight);
            SDL.SDL_Rect destRect = new SDL.SDL_Rect { x = 400 - textWidth / 2, y = 300 - textHeight / 2, w = textWidth, h = textHeight };
            SDL.SDL_RenderCopy(renderer, texture, IntPtr.Zero, ref destRect);
            SDL.SDL_DestroyTexture(texture);
        }
    }
}
