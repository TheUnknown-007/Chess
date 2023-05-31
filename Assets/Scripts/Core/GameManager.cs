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

    public bool KingInCheck { get; private set; }
    int legalKingMoves;
    int kingPosition;

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

        if(GameManager.Instance.ColourToMove == 8)
        {
            foreach(Piece piece in whitePieces)
                if(piece.pieceInt == (PieceUtil.White | PieceUtil.King))
                    kingPosition = piece.position;

            foreach(Piece piece in blackPieces)
            {
                piece.SetLegalMoves(new Move[0]);
                Move[] temp = MoveGenerator.GenerateMoves(piece, true);
                foreach(Move move in temp)
                    if(!attackedSquares.ContainsKey(move.TargetSquare)) attackedSquares.Add(move.TargetSquare, move.StartSquare);
            }
            foreach(Piece piece in whitePieces)
            {
                Move[] temp = MoveGenerator.GenerateMoves(piece, false);
                if(piece.pieceInt == (PieceUtil.White | PieceUtil.King))
                    legalKingMoves = temp.Length;
                piece.SetLegalMoves(temp);
            }
            KingInCheck = attackedSquares.ContainsKey(kingPosition);
        }
        else
        {
            foreach(Piece piece in whitePieces)
            {
                piece.SetLegalMoves(new Move[0]);
                Move[] temp = MoveGenerator.GenerateMoves(piece, true);
                foreach(Move move in temp)
                    if(!attackedSquares.ContainsKey(move.TargetSquare)) attackedSquares.Add(move.TargetSquare, move.StartSquare);
            }
            foreach(Piece piece in blackPieces)
                piece.SetLegalMoves(MoveGenerator.GenerateMoves(piece, false));
        }
    }

    public void MakeAMove(Move move)
    {
        Piece movingPiece = Board.Instance.Cells[move.StartSquare].GetComponent<Square>().piece;
        Piece targetPiece = Board.Instance.Cells[move.TargetSquare].GetComponent<Square>().piece;
        Board.Instance.Cells[move.StartSquare].GetComponent<Square>().SetPiece(null);
        movingPiece.SetPosition(move.TargetSquare);
        movingPiece.moved = true;

        if(move.MoveFlag == Move.Flag.PawnTwoForward) 
        {
            enPessantSquare = move.TargetSquare + (movingPiece.colour == PieceUtil.White ? -8 : 8);
            Board.Instance.Cells[move.TargetSquare].GetComponent<Square>().SetPiece(movingPiece);
        }
        
        else if(move.MoveFlag == Move.Flag.EnPassantCapture) 
        {
            targetPiece = Board.Instance.Cells[move.TargetSquare + (movingPiece.IsColour(PieceUtil.White) ? -8 : 8)].GetComponent<Square>().piece;
            Board.Instance.Cells[move.TargetSquare + (movingPiece.IsColour(PieceUtil.White) ? -8 : 8)].GetComponent<Square>().SetPiece(null);
            Board.Instance.Cells[move.TargetSquare].GetComponent<Square>().SetPiece(movingPiece);
            (targetPiece.colour == PieceUtil.White ? whitePieces : blackPieces).Remove(targetPiece);
        }

        else if(move.IsPromotion) 
        {
            Piece newQueen = new Piece(movingPiece.colour | PieceUtil.Queen, move.TargetSquare);
            Board.Instance.Cells[move.TargetSquare].GetComponent<Square>().SetPiece(newQueen);
            (movingPiece.colour == PieceUtil.White ? whitePieces : blackPieces).Remove(movingPiece);
            (movingPiece.colour == PieceUtil.White ? whitePieces : blackPieces).Add(newQueen);
            if(targetPiece != null)
                (targetPiece.colour == PieceUtil.White ? whitePieces : blackPieces).Remove(targetPiece);
        }

        else 
        {
            Board.Instance.Cells[move.TargetSquare].GetComponent<Square>().SetPiece(movingPiece);
            if(targetPiece != null)
                (targetPiece.colour == PieceUtil.White ? whitePieces : blackPieces).Remove(targetPiece);
            else halfMoves++;
        }

        ColourToMove = ColourToMove == PieceUtil.White ? PieceUtil.Black : PieceUtil.White;

        attackedSquares = new Dictionary<int, int>();
        if(GameManager.Instance.ColourToMove == 8)
        {
            foreach(Piece piece in blackPieces)
            {
                piece.SetLegalMoves(new Move[0]);
                Move[] temp = MoveGenerator.GenerateMoves(piece, true);
                foreach(Move move1 in temp)
                    if(!attackedSquares.ContainsKey(move1.TargetSquare)) attackedSquares.Add(move1.TargetSquare, move.StartSquare);
            }
            foreach(Piece piece in whitePieces)
                piece.SetLegalMoves(MoveGenerator.GenerateMoves(piece, false));
        }
        else
        {
            foreach(Piece piece in whitePieces)
            {
                piece.SetLegalMoves(new Move[0]);
                Move[] temp = MoveGenerator.GenerateMoves(piece, true);
                foreach(Move move1 in temp)
                    if(!attackedSquares.ContainsKey(move1.TargetSquare)) attackedSquares.Add(move1.TargetSquare, move.StartSquare);
            }
            foreach(Piece piece in blackPieces)
                piece.SetLegalMoves(MoveGenerator.GenerateMoves(piece, false));
        }

        if(ColourToMove == PieceUtil.White) fullMoves ++;
        movesText.text = fullMoves.ToString();
    }

    public void Checkmate()
    {
        Debug.Log("Checkmate!!!");
    }
}
