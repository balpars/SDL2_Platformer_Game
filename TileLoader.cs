// TileLoader.cs
using SDL2;
using System;
using System.Collections.Generic;

namespace Platformer_Game
{
    public class TileLoader
    {
        public int TileWidth { get; private set; }
        public int TileHeight { get; private set; }
        public Dictionary<int, IntPtr> Tileset { get; private set; }
        public List<SDL.SDL_Rect> CollisionRectangles { get; private set; }
        public List<SDL.SDL_Rect> ClimbingRectangles { get; private set; }

        public TileLoader()
        {
            Tileset = new Dictionary<int, IntPtr>();
            CollisionRectangles = new List<SDL.SDL_Rect>();
            ClimbingRectangles = new List<SDL.SDL_Rect>();
        }

        public void LoadTileset(int tileWidth, int tileHeight, string tilesetImagePath, IntPtr renderer)
        {
            try
            {
                TileWidth = tileWidth;
                TileHeight = tileHeight;

                IntPtr tilesetTexture = LoadTexture(tilesetImagePath, renderer);

                int textureWidth, textureHeight;
                SDL.SDL_QueryTexture(tilesetTexture, out _, out _, out textureWidth, out textureHeight);

                int columns = textureWidth / TileWidth;
                int rows = textureHeight / TileHeight;
                int tileCount = columns * rows;

                for (int i = 0; i < tileCount; i++)
                {
                    int row = i / columns;
                    int col = i % columns;

                    SDL.SDL_Rect tileRect = new SDL.SDL_Rect
                    {
                        x = col * TileWidth,
                        y = row * TileHeight,
                        w = TileWidth,
                        h = TileHeight
                    };

                    Tileset[i + 1] = CreateTileTexture(tilesetTexture, tileRect, renderer);
                }

                SDL.SDL_DestroyTexture(tilesetTexture);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading tileset: {ex.Message}");
            }
        }

        private IntPtr LoadTexture(string filePath, IntPtr renderer)
        {
            IntPtr surface = SDL_image.IMG_Load(filePath);
            if (surface == IntPtr.Zero)
            {
                throw new Exception($"Failed to load image: {SDL.SDL_GetError()}");
            }

            IntPtr texture = SDL.SDL_CreateTextureFromSurface(renderer, surface);
            SDL.SDL_FreeSurface(surface);

            if (texture == IntPtr.Zero)
            {
                throw new Exception($"Failed to create texture: {SDL.SDL_GetError()}");
            }

            // Set the blend mode for transparency
            SDL.SDL_SetTextureBlendMode(texture, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);

            return texture;
        }

