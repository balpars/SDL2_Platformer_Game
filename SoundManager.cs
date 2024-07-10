using SDL2;
using System;
using System.Collections.Generic;
using static SDL2.SDL_mixer;

namespace Platformer_Game
{
    public class SoundManager
    {
        private Dictionary<string, IntPtr> sounds;
        private Dictionary<string, int> channels;

        public SoundManager()
        {
            sounds = new Dictionary<string, IntPtr>();
            channels = new Dictionary<string, int>();
        }

        public void LoadContent()
        {
            LoadSound("walk", "Assets/Sounds/walk.mp3");
            LoadSound("sword", "Assets/Sounds/sword.mp3");
        }

        private void LoadSound(string soundName, string filePath)
        {
            IntPtr sound = Mix_LoadWAV(filePath);
            if (sound == IntPtr.Zero)
            {
                Console.WriteLine($"Failed to load sound {filePath}! SDL_Error: {SDL.SDL_GetError()}");
            }
            else
            {
                sounds[soundName] = sound;
            }
        }

        public void PlaySound(string soundName, int repeatCount = 0)
        {
            if (sounds.ContainsKey(soundName))
            {
                int channel = Mix_PlayChannel(-1, sounds[soundName], repeatCount);
                channels[soundName] = channel;
            }
        }

        public void StopSound(string soundName)
        {
            if (channels.ContainsKey(soundName))
            {
                Mix_HaltChannel(channels[soundName]);
                channels.Remove(soundName);
             
            }
        }

        public void Cleanup()
        {
            foreach (var sound in sounds.Values)
            {
                Mix_FreeChunk(sound);
            }
            sounds.Clear();
            channels.Clear();
        }
    }
}
