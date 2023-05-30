using System.Collections.Generic;
using static PrecomputedMoveData;

using UnityEngine;

public class MoveGenerator : MonoBehaviour
{
    List<Move> moves;

    public Move[] GenerateMoves()
    {
        moves = new List<Move>();

        // TODO: optimize
        for(int startSquare = 0; startSquare < 64; startSquare++)
        {
            Piece piece = Board.Instance.Cells[startSquare].GetComponent<Square>()._piece;
            if(piece.IsColour(GameManager.Instance.ColourToMove))
            {
                if(piece.IsSlidingPiece())
                    moves.AddRange(GenerateSlidingMoves(startSquare, piece));
            }
        }

        return moves.ToArray();
    }

    public static Move[] GenerateMoves(Piece piece)
    {
        if(piece.IsSlidingPiece())
            return GenerateSlidingMoves(piece.position, piece);
        else if(piece.type == PieceUtil.King)
            return GenerateKingMoves(piece.position, piece);
        return new Move[0];
    }

    static Move[] GenerateSlidingMoves(int startSquare, Piece piece)
    {
        List<Move> _moves = new List<Move>();
        int friendlyColor = piece.colour;
        int startDirOffset = piece.type == PieceUtil.Bishop ? 4 : 0;
        int endDirOffset =  piece.type == PieceUtil.Rook ? 4 : 8;

        for(int directionIndex = startDirOffset; directionIndex < endDirOffset; directionIndex++)
        {
            for(int n = 0; n<NumSquaresToEdge[startSquare][directionIndex]; n++)
            {
                int targetSquare = startSquare + DirectionOffsets[directionIndex] * (n+1);
                Piece pieceOnTargetSquare = Board.Instance.Cells[targetSquare].GetComponent<Square>()._piece;

                if(pieceOnTargetSquare != null)
                {
                    // Blocked by Friendly
                    if(pieceOnTargetSquare.IsColour(friendlyColor)) break;

                    _moves.Add(new Move(startSquare, targetSquare));

                    // Blocked by Opponent
                    if(!pieceOnTargetSquare.IsColour(friendlyColor)) break;
                }

                else _moves.Add(new Move(startSquare, targetSquare));
            }
        }

        return _moves.ToArray();
    }

    static Move[] GenerateKingMoves(int startSquare, Piece piece)
    {
        List<Move> _moves = new List<Move>();
        // Only allow safe squares
        return _moves.ToArray();
    }
}
