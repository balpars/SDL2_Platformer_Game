using SDL2;
using System;

namespace Platformer_Game
{
    public class Game
    {
        private const int ScreenWidth = 800;
        private const int ScreenHeight = 600;

        private IntPtr window;
        private IntPtr renderer;
        private bool running;
        private Player player;
        private TileMap tileMap;
        private int cameraX;

        public Game()
        {
            window = IntPtr.Zero;
            renderer = IntPtr.Zero;
            running = true;
            player = new Player();
            tileMap = new TileMap();
            cameraX = 0;
        }

        public void Initialize()
        {
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
            {
                Console.WriteLine("SDL could not initialize! SDL_Error: " + SDL.SDL_GetError());
                running = false;
            }
            else
            {
                window = SDL.SDL_CreateWindow("Platformer Game", SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, ScreenWidth, ScreenHeight, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);
                if (window == IntPtr.Zero)
                {
                    Console.WriteLine("Window could not be created! SDL_Error: " + SDL.SDL_GetError());
                    running = false;
                }
                else
                {
                    renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
                    if (renderer == IntPtr.Zero)
                    {
                        Console.WriteLine("Renderer could not be created! SDL_Error: " + SDL.SDL_GetError());
                        running = false;
                    }
                    else
                    {
                        player.LoadContent(renderer);
                        tileMap.LoadMap("Assets/map.json");
                        tileMap.LoadTileset(renderer, "Assets/world_tileset.png");
                    }
                }
            }
        }

        public void Run()
        {
            Initialize();

            SDL.SDL_Event e;
            float deltaTime = 0;
            uint lastTime = SDL.SDL_GetTicks();

            while (running)
            {
                while (SDL.SDL_PollEvent(out e) != 0)
                {
                    if (e.type == SDL.SDL_EventType.SDL_QUIT)
                    {
                        running = false;
                    }
                }

                uint currentTime = SDL.SDL_GetTicks();
                deltaTime = (currentTime - lastTime) / 1000.0f;
                lastTime = currentTime;

                Update(deltaTime);
                Render();
            }

            Cleanup();
        }

        public void Update(float deltaTime)
        {
            player.Update(deltaTime);
            UpdateCamera();
        }

        private void UpdateCamera()
        {
            cameraX = (int)(player.PositionX - (ScreenWidth / 2) + (player.FrameWidth / 2));
        }

        public void Render()
        {
            SDL.SDL_RenderClear(renderer);

            tileMap.Render(renderer, cameraX);
            player.Render(renderer, ScreenWidth / 2 - player.FrameWidth / 2, ScreenHeight - player.FrameHeight);

            SDL.SDL_RenderPresent(renderer);
        }

        public void Cleanup()
        {
            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_DestroyWindow(window);
            SDL.SDL_Quit();
        }
    }
}
