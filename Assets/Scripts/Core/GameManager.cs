using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] TMP_Text movesText;

    List<Piece> whitePieces;
    List<Piece> blackPieces;
    [HideInInspector] public int ColourToMove = 8;
    int halfMoves;
    int fullMoves;
    public int enPessantSquare { get; private set; }
    int[] whiteCastleRights;
    int[] blackCastleRights;

    Piece blackKingPiece;
    Piece whiteKingPiece;
    public bool KingInCheck { get; private set; }
    public List<int> checkResolveSquares { get; private set; }
    int legalMoves;

    public Dictionary<int, int> attackedSquares;

    void Awake()
    {
        Instance = this;
    }

    public void Initialize(List<Piece> WhitePieces, List<Piece> BlackPieces, int colourToMove, int[][] CastleRights, int EnPessantSquare, int HalfMoves, int FullMoves)
    {
        whitePieces = WhitePieces;
        blackPieces = BlackPieces;
        ColourToMove = colourToMove;
        whiteCastleRights = CastleRights[0];
        blackCastleRights = CastleRights[1];
        enPessantSquare = EnPessantSquare;
        halfMoves = HalfMoves;
        fullMoves = FullMoves;

        attackedSquares = new Dictionary<int, int>();

        foreach(Piece piece in blackPieces)
        {
            if(piece.type == PieceUtil.King) blackKingPiece = piece;
            piece.isPinned = false;
        }

        foreach(Piece piece in whitePieces)
        {
            if(piece.type == PieceUtil.King) whiteKingPiece = piece;
            piece.isPinned = false;
        }

        if(GameManager.Instance.ColourToMove == 8)
        {
            foreach(Piece piece in blackPieces)
            {
                piece.SetLegalMoves(new Move[0]);
                Move[] temp = MoveGenerator.GenerateMoves(piece, true);
                foreach(Move move in temp)
                    if(!attackedSquares.ContainsKey(move.TargetSquare)) 
                        attackedSquares.Add(move.TargetSquare, move.StartSquare);
            }
            MoveGenerator.SetPinnedPieces(whiteKingPiece);
            foreach(Piece piece in whitePieces)
            {
                Move[] temp = MoveGenerator.GenerateMoves(piece, false);
                legalMoves += temp.Length;
                piece.SetLegalMoves(temp);
            }
        }
        else
        {
            foreach(Piece piece in whitePieces)
            {
                piece.SetLegalMoves(new Move[0]);
                Move[] temp = MoveGenerator.GenerateMoves(piece, true);
                foreach(Move move in temp)
                    if(!attackedSquares.ContainsKey(move.TargetSquare))
                        attackedSquares.Add(move.TargetSquare, move.StartSquare);
            }
            MoveGenerator.SetPinnedPieces(blackKingPiece);
            foreach(Piece piece in blackPieces)
            {
                Move[] temp = MoveGenerator.GenerateMoves(piece, false);
                legalMoves += temp.Length;
                piece.SetLegalMoves(temp);
            }
        }

        if(legalMoves == 0) Checkmate();
    }

    public void MakeAMove(Move move)
    {
        legalMoves = 0;
        if(KingInCheck)
        {
            KingInCheck = false;
            checkResolveSquares = new List<int>();
        }

        Piece movingPiece = Board.Instance.Cells[move.StartSquare].piece;
        Piece targetPiece = Board.Instance.Cells[move.TargetSquare].piece;
        Board.Instance.Cells[move.StartSquare].SetPiece(null);
        movingPiece.SetPosition(move.TargetSquare);

        if(move.MoveFlag == Move.Flag.PawnTwoForward) 
        {
            enPessantSquare = move.TargetSquare + (movingPiece.colour == PieceUtil.White ? -8 : 8);
            Board.Instance.Cells[move.TargetSquare].SetPiece(movingPiece);
        }
        
        else if(move.MoveFlag == Move.Flag.EnPassantCapture) 
        {
            targetPiece = Board.Instance.Cells[move.TargetSquare + (movingPiece.IsColour(PieceUtil.White) ? -8 : 8)].piece;
            Board.Instance.Cells[move.TargetSquare + (movingPiece.IsColour(PieceUtil.White) ? -8 : 8)].SetPiece(null);
            Board.Instance.Cells[move.TargetSquare].SetPiece(movingPiece);
            (targetPiece.colour == PieceUtil.White ? whitePieces : blackPieces).Remove(targetPiece);
        }

        else if(move.IsPromotion) 
        {
            Piece newQueen = new Piece(movingPiece.colour | PieceUtil.Queen, move.TargetSquare);
            Board.Instance.Cells[move.TargetSquare].SetPiece(newQueen);
            (movingPiece.colour == PieceUtil.White ? whitePieces : blackPieces).Remove(movingPiece);
            (movingPiece.colour == PieceUtil.White ? whitePieces : blackPieces).Add(newQueen);
            if(targetPiece != null)
                (targetPiece.colour == PieceUtil.White ? whitePieces : blackPieces).Remove(targetPiece);
        }

        else 
        {
            Board.Instance.Cells[move.TargetSquare].SetPiece(movingPiece);
            if(targetPiece != null)
                (targetPiece.colour == PieceUtil.White ? whitePieces : blackPieces).Remove(targetPiece);
            else halfMoves++;
        }

        ColourToMove = ColourToMove == PieceUtil.White ? PieceUtil.Black : PieceUtil.White;

        attackedSquares = new Dictionary<int, int>();

        foreach(Piece piece in blackPieces)
            piece.isPinned = false;

        foreach(Piece piece in whitePieces)
            piece.isPinned = false;

        if(GameManager.Instance.ColourToMove == 8)
        {
            foreach(Piece piece in blackPieces)
            {
                piece.SetLegalMoves(new Move[0]);
                Move[] temp = MoveGenerator.GenerateMoves(piece, true);
                foreach(Move move1 in temp)
                    if(!attackedSquares.ContainsKey(move1.TargetSquare))
                        attackedSquares.Add(move1.TargetSquare, move1.StartSquare);
            }
            MoveGenerator.SetPinnedPieces(whiteKingPiece);
            foreach(Piece piece in whitePieces)
            {
                Move[] temp = MoveGenerator.GenerateMoves(piece, false);
                legalMoves += temp.Length;
                piece.SetLegalMoves(temp);
            }
        }
        else
        {
            foreach(Piece piece in whitePieces)
            {
                piece.SetLegalMoves(new Move[0]);
                Move[] temp = MoveGenerator.GenerateMoves(piece, true);
                foreach(Move move1 in temp)
                    if(!attackedSquares.ContainsKey(move1.TargetSquare))
                        attackedSquares.Add(move1.TargetSquare, move1.StartSquare);
            }
            MoveGenerator.SetPinnedPieces(blackKingPiece);
            foreach(Piece piece in blackPieces)
            {
                Move[] temp = MoveGenerator.GenerateMoves(piece, false);
                legalMoves += temp.Length;
                piece.SetLegalMoves(temp);
            }
        }


        if(ColourToMove == PieceUtil.White) fullMoves ++;
        movesText.text = fullMoves.ToString();
        if(legalMoves == 0) Checkmate();
    }

    public void RegisterCheck(Piece attacker, params int[] data)
    {
        checkResolveSquares = new List<int>();

        if(KingInCheck) return;

        checkResolveSquares.Add(attacker.position);

        if(attacker.IsSlidingPiece())
            for(int n = 1; n < data[1]; n++)
                checkResolveSquares.Add(attacker.position + (data[0]*n));

        KingInCheck = true;
    }

    public void Checkmate()
    {
        Debug.Log("Checkmate!!!");
    }
}
