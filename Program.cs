using SDL2;
using System;

namespace Platformer_Game
{
    class Program
    {
        enum GameState
        {
            MainMenu,
            Playing,
            GameOver,
            Level2Screen
        }

        static void Main(string[] args)
        {
            try
            {
                if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO | SDL.SDL_INIT_AUDIO) < 0)
                {
                    throw new Exception($"SDL could not initialize! SDL_Error: {SDL.SDL_GetError()}");
                }

                if (SDL_ttf.TTF_Init() == -1)
                {
                    throw new Exception($"SDL_ttf could not initialize!");
                }

                var (window, renderer, tileLoader, mapData, player, samurai, camera, collisionManager, soundManager, font) = Initializer.Init();

                bool running = true;
                bool startGame = false;
                bool debugMode = false;
                bool levelCompleted = false;
                bool levelTransition = false;
                float transitionTimer = 0f;
                bool showLevel2Screen = false;
                float level2ScreenTimer = 0f;

                MainMenu mainMenu = new MainMenu(renderer, font);
                GameOverScreen gameOverScreen = new GameOverScreen(renderer);

                GameState gameState = GameState.MainMenu;

                const float targetFps = 60.0f;
                const float fixedDeltaTime = 1.0f / targetFps;
                float accumulator = 0.0f;
                uint previousTime = SDL.SDL_GetTicks();

                while (running)
                {
                    SDL.SDL_Event e;
                    while (SDL.SDL_PollEvent(out e) != 0)
                    {
                        if (e.type == SDL.SDL_EventType.SDL_QUIT)
                        {
                            running = false;
                        }
                        else if (gameState == GameState.MainMenu)
                        {
                            mainMenu.HandleInput(e, ref running, ref startGame);
                            if (startGame)
                            {
                                soundManager.PlaySound("start");
                                soundManager.PlaySoundLoop("ambiance");
                                LoadLevel1(renderer, ref tileLoader, ref mapData, ref player, ref samurai, ref collisionManager, ref camera, soundManager);
                                gameState = GameState.Playing;
                                startGame = false;
                            }
                        }
                        else if (gameState == GameState.Playing)
                        {
                            if (e.type == SDL.SDL_EventType.SDL_KEYDOWN && e.key.keysym.sym == SDL.SDL_Keycode.SDLK_p)
                            {
                                debugMode = !debugMode;
                            }
                        }
                        else if (gameState == GameState.GameOver)
                        {
                            bool restartGame = false;
                            bool goToMainMenu = false;
                            gameOverScreen.HandleInput(e, ref running, ref restartGame, ref goToMainMenu);
                            if (restartGame)
                            {
                                soundManager.PlaySound("start");
                                soundManager.PlaySoundLoop("ambiance");
                                // Reset and reload level 1
                                LoadLevel1(renderer, ref tileLoader, ref mapData, ref player, ref samurai, ref collisionManager, ref camera, soundManager);
                                gameState = GameState.Playing;
                                startGame = false;
                            }
                            else if (goToMainMenu)
                            {

                                gameState = GameState.MainMenu;
                                soundManager.StopSound("ambiance");
                            }
                        }
                    }

                    uint currentTime = SDL.SDL_GetTicks();
                    float deltaTime = (currentTime - previousTime) / 1000.0f;
                    previousTime = currentTime;
                    accumulator += deltaTime;

                    while (accumulator >= fixedDeltaTime)
                    {
                        if (gameState == GameState.Playing)
                        {
                            IntPtr keyStatePtr = SDL.SDL_GetKeyboardState(out int numKeys);
                            byte[] keyState = new byte[numKeys];
                            System.Runtime.InteropServices.Marshal.Copy(keyStatePtr, keyState, 0, numKeys);

                            if (!levelTransition && !showLevel2Screen)
                            {
                                player.Update(fixedDeltaTime, keyState, collisionManager, tileLoader, samurai);
                                samurai.Update(fixedDeltaTime, collisionManager, player);
                                camera.Update(fixedDeltaTime);
                                tileLoader.UpdateCoinAnimation(fixedDeltaTime);
                                tileLoader.UpdateFlagAnimation(fixedDeltaTime);

                                if (CheckFlagCollision(player, tileLoader.FlagRectangles))
                                {
                                    levelTransition = true;
                                    transitionTimer = 2f;
                                    soundManager.PlaySound("winning");
                                }

                                if (player.IsDead())
                                {
                                    gameState = GameState.GameOver;
                                }
                            }
                            else if (levelTransition)
                            {
                                transitionTimer -= fixedDeltaTime;
                                if (transitionTimer <= 0)
                                {
                                    levelCompleted = true;
                                    showLevel2Screen = true;
                                    level2ScreenTimer = 3f; // Delay before loading Level 2
                                    levelTransition = false;
                                    gameState = GameState.Level2Screen;
                                }
                            }
                        }
                        else if (gameState == GameState.Level2Screen)
                        {
                            level2ScreenTimer -= fixedDeltaTime;
                            if (level2ScreenTimer <= 0)
                            {
                                LoadLevel2(renderer, ref tileLoader, ref mapData, ref player, ref samurai, ref collisionManager, ref camera, soundManager);
                                gameState = GameState.Playing;
                                showLevel2Screen = false;
                                levelCompleted = false;
                            }
                        }

                        accumulator -= fixedDeltaTime;
                    }

                    SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
                    SDL.SDL_RenderClear(renderer);

                    if (gameState == GameState.MainMenu)
                    {
                        mainMenu.Render();
                    }
                    else if (gameState == GameState.GameOver)
                    {
                        gameOverScreen.Render();
                    }
                    else if (gameState == GameState.Playing)
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
                    else if (gameState == GameState.Level2Screen)
                    {
                        RenderLevelComplete(renderer, font);
                    }

                    SDL.SDL_RenderPresent(renderer);
                }

