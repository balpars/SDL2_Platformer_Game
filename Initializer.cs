using System;
using System.IO;
using Newtonsoft.Json;
using SDL2;

namespace Platformer_Game
{
    public static class Initializer
    {
        public static (IntPtr window, IntPtr renderer, TileLoader tileLoader, dynamic mapData, Player player, Camera camera, CollisionManager collisionManager) Init()
        {
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
            {
                throw new Exception($"Failed to initialize SDL: {SDL.SDL_GetError()}");
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

            string mapFilePath = "Assets/Map/demo_map.json";
            var mapJson = File.ReadAllText(mapFilePath);
            dynamic mapData = JsonConvert.DeserializeObject(mapJson);

            string tilesetImagePath = "Assets/Map/world_tileset.png";
            int tileWidth = mapData.tilewidth;
            int tileHeight = mapData.tileheight;

            tileLoader.LoadTileset(tileWidth, tileHeight, tilesetImagePath, renderer);

            var playerSpawnPoint = tileLoader.GetPlayerSpawnPoint(mapData);
            Player player = new Player((int)playerSpawnPoint.Item1, (int)playerSpawnPoint.Item2, 120, 80, 200, renderer);
            player.LoadContent(renderer);

            Camera camera = new Camera(800, 600);
            camera.SetTarget(player);
            camera.Smoothing = 2.0f;
            camera.Zoom = 2.0f;

            tileLoader.GenerateCollisionRectangles(mapData);
            CollisionManager collisionManager = new CollisionManager(tileLoader.CollisionRectangles);

            return (window, renderer, tileLoader, mapData, player, camera, collisionManager);
        }
    }
}
