using SDL2;
using System;

namespace Platformer_Game
{
    public class MainMenu
    {
        private IntPtr renderer;
        private IntPtr font;
        private SDL.SDL_Rect playButton;
        private SDL.SDL_Rect audioButton;
        private SDL.SDL_Rect settingsButton;
        private SDL.SDL_Rect quitButton;
        private bool audioOn;
        private IntPtr backgroundTexture;

        public MainMenu(IntPtr renderer, IntPtr font)
        {
            this.renderer = renderer;
            this.font = font;
            playButton = new SDL.SDL_Rect { x = 300, y = 200, w = 200, h = 50 };
            audioButton = new SDL.SDL_Rect { x = 300, y = 300, w = 200, h = 50 };
            settingsButton = new SDL.SDL_Rect { x = 300, y = 400, w = 200, h = 50 };
            quitButton = new SDL.SDL_Rect { x = 300, y = 500, w = 200, h = 50 };
            audioOn = true;
            backgroundTexture = LoadTexture("Assets/Backgrounds/menu_background.png");
        }

        public void Render()
        {
            SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
            SDL.SDL_RenderClear(renderer);

            SDL.SDL_RenderCopy(renderer, backgroundTexture, IntPtr.Zero, IntPtr.Zero);

            RenderButton(playButton, "PLAY");
            RenderButton(audioButton, audioOn ? "AUDIO ON" : "AUDIO OFF");
            RenderButton(settingsButton, "SETTINGS");
            RenderButton(quitButton, "QUIT");

            SDL.SDL_RenderPresent(renderer);
        }

        private void RenderButton(SDL.SDL_Rect button, string text)
        {
            SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);
            SDL.SDL_RenderFillRect(renderer, ref button);

            // Render text in the center of the button
            RenderText(text, button.x + button.w / 2, button.y + button.h / 2);
        }

        private void RenderText(string text, int x, int y)
        {
            IntPtr surface = SDL_ttf.TTF_RenderText_Solid(font, text, new SDL.SDL_Color { r = 0, g = 0, b = 0, a = 255 });
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
            SDL.SDL_Rect destRect = new SDL.SDL_Rect { x = x - textWidth / 2, y = y - textHeight / 2, w = textWidth, h = textHeight };
            SDL.SDL_RenderCopy(renderer, texture, IntPtr.Zero, ref destRect);
            SDL.SDL_DestroyTexture(texture);
        }

        public void HandleInput(SDL.SDL_Event e, ref bool running, ref bool startGame)
        {
            if (e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN)
            {
                int mouseX = e.button.x;
                int mouseY = e.button.y;

                if (IsMouseOverButton(mouseX, mouseY, playButton))
                {
                    startGame = true;
                }
                else if (IsMouseOverButton(mouseX, mouseY, audioButton))
                {
                    audioOn = !audioOn;
                }
                else if (IsMouseOverButton(mouseX, mouseY, settingsButton))
                {
                    // Handle settings
                }
                else if (IsMouseOverButton(mouseX, mouseY, quitButton))
                {
                    running = false;
                }
            }
            else if (e.type == SDL.SDL_EventType.SDL_KEYDOWN && e.key.keysym.sym == SDL.SDL_Keycode.SDLK_RETURN)
            {
                startGame = true;
            }
        }

        private bool IsMouseOverButton(int mouseX, int mouseY, SDL.SDL_Rect button)
        {
            return mouseX > button.x && mouseX < button.x + button.w && mouseY > button.y && mouseY < button.h + button.y;
        }

        private IntPtr LoadTexture(string filePath)
        {

            IntPtr texture = SDL_image.IMG_LoadTexture(renderer, filePath);


            if (texture == IntPtr.Zero)
            {
                Console.WriteLine($"Failed to load texture {filePath}! SDL_Error: {SDL.SDL_GetError()}");
            }
            return texture;
        }

    }
}
