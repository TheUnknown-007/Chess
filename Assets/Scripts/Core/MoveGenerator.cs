using System.Collections.Generic;
using static PrecomputedMoveData;

using UnityEngine;

public class MoveGenerator : MonoBehaviour
{
    List<Move> moves;

    public static Move[] GenerateMoves(Piece piece, bool findAttackingSquares)
    {
        if(piece.IsSlidingPiece())
            return GenerateSlidingMoves(piece.position, piece, findAttackingSquares);
        else if(piece.type == PieceUtil.King)
            return GenerateKingMoves(piece.position, piece, findAttackingSquares);
        else if(piece.type == PieceUtil.Knight)
            return GenerateKnightMoves(piece.position, piece, findAttackingSquares);
        else if(piece.type == PieceUtil.Pawn)
            return GeneratePawnMoves(piece.position, piece, findAttackingSquares);
        return new Move[0];
    }

    static Move[] GenerateSlidingMoves(int startSquare, Piece piece, bool findAttackingSquares)
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
                Piece pieceOnTargetSquare = Board.Instance.Cells[targetSquare].GetComponent<Square>().piece;

                if(pieceOnTargetSquare != null)
                {
                    // Blocked by Friendly
                    if(pieceOnTargetSquare.IsColour(friendlyColor)) 
                    {
                        if(findAttackingSquares) _moves.Add(new Move(startSquare, targetSquare));
                        break;
                    }

                    _moves.Add(new Move(startSquare, targetSquare));

                    // Blocked by Opponent
                    if(!pieceOnTargetSquare.IsColour(friendlyColor)) break;
                }

