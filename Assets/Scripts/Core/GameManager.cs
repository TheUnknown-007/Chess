using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] TMP_Text movesText;

    public List<Piece> whitePieces;
    public List<Piece> blackPieces;
    [HideInInspector] public int ColourToMove = 8;
    int halfMoves;
    int fullMoves;
    public int enPessantSquare { get; private set; }
    public int[] whiteCastleRights { get; private set; }
    public int[] blackCastleRights { get; private set; }
    Piece whiteLongRook;
    Piece whiteShortRook;
    Piece blackLongRook;
    Piece blackShortRook;

    Piece blackKingPiece;
    Piece whiteKingPiece;
    public bool KingInCheck { get; private set; }
    public List<int> checkResolveSquares { get; private set; }
    int legalMoves;

    public List<int> attackedSquares;

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

        attackedSquares = new List<int>();

        Board.Instance.UpdateVisual();

        foreach(Piece piece in blackPieces)
        {
            if(piece.type == PieceUtil.King) blackKingPiece = piece;
            if(piece.type == PieceUtil.Rook)
            {
                if(piece.position < 60) blackLongRook = piece;
                else blackShortRook = piece;
            }
        }

        foreach(Piece piece in whitePieces)
        {
            if(piece.type == PieceUtil.King) whiteKingPiece = piece;
            if(piece.type == PieceUtil.Rook)
            {
                if(piece.position < 4) whiteLongRook = piece;
                else whiteShortRook = piece;
            }
        }

        GetLegalMoves();

        if(legalMoves == 0) Checkmate(!KingInCheck);
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

        if(movingPiece.type == PieceUtil.King) 
        {
            if(ColourToMove == 8) whiteCastleRights = new int[2] { 0, 0 };
            else blackCastleRights = new int[2] { 0, 0 };
        }
        else if(movingPiece.type == PieceUtil.Rook)
        {
            if(move.StartSquare < (ColourToMove == 8 ? whiteKingPiece : blackKingPiece).position)
                (ColourToMove == 8 ? whiteCastleRights : blackCastleRights)[0] = 0;
            else (ColourToMove == 8 ? whiteCastleRights : blackCastleRights)[1] = 0;
        }
        if(targetPiece != null && targetPiece.type == PieceUtil.Rook)
        {
            if(move.TargetSquare < (ColourToMove == 8 ? blackKingPiece : whiteKingPiece).position)
                (ColourToMove == 8 ? blackCastleRights : whiteCastleRights)[0] = 0;
            else (ColourToMove == 8 ? blackCastleRights : whiteCastleRights)[1] = 0;
        }
        
        if(move.MoveFlag == Move.Flag.PawnTwoForward) 
        {
            enPessantSquare = move.TargetSquare + (movingPiece.colour == PieceUtil.White ? -8 : 8);
            movingPiece.SetPosition(move.TargetSquare);
        }
        
        else if(move.MoveFlag == Move.Flag.EnPassantCapture) 
        {
            targetPiece = Board.Instance.Cells[move.TargetSquare + (movingPiece.IsColour(PieceUtil.White) ? -8 : 8)].piece;
            (targetPiece.colour == PieceUtil.White ? whitePieces : blackPieces).Remove(targetPiece);
            movingPiece.SetPosition(move.TargetSquare);
        }

        else if(move.IsPromotion) 
        {
            Piece newQueen = new Piece(movingPiece.colour | PieceUtil.Queen, move.TargetSquare);
            (movingPiece.colour == PieceUtil.White ? whitePieces : blackPieces).Remove(movingPiece);
            (movingPiece.colour == PieceUtil.White ? whitePieces : blackPieces).Add(newQueen);
            if(targetPiece != null)
                (targetPiece.colour == PieceUtil.White ? whitePieces : blackPieces).Remove(targetPiece);
            else halfMoves++;
            movingPiece.SetPosition(move.TargetSquare);
        }

        else if(move.MoveFlag == Move.Flag.Castling)
        {
            if(ColourToMove == 8)
            {
                whiteCastleRights = new int[2] { 0, 0 };

                if(move.TargetSquare > whiteKingPiece.position)
                {
                    whiteShortRook.SetPosition(5);
                    whiteKingPiece.SetPosition(6);
                }
                else
                {
                    whiteLongRook.SetPosition(3);
                    whiteKingPiece.SetPosition(2);
                }
            }

            else
            {
                blackCastleRights = new int[2] { 0, 0 };

                if(move.TargetSquare > blackKingPiece.position)
                {
                    blackShortRook.SetPosition(61);
                    blackKingPiece.SetPosition(62);
                }
                else
                {
                    blackLongRook.SetPosition(59);
                    blackKingPiece.SetPosition(58);
                }
            }
        }

        else 
        {
            if(targetPiece != null)
                (targetPiece.colour == PieceUtil.White ? whitePieces : blackPieces).Remove(targetPiece);
            else halfMoves++;
            movingPiece.SetPosition(move.TargetSquare);
        }

        ColourToMove = ColourToMove == PieceUtil.White ? PieceUtil.Black : PieceUtil.White;
        
        foreach(Piece piece in GameManager.Instance.blackPieces)
            piece.isPinned = false;
        foreach(Piece piece in GameManager.Instance.whitePieces)
            piece.isPinned = false;

        Board.Instance.UpdateVisual();
        GetLegalMoves();

        if(ColourToMove == PieceUtil.White) fullMoves ++;
        movesText.text = fullMoves.ToString();
        if(legalMoves == 0) Checkmate(!KingInCheck);
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

    void GetLegalMoves()
    {
        attackedSquares = new List<int>();
        if(GameManager.Instance.ColourToMove == 8)
        {
            foreach(Piece piece in blackPieces)
            {
                piece.SetLegalMoves(new Move[0]);
                Move[] temp = MoveGenerator.GenerateMoves(piece, true);
                foreach(Move move1 in temp)
                    if(!attackedSquares.Contains(move1.TargetSquare))
                        attackedSquares.Add(move1.TargetSquare);
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
                    if(!attackedSquares.Contains(move1.TargetSquare))
                        attackedSquares.Add(move1.TargetSquare);
            }
            MoveGenerator.SetPinnedPieces(blackKingPiece);
            foreach(Piece piece in blackPieces)
            {
                Move[] temp = MoveGenerator.GenerateMoves(piece, false);
                legalMoves += temp.Length;
                piece.SetLegalMoves(temp);
            }
        }
    }

    public void Checkmate(bool stalemate)
    {
        if(!stalemate) Debug.Log("Checkmate!!!");
        else Debug.Log("Stalemate :l");
    }
}
