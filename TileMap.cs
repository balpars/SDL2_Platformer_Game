using System;
using SDL2;
using System.IO;
using Newtonsoft.Json;

namespace Platformer_Game
{
    public class TileMap
    {
        private int[,] mapData;
        private int mapWidth;
        private int mapHeight;
        private int tileWidth;
        private int tileHeight;
        private IntPtr tilesetTexture;
        private int tilesetWidth;
        private int tilesetHeight;

        public void LoadMap(string filePath)
        {
            string json = File.ReadAllText(filePath);
            dynamic map = JsonConvert.DeserializeObject(json);

            mapWidth = map.width;
            mapHeight = map.height;
            tileWidth = map.tilewidth;
            tileHeight = map.tileheight;

            mapData = new int[mapHeight, mapWidth];

            int[] data = map.layers[0].data.ToObject<int[]>();

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    mapData[y, x] = data[y * mapWidth + x];
                }
            }

            Console.WriteLine($"Tile Map Loaded: Width={mapWidth}, Height={mapHeight}, TileWidth={tileWidth}, TileHeight={tileHeight}");
        }

        public void LoadTileset(IntPtr renderer, string filePath)
        {
            tilesetTexture = SDL_image.IMG_LoadTexture(renderer, filePath);
            if (tilesetTexture == IntPtr.Zero)
            {
                Console.WriteLine($"Failed to load texture! SDL_Error: {SDL.SDL_GetError()}");
                return;
            }

            SDL.SDL_QueryTexture(tilesetTexture, out _, out _, out tilesetWidth, out tilesetHeight);
            Console.WriteLine($"Tileset Loaded: Width={tilesetWidth}, Height={tilesetHeight}");
        }

        public void Render(IntPtr renderer, int cameraX)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    int tileIndex = mapData[y, x] - 1; // Tile indices in the map are 1-based

                    if (tileIndex >= 0)
                    {
                        int tilesPerRow = tilesetWidth / tileWidth;
                        int srcX = (tileIndex % tilesPerRow) * tileWidth;
                        int srcY = (tileIndex / tilesPerRow) * tileHeight;

                        SDL.SDL_Rect srcRect = new SDL.SDL_Rect { x = srcX, y = srcY, w = tileWidth, h = tileHeight };
                        SDL.SDL_Rect dstRect = new SDL.SDL_Rect { x = x * tileWidth - cameraX, y = y * tileHeight, w = tileWidth, h = tileHeight };

                        SDL.SDL_RenderCopy(renderer, tilesetTexture, ref srcRect, ref dstRect);
                    }
                }
            }
        }
    }
}
