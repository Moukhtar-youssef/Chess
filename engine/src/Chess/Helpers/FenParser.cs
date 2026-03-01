using System;
using System.Collections.Generic;

namespace Engine.Chess
{
    public static class FenParser
    {
        public const string StartPositionFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";


        public static void LoadPositionFromFen(string fen, Board Board)
        {

            var pieceTypeFromSymbol = new Dictionary<char, int>()
            {
                ['k'] = Pieces.King,
                ['q'] = Pieces.Queen,
                ['r'] = Pieces.Rook,
                ['b'] = Pieces.Bishop,
                ['n'] = Pieces.Knight,
                ['p'] = Pieces.Pawn
            };

            string[] fenParts = fen.Split(' ');
            string fenBoard = fenParts[0];
            int file = 0, rank = 7;

            foreach (char c in fenBoard)
            {
                if (c == '/')
                {
                    rank--;
                    file = 0;
                }
                else if (char.IsDigit(c))
                {
                    file += (int)char.GetNumericValue(c);
                }
                else
                {
                    int PieceColor = char.IsUpper(c) ? Pieces.White : Pieces.Black;
                    int PieceType = pieceTypeFromSymbol[char.ToLower(c)];
                    Board.Squares[rank * 8 + file] = PieceColor | PieceType;
                    file++;
                }
            }

            Board.IsWhiteToMove = fenParts[1] == "w" ? true : false;

            foreach (char c in fenParts[2])
            {
                if (c == 'K') Board.whiteCanCastleKingside = true;
                else if (c == 'Q') Board.whiteCanCastleQueenside = true;
                else if (c == 'k') Board.blackCanCastleKingside = true;
                else if (c == 'q') Board.blackCanCastleQueenside = true;
            }

            Board.enPassantTargetSquare = fenParts[3] != "-"
                ? BoardHelper.SquareToIndex(fenParts[3])
                : (int?)null;

            Board.HalfmoveClock = int.Parse(fenParts[4]);
            Board.FullmoveNumber = int.Parse(fenParts[5]);
        }
    }
}

