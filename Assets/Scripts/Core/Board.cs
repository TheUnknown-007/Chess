using System;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public static Board instance;

    [SerializeField] string startingPosition;
    [SerializeField] GameObject[] Cells;
    [SerializeField] Color DarkColor;
    [SerializeField] Color LightColor;
    [SerializeField] Sprite[] pieceSpritesBlack;
    [SerializeField] Sprite[] pieceSpritesWhite;
    public static Sprite[] PieceSpritesBlack;
    public static Sprite[] PieceSpritesWhite;

    [SerializeField] SpriteRenderer heldPieceSprite;
    
    public static bool isPieceHeld;
    int heldPiece;
    int heldPiecePos;

    int move;
    int halfMoves;
    int fullMoves;
    string enPessantSquare = "-";
    int[] whiteCastleRights;
    int[] blackCastleRights;

    void Awake()
    {
        if(instance == null) instance = this;
    }

    void Start()
    {
        PieceSpritesBlack = pieceSpritesBlack;
        PieceSpritesWhite = pieceSpritesWhite;
        PrepareBoard();
    }

    void PrepareBoard()
    {
        for(int rank = 0; rank < 8; rank++)
        {
            for(int file = 0; file < 8; file++)
            {
                int index = rank*8 + file;
                Cells[index].GetComponent<SpriteRenderer>().color = ((file+rank) % 2 == 0) ? DarkColor : LightColor;
            }   
        }
        PlacePieces(startingPosition);
    }

    void PlacePieces(string FEN)
    {
        var SymbolToPiece = new Dictionary<char, int>() {
            ['p'] = Piece.Pawn, ['n'] = Piece.Knight, ['b'] = Piece.Bishop,
            ['r'] = Piece.Rook, ['q'] = Piece.Queen,  ['k'] = Piece.King
        };
        int rank = 7;
        int file = 0;
        string[] fenParts = FEN.Split(" ");
        string pieces = fenParts[0];

        for(int x = 0; x < pieces.Length; x++)
        {
            char instruction = pieces[x];
            if(Char.IsDigit(instruction))
            {
                int offset = (int)Char.GetNumericValue(instruction);
                if(offset != 8)
                {
                    file+=offset;
                    x++;
                    int piece = SymbolToPiece[Char.ToLower(pieces[x])];
                    int color = Char.IsUpper(pieces[x]) ? Piece.White : Piece.Black;
                    PlacePiece(color | piece, rank*8 + file);
                }
            }
            else if(Char.IsLetter(instruction))
            {
                int color = Char.IsUpper(pieces[x]) ? Piece.White : Piece.Black;
                int piece = SymbolToPiece[Char.ToLower(instruction)];
                PlacePiece(color | piece, rank*8 + file);
                file++;
            }
            else if(instruction == '/')
            {
                file = 0;
                rank -= 1;
            }
        }

        if(fenParts[1] == "w") move = 0;
        else move = 1;

        int bs = 0;
        int bl = 0;
        int ws = 0;
        int wl = 0;
        foreach(char right in fenParts[2])
        {
            switch(right)
            {
                case 'Q':
                    wl = 1;
                    break;
                case 'K':
                    ws = 1;
                    break;
                case 'q':
                    bl = 1;
                    break;
                case 'k':
                    bs = 1;
                    break;
            }
        }
        whiteCastleRights = new int[2] { wl, ws };
        blackCastleRights = new int[2] { bl, bs };
        enPessantSquare = fenParts[3];
        halfMoves = int.Parse(fenParts[4]);
        fullMoves = int.Parse(fenParts[5]);
    }

    void PlacePiece(int piece, int cellIndex)
    {
        Cells[cellIndex].GetComponent<Square>().SetPiece(piece);
    }

    public void HoldPiece(int piece, int position)
    {
        heldPieceSprite.sprite = Piece.IsColour(piece, Piece.White) ? Board.PieceSpritesWhite[Piece.PieceType(piece) - 1] : Board.PieceSpritesBlack[Piece.PieceType(piece) - 1];          
        heldPiece = piece;
        heldPiecePos = position;
        isPieceHeld = true;
    }

    public void DropPiece(int piece)
    {
        
        // if legal make move

        // else drop it back to original square
        Cells[heldPiecePos].GetComponent<Square>().SetPiece(piece);
    }
}
