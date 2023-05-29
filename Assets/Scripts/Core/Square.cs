using UnityEngine;

public class Square : MonoBehaviour
{
    public int id;
    public int _piece;
    SpriteRenderer pieceSprite;

    void Awake()
    {
        pieceSprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    public void Initialize(int ID, Color Color)
    {
        id = ID;
        GetComponent<SpriteRenderer>().color = Color;
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
        if(Piece.PieceType(_piece) == 0) return;

        Board.instance.HoldPiece(_piece, id);
        SetPiece(0);
    }

    void OnMouseOver()
    {
        Board.instance.SetMousePosition(id);
    }
}
