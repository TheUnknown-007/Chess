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

        foreach(Piece piece in blackPieces)
        {
            if(piece.type == PieceUtil.King) blackKingPiece = piece;
            if(piece.type == PieceUtil.Rook)
            {
                if(piece.position < 60) blackLongRook = piece;
                else blackShortRook = piece;
            }
            piece.isPinned = false;
        }

        foreach(Piece piece in whitePieces)
        {
            if(piece.type == PieceUtil.King) whiteKingPiece = piece;
            if(piece.type == PieceUtil.Rook)
            {
                if(piece.position < 4) whiteLongRook = piece;
                else whiteShortRook = piece;
            }
            piece.isPinned = false;
        }

        if(GameManager.Instance.ColourToMove == 8)
        {
            foreach(Piece piece in blackPieces)
            {
                piece.SetLegalMoves(new Move[0]);
                Move[] temp = MoveGenerator.GenerateMoves(piece, true);
                foreach(Move move in temp)
                    if(!attackedSquares.Contains(move.TargetSquare)) 
                        attackedSquares.Add(move.TargetSquare);
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
                    if(!attackedSquares.Contains(move.TargetSquare))
                        attackedSquares.Add(move.TargetSquare);
            }
            MoveGenerator.SetPinnedPieces(blackKingPiece);
            foreach(Piece piece in blackPieces)
            {
                Move[] temp = MoveGenerator.GenerateMoves(piece, false);
                legalMoves += temp.Length;
                piece.SetLegalMoves(temp);
            }
        }

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

        else if(targetPiece != null && targetPiece.type == PieceUtil.Rook)
        {
            if(move.TargetSquare < (ColourToMove == 8 ? blackKingPiece : whiteKingPiece).position)
                (ColourToMove == 8 ? blackCastleRights : whiteCastleRights)[0] = 0;
            else (ColourToMove == 8 ? blackCastleRights : whiteCastleRights)[1] = 0;
        }

        Board.Instance.Cells[move.StartSquare].SetPiece(null);
        

        if(move.MoveFlag == Move.Flag.PawnTwoForward) 
        {
            enPessantSquare = move.TargetSquare + (movingPiece.colour == PieceUtil.White ? -8 : 8);
            Board.Instance.Cells[move.TargetSquare].SetPiece(movingPiece);
            movingPiece.SetPosition(move.TargetSquare);
        }
        
        else if(move.MoveFlag == Move.Flag.EnPassantCapture) 
        {
            targetPiece = Board.Instance.Cells[move.TargetSquare + (movingPiece.IsColour(PieceUtil.White) ? -8 : 8)].piece;
            Board.Instance.Cells[move.TargetSquare + (movingPiece.IsColour(PieceUtil.White) ? -8 : 8)].SetPiece(null);
            Board.Instance.Cells[move.TargetSquare].SetPiece(movingPiece);
            (targetPiece.colour == PieceUtil.White ? whitePieces : blackPieces).Remove(targetPiece);
            movingPiece.SetPosition(move.TargetSquare);
        }

        else if(move.IsPromotion) 
        {
            Piece newQueen = new Piece(movingPiece.colour | PieceUtil.Queen, move.TargetSquare);
            Board.Instance.Cells[move.TargetSquare].SetPiece(newQueen);
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
                Board.Instance.Cells[whiteKingPiece.position].SetPiece(null);
                whiteCastleRights = new int[2] { 0, 0 };

                if(move.TargetSquare > whiteKingPiece.position)
                {
                    Board.Instance.Cells[whiteShortRook.position].SetPiece(null);
                    whiteShortRook.SetPosition(5);
                    whiteKingPiece.SetPosition(6);
                    Board.Instance.Cells[5].SetPiece(whiteShortRook);
                    Board.Instance.Cells[6].SetPiece(whiteKingPiece);
                }
                else
                {
                    Board.Instance.Cells[whiteLongRook.position].SetPiece(null);
                    whiteLongRook.SetPosition(3);
                    whiteKingPiece.SetPosition(2);
                    Board.Instance.Cells[3].SetPiece(whiteLongRook);
                    Board.Instance.Cells[2].SetPiece(whiteKingPiece);
                }
            }

            else
            {
                Board.Instance.Cells[blackKingPiece.position].SetPiece(null);
                blackCastleRights = new int[2] { 0, 0 };

                if(move.TargetSquare > blackKingPiece.position)
                {
                    Board.Instance.Cells[blackShortRook.position].SetPiece(null);
                    blackShortRook.SetPosition(61);
                    blackKingPiece.SetPosition(62);
                    Board.Instance.Cells[61].SetPiece(blackShortRook);
                    Board.Instance.Cells[62].SetPiece(blackKingPiece);
                }
                else
                {
                    Board.Instance.Cells[blackLongRook.position].SetPiece(null);
                    blackLongRook.SetPosition(59);
                    blackKingPiece.SetPosition(58);
                    Board.Instance.Cells[59].SetPiece(blackLongRook);
                    Board.Instance.Cells[58].SetPiece(blackKingPiece);
                }
            }
        }

        else 
        {
            Board.Instance.Cells[move.TargetSquare].SetPiece(movingPiece);
            if(targetPiece != null)
                (targetPiece.colour == PieceUtil.White ? whitePieces : blackPieces).Remove(targetPiece);
            else halfMoves++;
            movingPiece.SetPosition(move.TargetSquare);
        }

        ColourToMove = ColourToMove == PieceUtil.White ? PieceUtil.Black : PieceUtil.White;

        attackedSquares = new List<int>();

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

    public void Checkmate(bool stalemate)
    {
        if(!stalemate) Debug.Log("Checkmate!!!");
        else Debug.Log("Stalemate :l");
    }
}