                Cleanup(renderer, window, soundManager);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhandled exception: {ex.Message}");
            }
        }

        private static void LoadLevel1(IntPtr renderer, ref TileLoader tileLoader, ref dynamic mapData, ref Player player, ref Samurai samurai, ref CollisionManager collisionManager, ref Camera camera, SoundManager soundManager)
        {
            // Reload map data
            string mapFilePath = "Assets/Map/demo_map.json";
            var mapJson = System.IO.File.ReadAllText(mapFilePath);
            mapData = Newtonsoft.Json.JsonConvert.DeserializeObject(mapJson);

            tileLoader = new TileLoader();
            tileLoader.LoadCoinSpritesheet("Assets/Map/coin.png", renderer);
            tileLoader.LoadFlagSpritesheet("Assets/Map/flag.png", renderer);

            int tileWidth = Convert.ToInt32(mapData.tilewidth);
            int tileHeight = Convert.ToInt32(mapData.tileheight);
            tileLoader.LoadTileset(tileWidth, tileHeight, "Assets/Map/world_tileset.png", renderer);
            tileLoader.GenerateCollisionRectangles(mapData);

            var playerSpawnPoint = tileLoader.GetPlayerSpawnPoint(mapData);
            int spawnX = (int)playerSpawnPoint.Item1;
            int spawnY = (int)playerSpawnPoint.Item2 - 25;

            player = new Player(spawnX, spawnY, 20, 40, renderer, soundManager);
            samurai = new Samurai(spawnX + 145, spawnY + 38, 20, 40, renderer, soundManager);

            collisionManager = new CollisionManager(tileLoader.CollisionRectangles, tileLoader.ClimbingRectangles);
            camera.SetTarget(player);

            player.LoadContent();
            samurai.LoadContent();
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

        private static bool CheckFlagCollision(Player player, List<SDL.SDL_Rect> flagRectangles)
        {
            SDL.SDL_Rect playerRect = player.Rect;

            foreach (var rect in flagRectangles)
            {
                SDL.SDL_Rect flagRect = rect;
                if (SDL.SDL_HasIntersection(ref playerRect, ref flagRect) == SDL.SDL_bool.SDL_TRUE)
                {
                    return true;
                }
            }
            return false;
        }

        private static void LoadLevel2(IntPtr renderer, ref TileLoader tileLoader, ref dynamic mapData, ref Player player, ref Samurai samurai, ref CollisionManager collisionManager, ref Camera camera, SoundManager soundManager)
        {
            string mapFilePath = "Assets/Map/demo_map2.json";
            var mapJson = System.IO.File.ReadAllText(mapFilePath);
            mapData = Newtonsoft.Json.JsonConvert.DeserializeObject(mapJson);

            tileLoader = new TileLoader();
            tileLoader.LoadCoinSpritesheet("Assets/Map/coin.png", renderer);
            tileLoader.LoadFlagSpritesheet("Assets/Map/flag.png", renderer);

            int tileWidth = Convert.ToInt32(mapData.tilewidth);
            int tileHeight = Convert.ToInt32(mapData.tileheight);
            tileLoader.LoadTileset(tileWidth, tileHeight, "Assets/Map/world_tileset.png", renderer);
            tileLoader.GenerateCollisionRectangles(mapData);

            var playerSpawnPoint = tileLoader.GetPlayerSpawnPoint(mapData);
            int spawnX = (int)playerSpawnPoint.Item1;
            int spawnY = (int)playerSpawnPoint.Item2 - 25;

            player = new Player(spawnX, spawnY, 20, 40, renderer, soundManager);
            samurai = new Samurai(spawnX + 145, spawnY + 38, 20, 40, renderer, soundManager);

            collisionManager = new CollisionManager(tileLoader.CollisionRectangles, tileLoader.ClimbingRectangles);
            camera.SetTarget(player);

            player.LoadContent();
            samurai.LoadContent();
        }

        private static void Cleanup(IntPtr renderer, IntPtr window, SoundManager soundManager)
        {
            soundManager.Cleanup();
            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_DestroyWindow(window);
            SDL_ttf.TTF_Quit();
            SDL.SDL_Quit();
        }
    }
}
