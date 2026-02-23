using System;
using System.Collections.Generic;

namespace Engine.Chess
{
    public class MoveGenerator
    {

        public static List<int> GenerateMoves(int[] board, int square, int sideToMove, int? enPassantTargetSquare, bool whiteCanCastleKingside, bool whiteCanCastleQueenside, bool blackCanCastleKingside, bool blackCanCastleQueenside)
        {
            var moves = new List<int>();
            int piece = board[square];

            if (piece == Pieces.None || Pieces.PieceColour(piece) != sideToMove)
                return moves;

            switch (Pieces.PieceType(piece))
            {
                case Pieces.Pawn:
                    GeneratePawnMoves(board, square, moves, enPassantTargetSquare);
                    break;

                case Pieces.Knight:
                    GenerateKnightMoves(board, square, moves);
                    break;

                case Pieces.Bishop:
                    GenerateSlidingMoves(board, square, moves, bishopDirections);
                    break;

                case Pieces.Rook:
                    GenerateSlidingMoves(board, square, moves, rookDirections);
                    break;

                case Pieces.Queen:
                    GenerateSlidingMoves(board, square, moves, queenDirections);
                    break;

                case Pieces.King:
                    GenerateKingMoves(board, square, moves, whiteCanCastleKingside, whiteCanCastleQueenside, blackCanCastleKingside, blackCanCastleQueenside);
                    break;
            }

            return moves;



        }


        static readonly int[] knightOffsets =
        {
    -17, -15, -10, -6,
     6,  10,  15, 17

        };

        private static void GenerateKnightMoves(int[] board, int square, List<int> moves)
        {
            int piece = board[square];
            int startFile = square % 8;

            foreach (int offset in knightOffsets)
            {
                int target = square + offset;

                if (target < 0 || target >= 64)
                    continue;

                int targetFile = target % 8;

                if (Math.Abs(startFile - targetFile) > 2)
                    continue;

                int targetPiece = board[target];

                if (targetPiece == Pieces.None ||
                    Pieces.IsEnemy(piece, targetPiece))
                {
                    moves.Add(target);
                }
            }
        }

        static readonly int[] rookDirections = { -8, 8, -1, 1 };
        static readonly int[] bishopDirections = { -9, -7, 7, 9 };
        static readonly int[] queenDirections = { -8, 8, -1, 1, -9, -7, 7, 9 };
        private static void GenerateSlidingMoves(int[] board, int square, List<int> moves, int[] directions)
        {
            int piece = board[square];
            int startFile = square % 8;

            foreach (int dir in directions)
            {
                int target = square;

                while (true)
                {
                    int next = target + dir;

                    if (next < 0 || next >= 64)
                        break;

                    int nextFile = next % 8;

                    // Prevent horizontal wrap
                    int fileDiff = Math.Abs(nextFile - (target % 8));

                    // Horizontal moves
                    if (dir == 1 || dir == -1)
                    {
                        if (fileDiff != 1)
                            break;
                    }
                    // Diagonal moves
                    else if (dir == 7 || dir == 9 || dir == -7 || dir == -9)
                    {
                        if (fileDiff != 1)
                            break;
                    }

                    int targetPiece = board[next];

                    if (targetPiece == Pieces.None)
                    {
                        moves.Add(next);
                    }
                    else
                    {
                        if (Pieces.IsEnemy(piece, targetPiece))
                            moves.Add(next);

                        break;
                    }

                    target = next;
                }
            }
        }
        private static void GeneratePawnMoves(int[] board, int square, List<int> moves, int? enPassantTarget)
        {
            int piece = board[square];
            int direction = Pieces.IsWhite(piece) ? 8 : -8;
            int startRank = Pieces.IsWhite(piece) ? 1 : 6;

            int rank = square / 8;
            int file = square % 8;

            int forward = square + direction;

            if (forward >= 0 && forward < 64 && board[forward] == Pieces.None)
            {
                moves.Add(forward);


                int doubleForward = forward + direction;
                if (rank == startRank && board[doubleForward] == Pieces.None)
                {
                    moves.Add(doubleForward);
                }
            }

            int[] captureOffsets = { direction - 1, direction + 1 };
            foreach (int offset in captureOffsets)
            {
                int target = square + offset;

                if (target < 0 || target >= 64)
                    continue;

                if (Math.Abs((target % 8) - (square % 8)) != 1) continue;

                if (board[target] != Pieces.None && Pieces.IsEnemy(piece, board[target]))
                {
                    moves.Add(target);
                }
                if (enPassantTarget.HasValue && target == enPassantTarget.Value)
                    moves.Add(target);
            }
        }
        private static void AddCastlingMoves(int[] board, int kingSquare, List<int> moves, bool whiteCanCastleKingside, bool whiteCanCastleQueenside, bool blackCanCastleKingside, bool blackCanCastleQueenside)
        {
            int color = Pieces.PieceColour(board[kingSquare]);
            bool kingside, queenside;
            int rank = (color == Pieces.White) ? 0 : 7; // adjust to your numbering

            if (color == Pieces.White)
            {
                kingside = whiteCanCastleKingside;
                queenside = whiteCanCastleQueenside;
            }
            else
            {
                kingside = blackCanCastleKingside;
                queenside = blackCanCastleQueenside;
            }

            // Kingside castling
            if (kingside &&
                board[rank * 8 + 5] == Pieces.None &&
                board[rank * 8 + 6] == Pieces.None &&
                !IsSquareAttacked(board, kingSquare, Pieces.OpponentColor(color)) &&
                !IsSquareAttacked(board, rank * 8 + 5, Pieces.OpponentColor(color)) &&
                !IsSquareAttacked(board, rank * 8 + 6, Pieces.OpponentColor(color)))
            {
                moves.Add(rank * 8 + 6); // castling destination square
            }

            // Queenside castling
            if (queenside &&
                board[rank * 8 + 1] == Pieces.None &&
                board[rank * 8 + 2] == Pieces.None &&
                board[rank * 8 + 3] == Pieces.None &&
                !IsSquareAttacked(board, kingSquare, Pieces.OpponentColor(color)) &&
                !IsSquareAttacked(board, rank * 8 + 3, Pieces.OpponentColor(color)) &&
                !IsSquareAttacked(board, rank * 8 + 2, Pieces.OpponentColor(color)))
            {
                moves.Add(rank * 8 + 2);
            }
        }