                else _moves.Add(new Move(startSquare, targetSquare));
            }
        }

        return _moves.ToArray();
    }

    static Move[] GenerateKingMoves(int startSquare, Piece piece, bool findAttackingSquares)
    {
        List<Move> _moves = new List<Move>();
        foreach(int offset in DirectionOffsets)
        {
            int targetSquare = startSquare + offset;
            int y = targetSquare / 8;
            int x = targetSquare - y*8;
            int distance = Mathf.Max(Mathf.Abs(y - piece.rank), Mathf.Abs(x - piece.file));
            if(distance == 1 && targetSquare > 0 && targetSquare < 64)
            {
                string debug = !GameManager.Instance.attackedSquares.ContainsKey(targetSquare) + " ";
                if(!GameManager.Instance.attackedSquares.ContainsKey(targetSquare))
                {
                    Piece targetPiece = Board.Instance.Cells[targetSquare].GetComponent<Square>().piece;
                    debug += $"{targetPiece != null} ";
                    if(targetPiece != null) debug += $"{!targetPiece.IsColour(piece.colour)} ";
                    if (targetPiece != null)
                        if(!targetPiece.IsColour(piece.colour))
                            _moves.Add(new Move(startSquare, targetSquare));
                    else _moves.Add(new Move(startSquare, targetSquare));
                }
                if(findAttackingSquares) _moves.Add(new Move(startSquare, targetSquare));
                if(GameManager.Instance.attackedSquares.ContainsKey(targetSquare))
                    Debug.Log($"{piece.position} : {targetSquare} : {GameManager.Instance.attackedSquares[targetSquare]}");
            }
        }
        return _moves.ToArray();
    }

    static Move[] GenerateKnightMoves(int startSquare, Piece piece, bool findAttackingSquares)
    {
        List<Move> _moves = new List<Move>();
        int[] knightOffsets = new int[] { 15, 17, -17, -15, 10, -6, 6, -10 };
        foreach(int offset in knightOffsets)
        {
            int targetSquare = startSquare + offset;
            if(targetSquare < 0 || targetSquare > 64) continue;
            int knightSquareY = targetSquare / 8;
			int knightSquareX = targetSquare - (knightSquareY * 8);
            int maxCoordMoveDst = System.Math.Max (System.Math.Abs (piece.file - knightSquareX), System.Math.Abs (piece.rank - knightSquareY));
            if(maxCoordMoveDst == 2)
            {
                Piece pieceOnTargetSquare = Board.Instance.Cells[targetSquare].GetComponent<Square>().piece;
                if(pieceOnTargetSquare != null)
                    if(findAttackingSquares || !pieceOnTargetSquare.IsColour(piece.colour))
                        _moves.Add(new Move(startSquare, targetSquare));
                else 
                    _moves.Add(new Move(startSquare, targetSquare));
            }
        }

        return _moves.ToArray();
    }

    static Move[] GeneratePawnMoves(int startSquare, Piece piece, bool findAttackingSquares)
    {
        List<Move> _moves = new List<Move>();

        int targetSquare;
        int distance;
        int x;
        int y;
        Piece pieceOnTargetSquare;
        if(!findAttackingSquares)
        {
            targetSquare = startSquare + (piece.IsColour(PieceUtil.White) ? 8 : -8);
            pieceOnTargetSquare = Board.Instance.Cells[targetSquare].GetComponent<Square>().piece;
            if(pieceOnTargetSquare == null)
            {
                if(piece.rank == 7)
                    _moves.Add(new Move(startSquare, targetSquare, Move.Flag.PromoteToQueen));
                else _moves.Add(new Move(startSquare, targetSquare));
            }

            if(!piece.moved)
            {
                targetSquare = startSquare + (piece.IsColour(PieceUtil.White) ? 16 : -16);
                pieceOnTargetSquare = Board.Instance.Cells[targetSquare].GetComponent<Square>().piece;
                if(pieceOnTargetSquare == null)
                    _moves.Add(new Move(startSquare, targetSquare, Move.Flag.PawnTwoForward));
            }

            targetSquare = startSquare + 1;
            y = targetSquare / 8;
            x = targetSquare - y*8;
            distance = Mathf.Max(Mathf.Abs(y - piece.rank), Mathf.Abs(x - piece.file));
            if(distance == 1)
            {
                int actualTargetSquare = targetSquare + (piece.IsColour(PieceUtil.White) ? 8 : -8);
                pieceOnTargetSquare = Board.Instance.Cells[targetSquare].GetComponent<Square>().piece;
                if(pieceOnTargetSquare != null && pieceOnTargetSquare.type == PieceUtil.Pawn && !pieceOnTargetSquare.IsColour(piece.colour) && GameManager.Instance.enPessantSquare == actualTargetSquare)
                    _moves.Add(new Move(startSquare, actualTargetSquare, Move.Flag.EnPassantCapture));
            }

            targetSquare = startSquare - 1;
            y = targetSquare / 8;
            x = targetSquare - y*8;
            distance = Mathf.Max(Mathf.Abs(y - piece.rank), Mathf.Abs(x - piece.file));
            if(distance == 1)
            {
                int actualTargetSquare = targetSquare + (piece.IsColour(PieceUtil.White) ? 8 : -8);
                pieceOnTargetSquare = Board.Instance.Cells[targetSquare].GetComponent<Square>().piece;
                if(pieceOnTargetSquare != null && pieceOnTargetSquare.type == PieceUtil.Pawn && !pieceOnTargetSquare.IsColour(piece.colour) && GameManager.Instance.enPessantSquare == actualTargetSquare)
                    _moves.Add(new Move(startSquare, actualTargetSquare, Move.Flag.EnPassantCapture));
            }
        }
        
        targetSquare = startSquare + (piece.IsColour(PieceUtil.White) ? 9 : -9);
        y = targetSquare / 8;
        x = targetSquare - y*8;
        distance = Mathf.Max(Mathf.Abs(y - piece.rank), Mathf.Abs(x - piece.file));
        if(distance == 1)
        {
            pieceOnTargetSquare = Board.Instance.Cells[targetSquare].GetComponent<Square>().piece;
            if(pieceOnTargetSquare != null)
            {
                if(!pieceOnTargetSquare.IsColour(piece.colour))
                {
                    if(piece.rank == (piece.IsColour(PieceUtil.White) ? 6 : 1))
                        _moves.Add(new Move(startSquare, targetSquare, Move.Flag.PromoteToQueen));
                    else _moves.Add(new Move(startSquare, targetSquare));
                }
            }
            else if(findAttackingSquares) _moves.Add(new Move(startSquare, targetSquare));
        }
        

        targetSquare = startSquare + (piece.IsColour(PieceUtil.White) ? 7 : -7);
        y = targetSquare / 8;
        x = targetSquare - y*8;
        distance = Mathf.Max(Mathf.Abs(y - piece.rank), Mathf.Abs(x - piece.file));
        if(distance == 1)
        {
            pieceOnTargetSquare = Board.Instance.Cells[targetSquare].GetComponent<Square>().piece;
            if(pieceOnTargetSquare != null)
            {
                if(!pieceOnTargetSquare.IsColour(piece.colour))
                {
                    if(piece.rank == (piece.IsColour(PieceUtil.White) ? 6 : 1))
                        _moves.Add(new Move(startSquare, targetSquare, Move.Flag.PromoteToQueen));
                    else _moves.Add(new Move(startSquare, targetSquare));
                }
            }
            else if(findAttackingSquares) _moves.Add(new Move(startSquare, targetSquare));
        }

        

        return _moves.ToArray();
    }
}
