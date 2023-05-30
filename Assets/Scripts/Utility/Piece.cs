public class Piece
{
    public int piece;
    public int position;
    public int file;
    public int rank;
    public int colour;
    public int type;
    public bool moved = false;

    public Move[] legalMoves;

    public Piece(int Piece, int Position)
    {
        piece = Piece;
        position = Position;
        legalMoves = null;
        type = PieceUtil.PieceType(piece);
        colour = PieceUtil.Colour(piece);

        rank = position / 8;
        file = position - (rank * 8);
    }

    public void SetLegalMoves(Move[] moves)
    {
        legalMoves = moves;
    }

    public void SetPosition(int newPos)
    {
        position = newPos;
        rank = position / 8;
        file = position - (rank * 8);
    }

    public bool IsWhite()
    {
        return colour == PieceUtil.White;
    }

    public bool IsColour(int match)
    {
        return match == colour;
    }

    public bool IsSlidingPiece()
    {
        return PieceUtil.IsSlidingPiece(piece);
    }
}
