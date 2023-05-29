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
            int piece = Board.instance.Cells[startSquare].GetComponent<Square>()._piece;
            if(Piece.IsColour(piece, Board.instance.ColourToMove))
            {
                if(Piece.IsSlidingPiece(piece))
                    moves.AddRange(GenerateSlidingMoves(startSquare, piece));
            }
        }

        return moves.ToArray();
    }

    public static Move[] GenerateSlidingMoves(int startSquare, int piece)
    {
        Debug.Log(NumSquaresToEdge[startSquare][3]);
        List<Move> _moves = new List<Move>();
        int friendlyColor = Piece.Colour(piece);
        for(int directionIndex = 0; directionIndex < 8; directionIndex++)
        {
            for(int n = 0; n<NumSquaresToEdge[startSquare][directionIndex]; n++)
            {
                int targetSquare = startSquare + DirectionOffsets[directionIndex] * (n+1);
                int pieceOnTargetSquare = Board.instance.Cells[targetSquare].GetComponent<Square>()._piece;

                if(Piece.PieceType(pieceOnTargetSquare) != 0)
                {
                    // Blocked by Friendly
                    if(Piece.IsColour(pieceOnTargetSquare, friendlyColor)) break;

                    _moves.Add(new Move(startSquare, targetSquare));

                    // Blocked by Opponent
                    if(!Piece.IsColour(pieceOnTargetSquare, friendlyColor)) break;
                }

                else _moves.Add(new Move(startSquare, targetSquare));
            }
        }

        return _moves.ToArray();
    }
}
