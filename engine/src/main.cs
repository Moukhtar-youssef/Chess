using Raylib_cs;
using Engine.Chess;

namespace Engine
{
    public class Program
    {
        const int ScreenWidth = 1200;
        const int ScreenHeight = 800;

        public static void Main(string[] args)
        {
            Raylib.InitWindow(ScreenWidth, ScreenHeight, "2D Chess Engine");
            Raylib.SetTargetFPS(60);
            Raylib.SetWindowState(ConfigFlags.ResizableWindow);
            Raylib.InitAudioDevice();

            Board chessBoard = new Board();

            while (!Raylib.WindowShouldClose())
            {

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);

                chessBoard.Draw();

                Raylib.EndDrawing();
            }

            Raylib.CloseAudioDevice();
            Raylib.CloseWindow();
        }
    }
}
