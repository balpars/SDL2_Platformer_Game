using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace Demo
{
    public class MapLoader
    {
        public int TileWidth { get; private set; }
        public int TileHeight { get; private set; }
        public int MapWidth { get; private set; }
        public int MapHeight { get; private set; }
        public int[] TileGIDs { get; private set; }

        public void LoadMap(string jsonPath)
        {
            string jsonString = File.ReadAllText(jsonPath);
            JObject mapData = JObject.Parse(jsonString);

            MapWidth = (int)mapData["width"];
            MapHeight = (int)mapData["height"];
            TileWidth = (int)mapData["tilewidth"];
            TileHeight = (int)mapData["tileheight"];

            JArray layers = (JArray)mapData["layers"];
            TileGIDs = layers[0]["data"].ToObject<int[]>();
        }
    }
}