        private IntPtr CreateTileTexture(IntPtr tilesetTexture, SDL.SDL_Rect tileRect, IntPtr renderer)
        {
            IntPtr tileTexture = SDL.SDL_CreateTexture(renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
                (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, TileWidth, TileHeight);

            // Set the blend mode for transparency
            SDL.SDL_SetTextureBlendMode(tileTexture, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);

            SDL.SDL_SetRenderTarget(renderer, tileTexture);
            SDL.SDL_RenderCopy(renderer, tilesetTexture, ref tileRect, IntPtr.Zero);
            SDL.SDL_SetRenderTarget(renderer, IntPtr.Zero);

            return tileTexture;
        }

        public void RenderMap(dynamic mapData, IntPtr renderer, Camera camera)
        {
            try
            {
                int mapWidth = mapData.width;
                int mapHeight = mapData.height;

                // Render the BackgroundLayer first
                foreach (var layer in mapData.layers)
                {
                    if (layer.type == "tilelayer" && layer.name == "BackgroundLayer")
                    {
                        RenderLayer(layer, mapWidth, mapHeight, renderer, camera);
                    }
                }

                // Render other layers except BackgroundLayer
                foreach (var layer in mapData.layers)
                {
                    if (layer.type == "tilelayer" && layer.name != "BackgroundLayer")
                    {
                        RenderLayer(layer, mapWidth, mapHeight, renderer, camera);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error rendering map: {ex.Message}");
            }
        }

        private void RenderLayer(dynamic layer, int mapWidth, int mapHeight, IntPtr renderer, Camera camera)
        {
            int[] tileIds = layer.data.ToObject<int[]>();

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    int tileId = tileIds[y * mapWidth + x];

                    if (tileId == 0 || !Tileset.ContainsKey(tileId))
                    {
                        continue;
                    }

                    SDL.SDL_Rect destRect = new SDL.SDL_Rect
                    {
                        x = x * TileWidth,
                        y = y * TileHeight,
                        w = TileWidth,
                        h = TileHeight
                    };

                    SDL.SDL_Rect renderRect = camera.GetRenderRect(destRect);

                    SDL.SDL_RenderCopy(renderer, Tileset[tileId], IntPtr.Zero, ref renderRect);
                }
            }
        }

        public void GenerateCollisionRectangles(dynamic mapData)
        {
            try
            {
                int mapWidth = mapData.width;
                int mapHeight = mapData.height;

                foreach (var layer in mapData.layers)
                {
                    string layerType = (string)layer.type;
                    string layerName = (string)layer.name;

                    if (layerType == "tilelayer" && layerName == "CollisionLayer")
                    {
                        int[] tileIds = layer.data.ToObject<int[]>();

                        for (int y = 0; y < mapHeight; y++)
                        {
                            for (int x = 0; x < mapWidth; x++)
                            {
                                int tileId = tileIds[y * mapWidth + x];

                                if (tileId == 0 || !Tileset.ContainsKey(tileId))
                                {
                                    continue;
                                }

                                SDL.SDL_Rect destRect = new SDL.SDL_Rect
                                {
                                    x = x * TileWidth,
                                    y = y * TileHeight,
                                    w = TileWidth,
                                    h = TileHeight
                                };

                                CollisionRectangles.Add(destRect);
                            }
                        }
                    }
                    else if (layerType == "tilelayer" && layerName == "ClimbingLayer")
                    {
                        int[] tileIds = layer.data.ToObject<int[]>();

                        for (int y = 0; y < mapHeight; y++)
                        {
                            for (int x = 0; x < mapWidth; x++)
                            {
                                int tileId = tileIds[y * mapWidth + x];

                                if (tileId == 0 || !Tileset.ContainsKey(tileId))
                                {
                                    continue;
                                }

                                SDL.SDL_Rect destRect = new SDL.SDL_Rect
                                {
                                    x = x * TileWidth,
                                    y = y * TileHeight,
                                    w = TileWidth,
                                    h = TileHeight
                                };

                                ClimbingRectangles.Add(destRect); // Yeni eklenen tırmanma kareleri
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating collision rectangles: {ex.Message}");
            }
        }

        public void RenderDebug(IntPtr renderer, Camera camera)
        {
            SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255); // Siyah renk

            foreach (var rect in CollisionRectangles)
            {
                SDL.SDL_Rect renderRect = camera.GetRenderRect(rect);
                SDL.SDL_RenderDrawRect(renderer, ref renderRect);
            }

            foreach (var rect in ClimbingRectangles)
            {
                SDL.SDL_Rect renderRect = camera.GetRenderRect(rect);
                SDL.SDL_RenderDrawRect(renderer, ref renderRect);
            }
        }

        public (int, int) GetPlayerSpawnPoint(dynamic mapData)
        {
            foreach (var layer in mapData.layers)
            {
                string layerType = (string)layer.type;
                if (layerType == "objectgroup")
                {
                    foreach (var obj in layer.objects)
                    {
                        foreach (var property in obj.properties)
                        {
                            if ((string)property.name == "Type" && (string)property.value == "PlayerSpawn")
                            {
                                int x = (int)obj.x;
                                int y = (int)obj.y;
                                return (x, y);
                            }
                        }
                    }
                }
            }

            return (0, 0); // Default spawn point if not found
        }
    }
}
