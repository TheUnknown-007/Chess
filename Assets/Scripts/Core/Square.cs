using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Square : MonoBehaviour
{
    int id;
    int _piece;
    SpriteRenderer pieceSprite;

    void Awake()
    {
        pieceSprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    public void SetPiece(int piece)
    {
        _piece = piece;
        int type = Piece.PieceType(piece);
        if(type == 0)
            pieceSprite.sprite = null;
        else {
            if(Piece.IsColour(piece, Piece.White))
                pieceSprite.sprite = Board.instance.PieceSpritesWhite[type - 1];
            else
                pieceSprite.sprite = Board.instance.PieceSpritesBlack[type - 1];
        }
    }

    void OnMouseDown()
    {
        Board.instance.HoldPiece(_piece, id);
        SetPiece(Piece.Colour(_piece));
    }
}
