using UnityEngine;

public class Square : MonoBehaviour
{
    public int id;
    public Piece _piece;
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

    public void SetPiece(Piece piece)
    {
        if(piece == null)
        {
            _piece = null;
            pieceSprite.sprite = null;
            return;
        }

        _piece = piece;
        if(piece.IsWhite())
            pieceSprite.sprite = Board.instance.PieceSpritesWhite[piece.type - 1];
        else
            pieceSprite.sprite = Board.instance.PieceSpritesBlack[piece.type - 1];
    }

    void OnMouseDown()
    {
        if(_piece == null) return;

        Board.instance.HoldPiece(_piece, id);
        SetPiece(null);
    }

    void OnMouseOver()
    {
        Board.instance.SetMousePosition(id);
    }
}
