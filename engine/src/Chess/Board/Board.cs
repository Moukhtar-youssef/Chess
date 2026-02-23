using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;

namespace Engine.Chess
{
    public class Board
    {
        public int[] Squares;

        struct BoardLayout
        {
            public int Size;
            public int OffsetX;
            public int OffsetY;
            public int SquareSize;
        }

        BoardLayout Layout;

        // Sprite sheet
        private Texture2D pieceSpriteSheet;
        private Dictionary<int, Rectangle> pieceSourceRects = new Dictionary<int, Rectangle>();

        // Drag and drop
        private int? selectedSquare = null;
        List<int>? legalMoves = null;
        private int dragOffsetX = 0;
        private int dragOffsetY = 0;

        public bool IsWhiteToMove = true;
        public int MoveColour => IsWhiteToMove ? Pieces.White : Pieces.Black;
        private int? enPassantTargetSquare = null;
        private static bool whiteCanCastleKingside = true;
        private static bool whiteCanCastleQueenside = true;
        private static bool blackCanCastleKingside = true;
        private static bool blackCanCastleQueenside = true;

        // Sounds
        private Sound moveSound;
        private Sound captureSound;
        private Sound checkSound;




        public Board()
        {
            Squares = new int[64];
            FenParser.LoadPositionFromFen(FenParser.StartPositionFEN, this);

            LoadPieceSpriteSheet();
            MapPiecesFromSprite();

            moveSound = Raylib.LoadSound("Assets/move.wav");
            captureSound = Raylib.LoadSound("Assets/capture.wav");
            checkSound = Raylib.LoadSound("Assets/check.wav");
        }

        private void LoadPieceSpriteSheet()
        {
            pieceSpriteSheet = Raylib.LoadTexture("Assets/pieces.png");
        }

        private void MapPiecesFromSprite()
        {
            int spriteWidth = pieceSpriteSheet.Width / 6;
            int spriteHeight = pieceSpriteSheet.Height / 2;

            // White pieces
            pieceSourceRects[Pieces.White | Pieces.King] = new Rectangle(0 * spriteWidth, 0, spriteWidth, spriteHeight);
            pieceSourceRects[Pieces.White | Pieces.Queen] = new Rectangle(1 * spriteWidth, 0, spriteWidth, spriteHeight);
            pieceSourceRects[Pieces.White | Pieces.Bishop] = new Rectangle(2 * spriteWidth, 0, spriteWidth, spriteHeight);
            pieceSourceRects[Pieces.White | Pieces.Knight] = new Rectangle(3 * spriteWidth, 0, spriteWidth, spriteHeight);
            pieceSourceRects[Pieces.White | Pieces.Rook] = new Rectangle(4 * spriteWidth, 0, spriteWidth, spriteHeight);
            pieceSourceRects[Pieces.White | Pieces.Pawn] = new Rectangle(5 * spriteWidth, 0, spriteWidth, spriteHeight);

            // Black pieces
            pieceSourceRects[Pieces.Black | Pieces.King] = new Rectangle(0 * spriteWidth, spriteHeight, spriteWidth, spriteHeight);
            pieceSourceRects[Pieces.Black | Pieces.Queen] = new Rectangle(1 * spriteWidth, spriteHeight, spriteWidth, spriteHeight);
            pieceSourceRects[Pieces.Black | Pieces.Bishop] = new Rectangle(2 * spriteWidth, spriteHeight, spriteWidth, spriteHeight);
            pieceSourceRects[Pieces.Black | Pieces.Knight] = new Rectangle(3 * spriteWidth, spriteHeight, spriteWidth, spriteHeight);
            pieceSourceRects[Pieces.Black | Pieces.Rook] = new Rectangle(4 * spriteWidth, spriteHeight, spriteWidth, spriteHeight);
            pieceSourceRects[Pieces.Black | Pieces.Pawn] = new Rectangle(5 * spriteWidth, spriteHeight, spriteWidth, spriteHeight);
        }

        private void UpdateLayout()
        {
            int screenWidth = Raylib.GetScreenWidth();
            int screenHeight = Raylib.GetScreenHeight();

            // Use 90% of smaller dimension so it has margin
            int boardSize = (int)(Math.Min(screenWidth, screenHeight) * 0.9f);

            Layout.Size = boardSize;
            Layout.SquareSize = boardSize / 8;
            Layout.OffsetX = (screenWidth - boardSize) / 2;
            Layout.OffsetY = (screenHeight - boardSize) / 2;
        }

