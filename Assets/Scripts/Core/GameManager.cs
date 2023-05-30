using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    List<Piece> whitePieces;
    List<Piece> blackPieces;
    [HideInInspector] public int ColourToMove = 8;
    int halfMoves;
    int fullMoves;
    int enPessantSquare = -1;
    int[] whiteCastleRights;
    int[] blackCastleRights;

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

        if(GameManager.Instance.ColourToMove == 8)
        {
            foreach(Piece piece in whitePieces)
                piece.SetLegalMoves(MoveGenerator.GenerateMoves(piece));
            foreach(Piece piece in blackPieces)
                piece.SetLegalMoves(new Move[0]);
        }
        else
        {
            foreach(Piece piece in whitePieces)
                piece.SetLegalMoves(new Move[0]);
            foreach(Piece piece in blackPieces)
                piece.SetLegalMoves(MoveGenerator.GenerateMoves(piece));
        }
    }
}
