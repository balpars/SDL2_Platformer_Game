// SamuraiMovementManager.cs
using SDL2;

namespace Platformer_Game
{
    public class SamuraiMovementManager
    {
        private const float RunSpeed = 50f; // Speed of the samurai when running
        private const float Gravity = 0.3f; // Gravity

        public void MoveSamurai(float deltaTime, ref SDL.SDL_Rect rect, SamuraiState currentState)
        {
            if (currentState == SamuraiState.Running)
            {
                float movementAmount = RunSpeed * deltaTime;
                rect.x -= (int)movementAmount; // Move left while running

                // Debugging: Print the amount moved
                //Console.WriteLine($"Moving Samurai left by: {movementAmount} units (RunSpeed: {RunSpeed}, deltaTime: {deltaTime})");
            }
        }

        public void UpdatePosition(float deltaTime, ref SDL.SDL_Rect rect, ref SamuraiState currentState)
        {
            // Implement any additional logic here for updating the position
            // For example, handle gravity or other state-specific movements
            if (currentState == SamuraiState.Running)
            {
                // Apply gravity if necessary (example logic)
                rect.y += (int)(Gravity * 100 * deltaTime);
            }

            // Debugging: Print the new position
            //Console.WriteLine($"Updated Samurai Position: ({rect.x}, {rect.y})");
        }
    }
}