        private static void GenerateKingMoves(int[] board, int square, List<int> moves, bool whiteCanCastleKingside, bool whiteCanCastleQueenside, bool blackCanCastleKingside, bool blackCanCastleQueenside)
        {
            int piece = board[square];
            int rank = square / 8;
            int file = square % 8;

            int[] offsets = { -9, -8, -7, -1, 1, 7, 8, 9 };

            foreach (int offset in offsets)
            {
                int target = square + offset;

                if (target < 0 || target >= 64)
                    continue;

                int targetRank = target / 8;
                int targetFile = target % 8;

                // Prevent horizontal wrap
                if (Math.Abs(targetFile - file) > 1)
                    continue;

                int targetPiece = board[target];
                if (targetPiece == Pieces.None || Pieces.IsEnemy(piece, targetPiece))
                    moves.Add(target);
            }
            AddCastlingMoves(board, square, moves, whiteCanCastleKingside, whiteCanCastleQueenside, blackCanCastleKingside, blackCanCastleQueenside);
        }

        private static int GetKingSquare(int[] board, int color)
        {
            for (int i = 0; i < 64; i++)
            {
                int piece = board[i];
                if (Pieces.PieceType(piece) == Pieces.King && Pieces.PieceColour(piece) == color)
                    return i;
            }
            return -1;
        }
        private static void GeneratePawnAttacks(int[] board, int square, List<int> moves)
        {
            int piece = board[square];
            int direction = Pieces.IsWhite(piece) ? 8 : -8; // your board numbering
            int file = square % 8;

            int[] offsets = { direction - 1, direction + 1 };
            foreach (int offset in offsets)
            {
                int target = square + offset;
                if (target < 0 || target >= 64)
                    continue;

                int targetFile = target % 8;
                if (Math.Abs(targetFile - file) != 1)
                    continue;

                moves.Add(target);
            }
        }
        private static void GenerateKingAttack(int[] board, int square, List<int> moves)
        {
            int piece = board[square];
            int rank = square / 8;
            int file = square % 8;

            int[] offsets = { -9, -8, -7, -1, 1, 7, 8, 9 };

            foreach (int offset in offsets)
            {
                int target = square + offset;

                if (target < 0 || target >= 64)
                    continue;

                int targetRank = target / 8;
                int targetFile = target % 8;

                // Prevent horizontal wrap
                if (Math.Abs(targetFile - file) > 1)
                    continue;

                int targetPiece = board[target];
                if (targetPiece == Pieces.None || Pieces.IsEnemy(piece, targetPiece))
                    moves.Add(target);
            }


        }
        public static bool IsSquareAttacked(int[] board, int square, int byColor)
        {
            for (int i = 0; i < 64; i++)
            {
                int piece = board[i];
                if (Pieces.PieceColour(piece) != byColor)
                    continue;

                List<int> moves = new List<int>();

                switch (Pieces.PieceType(piece))
                {
                    case Pieces.Pawn:
                        GeneratePawnAttacks(board, i, moves);
                        break;
                    case Pieces.Knight:
                        GenerateKnightMoves(board, i, moves);
                        break;
                    case Pieces.Bishop:
                        GenerateSlidingMoves(board, i, moves, bishopDirections);
                        break;
                    case Pieces.Rook:
                        GenerateSlidingMoves(board, i, moves, rookDirections);
                        break;
                    case Pieces.Queen:
                        GenerateSlidingMoves(board, i, moves, queenDirections);
                        break;
                    case Pieces.King:
                        GenerateKingAttack(board, i, moves);
                        break;
                }

                if (moves.Contains(square))
                    return true;
            }

            return false;
        }

        public static List<int> FilterMovesToAvoidCheck(int[] board, int square, List<int> moves)
        {
            int piece = board[square];
            int color = Pieces.PieceColour(piece);
            List<int> legalMoves = new List<int>();

            foreach (int target in moves)
            {
                int[] tempBoard = (int[])board.Clone();
                tempBoard[target] = tempBoard[square];
                tempBoard[square] = Pieces.None;

                int kingSquare = GetKingSquare(tempBoard, color);

                if (!IsSquareAttacked(tempBoard, kingSquare, Pieces.OpponentColor(color)))
                    legalMoves.Add(target);
            }

            return legalMoves;
        }
    }
}

