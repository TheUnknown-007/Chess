public class Piece
{
    public int pieceInt;
    public int position;
    public int file;
    public int rank;
    public int colour;
    public int type;
    public bool moved = false;

    public Move[] legalMoves;

    public Piece(int Piece, int Position)
    {
        pieceInt = Piece;
        position = Position;
        legalMoves = null;
        type = PieceUtil.PieceType(pieceInt);
        colour = PieceUtil.Colour(pieceInt);

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
        return PieceUtil.IsSlidingPiece(pieceInt);
    }
}