        public void Draw()
        {
            UpdateLayout();
            int mouseX = Raylib.GetMouseX();
            int mouseY = Raylib.GetMouseY();

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    bool isLightSquare = (row + col) % 2 == 0;
                    Color squareColor = isLightSquare ? new Color(240, 217, 181, 255) : new Color(181, 136, 99, 255);

                    int x = Layout.OffsetX + col * Layout.SquareSize;
                    int y = Layout.OffsetY + row * Layout.SquareSize;

                    int squareIndex = (7 - row) * 8 + col;

                    if (selectedSquare.HasValue && selectedSquare.Value == squareIndex)
                        squareColor = Color.Yellow;




                    Raylib.DrawRectangle(x, y, Layout.SquareSize, Layout.SquareSize, squareColor);

                    if (legalMoves != null && legalMoves.Contains(squareIndex))
                    {
                        Raylib.DrawRectangle(
                            x, y,
                            Layout.SquareSize,
                            Layout.SquareSize,
                            new Color(0, 255, 0, 120)); // semi-transparent green
                    }
                    if (Squares[squareIndex] != Pieces.None)
                    {
                        if (selectedSquare.HasValue && selectedSquare.Value == squareIndex)
                            continue; // skip drawn piece if it's being dragged

                        DrawPiece(Squares[squareIndex], x, y, Layout.SquareSize, Layout.SquareSize);
                    }
                }
            }

            // Draw dragged piece on top of everything
            if (selectedSquare.HasValue)
            {
                int piece = Squares[selectedSquare.Value];
                int drawX = mouseX - dragOffsetX;
                int drawY = mouseY - dragOffsetY;
                DrawPiece(piece, drawX, drawY, Layout.SquareSize, Layout.SquareSize);
            }

