using UnityEngine;

public class Square : MonoBehaviour
{
    public int id;
    public Piece piece;
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

    public void SetPiece(Piece newPiece)
    {
        if(newPiece == null)
        {
            piece = null;
            pieceSprite.sprite = null;
            return;
        }

        piece = newPiece;
        if(piece.IsWhite())
            pieceSprite.sprite = Board.Instance.PieceSpritesWhite[piece.type - 1];
        else
            pieceSprite.sprite = Board.Instance.PieceSpritesBlack[piece.type - 1];
    }

    void OnMouseDown()
    {
        if(piece == null) return;

        Board.Instance.HoldPiece(piece, id);
        pieceSprite.sprite = null;
    }

    void OnMouseOver()
    {
        Board.Instance.SetMousePosition(id);
    }
}
