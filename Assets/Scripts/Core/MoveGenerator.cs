using System.Collections.Generic;
using System.Linq;
using static PrecomputedMoveData;

using UnityEngine;

public class MoveGenerator : MonoBehaviour
{
    List<Move> moves;

    public static void SetPinnedPieces(Piece kingPiece)
    {
        int startSquare = kingPiece.position;
        
        for(int directionIndex = 0; directionIndex < 8; directionIndex++)
        {
            Piece pieceToPin = null;
            for(int n = 0; n<NumSquaresToEdge[startSquare][directionIndex]; n++)
            {
                int targetSquare = startSquare + DirectionOffsets[directionIndex] * (n+1);
                Piece pieceOnTargetSquare = Board.Instance.Cells[targetSquare].piece;
                if(pieceOnTargetSquare == null) continue;
                if(pieceToPin == null)
                    pieceToPin = pieceOnTargetSquare;

                else
                {
                    if(!pieceOnTargetSquare.IsColour(kingPiece.colour) && (pieceOnTargetSquare.IsRookOrQueen() || pieceOnTargetSquare.IsBishopOrQueen()))
                    {
                        if(directionIndex < 4)
                        { 
                            if (pieceOnTargetSquare.IsRookOrQueen())
                            {
                                pieceToPin.isPinned = true;
                                List<int> temp = new List<int>();
                                temp.Add(targetSquare);
                                for(int m = 1; m < n; m++)
                                    temp.Add(pieceToPin.position + (DirectionOffsets[directionIndex]*m));
                                pieceToPin.pinLine = temp.ToArray();
                            }
                        }
                        else if (pieceOnTargetSquare.IsBishopOrQueen()) 
                        {
                                pieceToPin.isPinned = true;
                                List<int> temp = new List<int>();
                                temp.Add(targetSquare);
                                for(int m = 1; m < n; m++)
                                    temp.Add(pieceToPin.position + (DirectionOffsets[directionIndex]*m));
                                pieceToPin.pinLine = temp.ToArray();
                        }
                    }
                    else goto EndLoop;
                    
                }
            }

            EndLoop:
            _ = false;
        }
        
    }

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
        if(!findAttackingSquares && GameManager.Instance.KingInCheck && GameManager.Instance.checkResolveSquares.Count == 0) return new Move[0];

        List<Move> _moves = new List<Move>();
        int friendlyColor = piece.colour;
        int startDirOffset = piece.type == PieceUtil.Bishop ? 4 : 0;
        int endDirOffset =  piece.type == PieceUtil.Rook ? 4 : 8;

        for(int directionIndex = startDirOffset; directionIndex < endDirOffset; directionIndex++)
        {
            for(int n = 0; n<NumSquaresToEdge[startSquare][directionIndex]; n++)
            {
                int targetSquare = startSquare + DirectionOffsets[directionIndex] * (n+1);
                Piece pieceOnTargetSquare = Board.Instance.Cells[targetSquare].piece;

                if(pieceOnTargetSquare != null)
                {
                    // Blocked by Friendly
                    if(pieceOnTargetSquare.IsColour(friendlyColor)) 
                    {
                        if(findAttackingSquares) _moves.Add(new Move(startSquare, targetSquare));
                        break;
                    }

                    if(!findAttackingSquares)
                    {
                        if((!piece.isPinned || piece.pinLine.Contains(targetSquare)) && (!GameManager.Instance.KingInCheck || GameManager.Instance.checkResolveSquares.Contains(targetSquare)))
                            _moves.Add(new Move(startSquare, targetSquare));
                    }
                    else _moves.Add(new Move(startSquare, targetSquare));

                    // Blocked by Opponent
                    if(!pieceOnTargetSquare.IsColour(friendlyColor))
                    {
                        if(!findAttackingSquares) break;
                        else {
                            if(pieceOnTargetSquare.type != PieceUtil.King) break;
                            else {
                                int distance = Mathf.Max(Mathf.Abs(pieceOnTargetSquare.file - piece.file), Mathf.Abs(pieceOnTargetSquare.rank - piece.rank));
                                GameManager.Instance.RegisterCheck(piece, DirectionOffsets[directionIndex], distance, piece.position);
                            }
                        }
                    }
                }

                else if(findAttackingSquares) _moves.Add(new Move(startSquare, targetSquare));
                else if(!GameManager.Instance.KingInCheck || GameManager.Instance.checkResolveSquares.Contains(targetSquare))
                    if(!piece.isPinned || piece.pinLine.Contains(targetSquare)) _moves.Add(new Move(startSquare, targetSquare));
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
                if(!GameManager.Instance.attackedSquares.Contains(targetSquare))
                {
                    Piece targetPiece = Board.Instance.Cells[targetSquare].piece;
                    if (targetPiece != null)
                    {
                        if(!targetPiece.IsColour(piece.colour))
                            _moves.Add(new Move(startSquare, targetSquare));
                    }
                    else _moves.Add(new Move(startSquare, targetSquare));
                }
                if(findAttackingSquares) _moves.Add(new Move(startSquare, targetSquare));
            }
        }


