
using System;

namespace Engine.Chess
{
    public class BoardHelper
    {

        public static int SquareToIndex(string square)
        {
            if (string.IsNullOrWhiteSpace(square) || square.Length != 2)
                throw new ArgumentException("Invalid square format.");

            char fileChar = char.ToLower(square[0]);
            char rankChar = square[1];

            if (fileChar < 'a' || fileChar > 'h')
                throw new ArgumentException("Invalid file (must be a-h).");

            if (rankChar < '1' || rankChar > '8')
                throw new ArgumentException("Invalid rank (must be 1-8).");

            int file = fileChar - 'a';
            int rank = rankChar - '1';

            return rank * 8 + file;
        }
        public static string IndexToSquare(int index)
        {
            if (index < 0 || index > 63)
                throw new ArgumentException("Index must be between 0 and 63.");

            int file = index % 8;
            int rank = index / 8;

            char fileChar = (char)('a' + file);
            char rankChar = (char)('1' + rank);

            return $"{fileChar}{rankChar}";
        }
    }
}