            HandleMouseInput();
        }

        private void DrawPiece(int piece, int x, int y, int width, int height)
        {
            if (pieceSourceRects.ContainsKey(piece))
            {
                Rectangle source = pieceSourceRects[piece];
                Rectangle dest = new Rectangle(x, y, width, height);
                Raylib.DrawTexturePro(pieceSpriteSheet, source, dest, new Vector2(0, 0), 0f, Color.White);
            }
        }

        private int? GetSquareUnderMouse()
        {
            int mx = Raylib.GetMouseX();
            int my = Raylib.GetMouseY();

            if (mx < Layout.OffsetX || mx > Layout.OffsetX + Layout.Size ||
                my < Layout.OffsetY || my > Layout.OffsetY + Layout.Size)
                return null;

            int col = (mx - Layout.OffsetX) / Layout.SquareSize;
            int row = (my - Layout.OffsetY) / Layout.SquareSize;

            return (7 - row) * 8 + col;
        }

        private void HandleMouseInput()
        {
            int? square = GetSquareUnderMouse();

            // Pick up piece
            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                if (square.HasValue && Squares[square.Value] != Pieces.None && Pieces.PieceColour(Squares[square.Value]) == MoveColour)
                {
                    selectedSquare = square.Value;
                    legalMoves = MoveGenerator.GenerateMoves(Squares, square.Value, MoveColour, enPassantTargetSquare, whiteCanCastleKingside, whiteCanCastleQueenside, blackCanCastleKingside, blackCanCastleQueenside);
                    legalMoves = MoveGenerator.FilterMovesToAvoidCheck(Squares, selectedSquare.Value, legalMoves);
                    dragOffsetX = Raylib.GetMouseX() - (Layout.OffsetX + (square.Value % 8) * Layout.SquareSize);
                    dragOffsetY = Raylib.GetMouseY() - (Layout.OffsetY + (7 - square.Value / 8) * Layout.SquareSize);
                }
            }

            // Drop piece
            if (Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                if (selectedSquare.HasValue && square.HasValue && legalMoves?.Contains(square.Value) == true)
                {
                    bool wasCapture = Squares[square.Value] != Pieces.None;

                    ExecuteMoves(selectedSquare.Value, square.Value);

                    if (wasCapture)
                        Raylib.PlaySound(captureSound);
                    else
                        Raylib.PlaySound(moveSound);

                    // Switch turn
                    IsWhiteToMove = !IsWhiteToMove;

                    int sideToMove = MoveColour;

                    // -----------------------------
                    // CHECK GAME STATE
                    // -----------------------------
                    if (!HasAnyLegalMoves(sideToMove))
                    {
                        if (IsKingInCheck(sideToMove))
                        {
                            string winner = sideToMove == Pieces.White ? "Black" : "White";
                            Console.WriteLine($"CHECKMATE! {winner} wins!");
                        }
                        else
                        {
                            Console.WriteLine("STALEMATE!");
                        }
                    }
                }

                selectedSquare = null;
                legalMoves.Clear();
            }
        }


        private void ExecuteMoves(int from, int to)
        {
            int piece = Squares[from];
            int color = Pieces.PieceColour(piece);
            int direction = Pieces.IsWhite(piece) ? 8 : -8;

            // -----------------------------
            // 1️⃣ Handle en passant capture
            // -----------------------------
            if (Pieces.PieceType(piece) == Pieces.Pawn &&
                    enPassantTargetSquare.HasValue &&
                    to == enPassantTargetSquare.Value &&
                    Squares[to] == Pieces.None)
            {
                int capturedPawnSquare = to - direction;
                Squares[capturedPawnSquare] = Pieces.None;
            }

            // -----------------------------------
            // 2️⃣ Update en passant target square
            // -----------------------------------
            if (Pieces.PieceType(piece) == Pieces.Pawn && Math.Abs(to - from) == 16)
            {
                // Pawn moved 2 squares forward → set en passant target behind it
                enPassantTargetSquare = (from + direction);
            }
            else
            {
                // Clear if not a double pawn move
                enPassantTargetSquare = null;
            }

            // -----------------------------
            // 3️⃣ Handle castling
            // -----------------------------
            int rank = (color == Pieces.White) ? 0 : 7;
            if (Pieces.PieceType(piece) == Pieces.King)
            {
                // King-side castling
                if (to == rank * 8 + 6)
                {
                    Squares[rank * 8 + 5] = Squares[rank * 8 + 7]; // Move rook
                    Squares[rank * 8 + 7] = Pieces.None;
                }

                // Queen-side castling
                if (to == rank * 8 + 2)
                {
                    Squares[rank * 8 + 3] = Squares[rank * 8 + 0]; // Move rook
                    Squares[rank * 8 + 0] = Pieces.None;
                }

                // Remove castling rights for that color
                if (color == Pieces.White)
                {
                    whiteCanCastleKingside = false;
                    whiteCanCastleQueenside = false;
                }
                else
                {
                    blackCanCastleKingside = false;
                    blackCanCastleQueenside = false;
                }
            }

            // -----------------------------
            // 4️⃣ Update castling rights for rook moves
            // -----------------------------
            if (Pieces.PieceType(piece) == Pieces.Rook)
            {
                if (from == rank * 8 + 0)
                {
                    if (color == Pieces.White) whiteCanCastleQueenside = false;
                    else blackCanCastleQueenside = false;
                }
                if (from == rank * 8 + 7)
                {
                    if (color == Pieces.White) whiteCanCastleKingside = false;
                    else blackCanCastleKingside = false;
                }
            }

            // -----------------------------
            // 5️⃣ Execute the move
            // -----------------------------
            Squares[to] = Squares[from];
            Squares[from] = Pieces.None;
        }
        private bool IsKingInCheck(int side)
        {
            int kingSquare = -1;

            for (int i = 0; i < 64; i++)
            {
                if (Pieces.PieceType(Squares[i]) == Pieces.King &&
                    Pieces.PieceColour(Squares[i]) == side)
                {
                    kingSquare = i;
                    break;
                }
            }

            if (kingSquare == -1)
                return false;

            return MoveGenerator.IsSquareAttacked(
                Squares,
                kingSquare,
                Pieces.OpponentColor(side)
            );
        }

        private bool HasAnyLegalMoves(int side)
        {
            for (int i = 0; i < 64; i++)
            {
                int piece = Squares[i];

                if (piece == Pieces.None) continue;
                if (Pieces.PieceColour(piece) != side) continue;

                var moves = MoveGenerator.GenerateMoves(
                    Squares,
                    i,
                    side,
                    enPassantTargetSquare,
                    whiteCanCastleKingside,
                    whiteCanCastleQueenside,
                    blackCanCastleKingside,
                    blackCanCastleQueenside
                );

                moves = MoveGenerator.FilterMovesToAvoidCheck(Squares, i, moves);

                if (moves.Count > 0)
                    return true;
            }

            return false;
        }
    }
}