        int[] castleRights = piece.IsColour(PieceUtil.White) ? GameManager.Instance.whiteCastleRights : GameManager.Instance.blackCastleRights;
        int moveColor = piece.IsColour(PieceUtil.White) ? 0 : 1;
        if(!findAttackingSquares && !GameManager.Instance.KingInCheck)
        {
            bool emptyAndNotAttackedLong = true;
            bool emptyAndNotAttackedShort = true;
            foreach(int square in CastleSquares[moveColor][0])
                if(Board.Instance.Cells[square].piece != null || GameManager.Instance.attackedSquares.Contains(square)) emptyAndNotAttackedLong = false;
            foreach(int square in CastleSquares[moveColor][1])
                if(Board.Instance.Cells[square].piece != null || GameManager.Instance.attackedSquares.Contains(square)) emptyAndNotAttackedShort = false;

            if(emptyAndNotAttackedLong && castleRights[0] == 1) _moves.Add(new Move(startSquare, startSquare - 2, Move.Flag.Castling));
            if(emptyAndNotAttackedShort && castleRights[1] == 1) _moves.Add(new Move(startSquare, startSquare + 2, Move.Flag.Castling));
        }

        return _moves.ToArray();
    }
    
    static Move[] GenerateKnightMoves(int startSquare, Piece piece, bool findAttackingSquares)
    {
        if(!findAttackingSquares && GameManager.Instance.KingInCheck && GameManager.Instance.checkResolveSquares.Count == 0) return new Move[0];

        List<Move> _moves = new List<Move>();
        int[] knightOffsets = new int[] { 15, 17, -17, -15, 10, -6, 6, -10 };
        foreach(int offset in knightOffsets)
        {
            int targetSquare = startSquare + offset;
            if(targetSquare < 0 || targetSquare > 64) continue;
            string debug = $"{targetSquare}\n";
            int knightSquareY = targetSquare / 8;
			int knightSquareX = targetSquare - (knightSquareY * 8);
            int maxCoordMoveDst = System.Math.Max (System.Math.Abs (piece.file - knightSquareX), System.Math.Abs (piece.rank - knightSquareY));
            if(maxCoordMoveDst == 2)
            {
                Piece pieceOnTargetSquare = Board.Instance.Cells[targetSquare].piece;
                debug += (pieceOnTargetSquare != null) + " ";
                debug += findAttackingSquares + " ";
                if(pieceOnTargetSquare != null)
                {
                    debug += pieceOnTargetSquare.IsColour(piece.colour) + " ";
                    if(findAttackingSquares)
                    {
                        if(pieceOnTargetSquare.type == PieceUtil.King && !pieceOnTargetSquare.IsColour(piece.colour))
                            GameManager.Instance.RegisterCheck(piece);
                        _moves.Add(new Move(startSquare, targetSquare));
                    }
                    else if(!pieceOnTargetSquare.IsColour(piece.colour))
                    {
                        if(!GameManager.Instance.KingInCheck || GameManager.Instance.checkResolveSquares.Contains(targetSquare))
                            if(!piece.isPinned || piece.pinLine.Contains(targetSquare)) _moves.Add(new Move(startSquare, targetSquare));
                    }
                }
                else if(findAttackingSquares) _moves.Add(new Move(startSquare, targetSquare));
                else if(!GameManager.Instance.KingInCheck || GameManager.Instance.checkResolveSquares.Contains(targetSquare))
                    if(!piece.isPinned || piece.pinLine.Contains(targetSquare)) _moves.Add(new Move(startSquare, targetSquare));
            }

            if(startSquare == 38) Debug.Log(debug);
        }

        return _moves.ToArray();
    }

    static Move[] GeneratePawnMoves(int startSquare, Piece piece, bool findAttackingSquares)
    {
        if(!findAttackingSquares && GameManager.Instance.KingInCheck && GameManager.Instance.checkResolveSquares.Count == 0) return new Move[0];

        List<Move> _moves = new List<Move>();

        int targetSquare;
        int distance;
        int x;
        int y;
        Piece pieceOnTargetSquare;
        if(!findAttackingSquares)
        {
            targetSquare = startSquare + (piece.IsColour(PieceUtil.White) ? 8 : -8);
            pieceOnTargetSquare = Board.Instance.Cells[targetSquare].piece;
            if(pieceOnTargetSquare == null)
            {
                if(!GameManager.Instance.KingInCheck || GameManager.Instance.checkResolveSquares.Contains(targetSquare))
                {
                    if(!piece.isPinned || piece.pinLine.Contains(targetSquare))
                    {
                        if(piece.rank == (piece.IsColour(PieceUtil.White) ? 6 : 1))
                            _moves.Add(new Move(startSquare, targetSquare, Move.Flag.PromoteToQueen));
                        else _moves.Add(new Move(startSquare, targetSquare));
                    }
                }
            }

            if(piece.rank == (piece.IsColour(PieceUtil.White) ? 1 : 6) && Board.Instance.Cells[targetSquare].piece == null)
            {
                targetSquare = startSquare + (piece.IsColour(PieceUtil.White) ? 16 : -16);
                pieceOnTargetSquare = Board.Instance.Cells[targetSquare].piece;
                if(pieceOnTargetSquare == null && (!piece.isPinned || piece.pinLine.Contains(targetSquare)) && (!GameManager.Instance.KingInCheck || GameManager.Instance.checkResolveSquares.Contains(targetSquare)))
                    _moves.Add(new Move(startSquare, targetSquare, Move.Flag.PawnTwoForward));
            }

            targetSquare = startSquare + 1;
            y = targetSquare / 8;
            x = targetSquare - y*8;
            distance = Mathf.Max(Mathf.Abs(y - piece.rank), Mathf.Abs(x - piece.file));
            if(distance == 1 && (!piece.isPinned || piece.pinLine.Contains(targetSquare)))
            {
                int actualTargetSquare = targetSquare + (piece.IsColour(PieceUtil.White) ? 8 : -8);
                pieceOnTargetSquare = Board.Instance.Cells[targetSquare].piece;
                if(pieceOnTargetSquare != null && pieceOnTargetSquare.type == PieceUtil.Pawn && !pieceOnTargetSquare.IsColour(piece.colour) && GameManager.Instance.enPessantSquare == actualTargetSquare)
                    if(!GameManager.Instance.KingInCheck || GameManager.Instance.checkResolveSquares.Contains(targetSquare))
                        _moves.Add(new Move(startSquare, actualTargetSquare, Move.Flag.EnPassantCapture));
            }

            targetSquare = startSquare - 1;
            y = targetSquare / 8;
            x = targetSquare - y*8;
            distance = Mathf.Max(Mathf.Abs(y - piece.rank), Mathf.Abs(x - piece.file));
            if(distance == 1 && (!piece.isPinned || piece.pinLine.Contains(targetSquare)))
            {
                int actualTargetSquare = targetSquare + (piece.IsColour(PieceUtil.White) ? 8 : -8);
                pieceOnTargetSquare = Board.Instance.Cells[targetSquare].piece;
                if(pieceOnTargetSquare != null && pieceOnTargetSquare.type == PieceUtil.Pawn && !pieceOnTargetSquare.IsColour(piece.colour) && GameManager.Instance.enPessantSquare == actualTargetSquare)
                    if(!GameManager.Instance.KingInCheck || GameManager.Instance.checkResolveSquares.Contains(targetSquare))
                        _moves.Add(new Move(startSquare, actualTargetSquare, Move.Flag.EnPassantCapture));
            }
        }
        
        targetSquare = startSquare + (piece.IsColour(PieceUtil.White) ? 9 : -9);
        y = targetSquare / 8;
        x = targetSquare - y*8;
        distance = Mathf.Max(Mathf.Abs(y - piece.rank), Mathf.Abs(x - piece.file));
        if(distance == 1)
        {
            pieceOnTargetSquare = Board.Instance.Cells[targetSquare].piece;
            if(pieceOnTargetSquare != null)
            {
                if(!pieceOnTargetSquare.IsColour(piece.colour))
                {
                    if(!GameManager.Instance.KingInCheck || GameManager.Instance.checkResolveSquares[0] == targetSquare)
                    {
                        if(!piece.isPinned || piece.pinLine.Contains(targetSquare))
                        {
                            if(piece.rank == (piece.IsColour(PieceUtil.White) ? 6 : 1))
                                _moves.Add(new Move(startSquare, targetSquare, Move.Flag.PromoteToQueen));
                            else _moves.Add(new Move(startSquare, targetSquare));
                        }
                        
                    }
                    
                    if(pieceOnTargetSquare.type == PieceUtil.King)
                        GameManager.Instance.RegisterCheck(piece);
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
            pieceOnTargetSquare = Board.Instance.Cells[targetSquare].piece;
            if(pieceOnTargetSquare != null)
            {
                if(!pieceOnTargetSquare.IsColour(piece.colour))
                {
                    if(!GameManager.Instance.KingInCheck || GameManager.Instance.checkResolveSquares[0] == targetSquare)
                    {
                        if(!piece.isPinned || piece.pinLine.Contains(targetSquare))
                        {
                            if(piece.rank == (piece.IsColour(PieceUtil.White) ? 6 : 1))
                                _moves.Add(new Move(startSquare, targetSquare, Move.Flag.PromoteToQueen));
                            else _moves.Add(new Move(startSquare, targetSquare));
                        }
                    }

                    if(pieceOnTargetSquare.type == PieceUtil.King)
                        GameManager.Instance.RegisterCheck(piece);
                }
            }
            else if(findAttackingSquares) _moves.Add(new Move(startSquare, targetSquare));
        }

        return _moves.ToArray();
    }


}
