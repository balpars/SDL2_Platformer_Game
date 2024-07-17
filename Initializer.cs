// Initializer.cs
using SDL2;
using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using static SDL2.SDL_mixer;
using static SDL2.SDL_ttf;

namespace Platformer_Game
{
    public static class Initializer
    {
        public static (IntPtr window, IntPtr renderer, TileLoader tileLoader, dynamic mapData, Player player, Samurai samurai, Camera camera, CollisionManager collisionManager, SoundManager soundManager, IntPtr font) Init()
        {
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO | SDL.SDL_INIT_AUDIO) < 0)
            {
                throw new Exception($"Failed to initialize SDL: {SDL.SDL_GetError()}");
            }

            if (Mix_OpenAudio(44100, MIX_DEFAULT_FORMAT, 2, 2048) < 0)
            {
                throw new Exception($"Failed to initialize SDL_mixer: {SDL.SDL_GetError()}");
            }

            if (TTF_Init() < 0)
            {
                throw new Exception($"Failed to initialize SDL_ttf: {SDL.SDL_GetError()}");
            }

            IntPtr font = TTF_OpenFont("Assets/Fonts/GreenBerry.ttf", 24);
            if (font == IntPtr.Zero)
            {
                throw new Exception($"Failed to load font: {SDL.SDL_GetError()}");
            }

            IntPtr window = SDL.SDL_CreateWindow("Knight Offline",
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
            int spawnX = (int)playerSpawnPoint.Item1;
            int spawnY = (int)playerSpawnPoint.Item2 - 25;

            SoundManager soundManager = new SoundManager();
            soundManager.LoadContent();

            Player player = new Player(spawnX, spawnY, 20, 40, renderer, soundManager);
            Samurai samurai = new Samurai(spawnX + 145, spawnY + 38, 93, 50, renderer, soundManager); // Adjust Samurai y-coordinate by +20

            Camera camera = new Camera(800, 600);
            camera.SetTarget(player);
            camera.Smoothing = 2.0f;
            camera.Zoom = 2.0f;

            tileLoader.GenerateCollisionRectangles(mapData);
            List<SDL.SDL_Rect> collisionRectangles = tileLoader.CollisionRectangles;
            List<SDL.SDL_Rect> climbingRectangles = tileLoader.ClimbingRectangles;

            CollisionManager collisionManager = new CollisionManager(collisionRectangles, climbingRectangles);

            player.LoadContent();
            samurai.LoadContent(); // Load Samurai content

            return (window, renderer, tileLoader, mapData, player, samurai, camera, collisionManager, soundManager, font);
        }
    }
}
