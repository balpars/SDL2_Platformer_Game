using System;
using System.IO;
using Newtonsoft.Json;
using SDL2;
using static SDL2.SDL_mixer;

namespace Platformer_Game
{
    public static class Initializer
    {
        public static (IntPtr window, IntPtr renderer, TileLoader tileLoader, dynamic mapData, Player player, Camera camera, CollisionManager collisionManager, SoundManager soundManager) Init()
        {
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO | SDL.SDL_INIT_AUDIO) < 0)
            {
                throw new Exception($"Failed to initialize SDL: {SDL.SDL_GetError()}");
            }

            if (Mix_OpenAudio(44100, MIX_DEFAULT_FORMAT, 2, 2048) < 0)
            {
                throw new Exception($"Failed to initialize SDL_mixer: {SDL.SDL_GetError()}");
            }

            IntPtr window = SDL.SDL_CreateWindow("Tile Loader",
                SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED, 800, 600, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);

            if (window == IntPtr.Zero)
            {
                SDL.SDL_Quit();
                throw new Exception($"Failed to create window: {SDL.SDL_GetError()}");
            }

            IntPtr renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);

            if (renderer == IntPtr.Zero)
            {
                SDL.SDL_DestroyWindow(window);
                SDL.SDL_Quit();
                throw new Exception($"Failed to create renderer: {SDL.SDL_GetError()}");
            }

            TileLoader tileLoader = new TileLoader();

            // Load the map file
            string mapFilePath = "Assets/Map/demo_map.json";
            var mapJson = File.ReadAllText(mapFilePath);
            dynamic mapData = JsonConvert.DeserializeObject(mapJson);

            // Load the tileset
            string tilesetImagePath = "Assets/Map/world_tileset.png";
            int tileWidth = mapData.tilewidth;
            int tileHeight = mapData.tileheight;

            tileLoader.LoadTileset(tileWidth, tileHeight, tilesetImagePath, renderer);

            // Initialize player
            var playerSpawnPoint = tileLoader.GetPlayerSpawnPoint(mapData);
            int spawnX = (int)playerSpawnPoint.Item1;
            int spawnY = (int)playerSpawnPoint.Item2-25 ; // Adjust the Y coordinate to spawn the player higher

            SoundManager soundManager = new SoundManager();
            soundManager.LoadContent();

            Player player = new Player(spawnX, spawnY, 120, 80, renderer, soundManager);

            // Initialize camera
            Camera camera = new Camera(800, 600); // Assuming the screen size is 800x600
            camera.SetTarget(player);
            camera.Smoothing = 2.0f; // Set to 1 for immediate follow
            camera.Zoom = 2.0f; // Zoom factor, adjust as needed

            // Initialize collision manager
            tileLoader.GenerateCollisionRectangles(mapData);
            CollisionManager collisionManager = new CollisionManager(tileLoader.CollisionRectangles);

            player.LoadContent();

            return (window, renderer, tileLoader, mapData, player, camera, collisionManager, soundManager);
        }
    }
}
