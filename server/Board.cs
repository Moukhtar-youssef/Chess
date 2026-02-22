class ChessBoard
{
  public int Size = 8;

  public ChessBoard()
  {
    int BoardFull = Size * Size;

    Console.WriteLine($"Chess board initialized with {BoardFull} squares.");
  }
}