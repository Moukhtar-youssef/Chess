namespace Engine.Chess
{
    public static class Pieces
    {
        public const int None = 0;
        public const int Pawn = 1;
        public const int Knight = 2;
        public const int Bishop = 3;
        public const int Rook = 4;
        public const int Queen = 5;
        public const int King = 6;

        public const int White = 0;
        public const int Black = 8;

        public const int WhitePawn = Pawn | White;
        public const int WhiteKnight = Knight | White;
        public const int WhiteBishop = Bishop | White;
        public const int WhiteRook = Rook | White;
        public const int WhiteQueen = Queen | White;
        public const int WhiteKing = King | White;

        public const int BlackPawn = Pawn | Black;
        public const int BlackKnight = Knight | Black;
        public const int BlackBishop = Bishop | Black;
        public const int BlackRook = Rook | Black;
        public const int BlackQueen = Queen | Black;
        public const int BlackKing = King | Black;

        const int typeMask = 0b0111;
        const int colourMask = 0b1000;

        // Returns true if given piece matches the given colour. If piece is of type 'none', result will always be false.
        public static bool IsColour(int piece, int colour) => piece != None && PieceColour(piece) == colour;

        public static bool IsWhite(int piece) => IsColour(piece, White);

        public static int PieceColour(int piece) => piece & colourMask;

        public static int PieceType(int piece) => piece & typeMask;

        public static bool IsOrthogonalSlider(int piece) => PieceType(piece) is Queen or Rook;

        public static bool IsDiagonalSlider(int piece) => PieceType(piece) is Queen or Bishop;

        public static bool IsSlidingPiece(int piece) => PieceType(piece) is Queen or Bishop or Rook;

        public static int OpponentColor(int color) => color == Pieces.White ? Pieces.Black : Pieces.White;

        public static bool IsEnemy(int pieceA, int pieceB)
        {
            return pieceA != None &&
                   pieceB != None &&
                   PieceColour(pieceA) != PieceColour(pieceB);
        }

        public static bool IsFriendly(int pieceA, int pieceB)
        {
            return pieceA != None &&
                   pieceB != None &&
                   PieceColour(pieceA) == PieceColour(pieceB);
        }
    }
}
