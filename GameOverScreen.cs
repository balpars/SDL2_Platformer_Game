using SDL2;

public class GameOverScreen
{
    private IntPtr renderer;
    private IntPtr largeFont;
    private IntPtr smallFont;
    private SDL.SDL_Rect restartButton;
    private SDL.SDL_Rect mainMenuButton;

    public GameOverScreen(IntPtr renderer)
    {
        this.renderer = renderer;
        LoadFonts();
        InitializeButtons();
    }

    private void LoadFonts()
    {
        // Load large font for "GAME OVER"
        largeFont = SDL_ttf.TTF_OpenFont("Assets/Fonts/GreenBerry.ttf", 48);
        if (largeFont == IntPtr.Zero)
        {
            throw new Exception($"Failed to load large font: {SDL.SDL_GetError()}");
        }

        // Load small font for buttons
        smallFont = SDL_ttf.TTF_OpenFont("Assets/Fonts/GreenBerry.ttf", 24);
        if (smallFont == IntPtr.Zero)
        {
            throw new Exception($"Failed to load small font: {SDL.SDL_GetError()}");
        }
    }

    private void InitializeButtons()
    {
        restartButton = new SDL.SDL_Rect { x = 300, y = 300, w = 200, h = 50 };
        mainMenuButton = new SDL.SDL_Rect { x = 300, y = 400, w = 200, h = 50 };
    }

    public void Update(byte[] keyState)
    {
        // Update logic for when the game is over (if needed)
    }

    public void Render()
    {
        SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255); // Black background
        SDL.SDL_RenderClear(renderer);

        // Render "GAME OVER" with the large font
        RenderText("GAME OVER", 400, 200, new SDL.SDL_Color { r = 255, g = 255, b = 255, a = 255 }, largeFont);

        // Render buttons with smaller text
        RenderButton(restartButton, "RESTART", new SDL.SDL_Color { r = 0, g = 0, b = 0, a = 255 }, smallFont); // Button text color black
        RenderButton(mainMenuButton, "MAIN MENU", new SDL.SDL_Color { r = 0, g = 0, b = 0, a = 255 }, smallFont); // Button text color black

        SDL.SDL_RenderPresent(renderer);
    }

    private void RenderButton(SDL.SDL_Rect button, string text, SDL.SDL_Color textColor, IntPtr font)
    {
        SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255); // White button background
        SDL.SDL_RenderFillRect(renderer, ref button);

        RenderText(text, button.x + button.w / 2, button.y + button.h / 2, textColor, font);
    }

    private void RenderText(string text, int x, int y, SDL.SDL_Color textColor, IntPtr font)
    {
        IntPtr surface = SDL_ttf.TTF_RenderText_Solid(font, text, textColor);
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

    public void HandleInput(SDL.SDL_Event e, ref bool running, ref bool restartGame, ref bool goToMainMenu)
    {
        if (e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN)
        {
            int mouseX = e.button.x;
            int mouseY = e.button.y;

            if (IsMouseOverButton(mouseX, mouseY, restartButton))
            {
                restartGame = true;
                Console.WriteLine("Restart");
            }
            else if (IsMouseOverButton(mouseX, mouseY, mainMenuButton))
            {
                goToMainMenu = true;
                Console.WriteLine("Main Menu");
            }
        }
    }

    private bool IsMouseOverButton(int mouseX, int mouseY, SDL.SDL_Rect button)
    {
        return mouseX > button.x && mouseX < button.x + button.w && mouseY > button.y && mouseY < button.y + button.h;
    }

    public void Dispose()
    {
        if (largeFont != IntPtr.Zero)
        {
            SDL_ttf.TTF_CloseFont(largeFont);
            largeFont = IntPtr.Zero;
        }

        if (smallFont != IntPtr.Zero)
        {
            SDL_ttf.TTF_CloseFont(smallFont);
            smallFont = IntPtr.Zero;
        }
    }
}
