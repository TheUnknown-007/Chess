public class Piece
{
    public int piece;
    public int position;
    public int colour;
    public int type;

    public Move[] legalMoves;

    public Piece(int Piece, int Position)
    {
        piece = Piece;
        position = Position;
        legalMoves = null;
        type = PieceUtil.PieceType(piece);
        colour = PieceUtil.Colour(piece);
    }

    public void SetLegalMoves(Move[] moves)
    {
        legalMoves = moves;
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
