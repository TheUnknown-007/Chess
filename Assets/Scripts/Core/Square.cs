using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Square : MonoBehaviour
{
    int id;
    int _piece;
    SpriteRenderer pieceSprite;

    void Start()
    {
        pieceSprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    public void SetPiece(int piece)
    {
        _piece = piece;
        if(Piece.IsColour(piece, Piece.White))
            pieceSprite.sprite = Board.PieceSpritesWhite[Piece.PieceType(piece) - 1];
        else 
            pieceSprite.sprite = Board.PieceSpritesBlack[Piece.PieceType(piece) - 1];
    }

    void OnMouseDown()
    {
        Board.instance.HoldPiece(_piece, id);
    }
}
