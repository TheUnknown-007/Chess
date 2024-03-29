using System;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public static Board Instance;

    [SerializeField] SpriteRenderer heldPieceSprite;
    [SerializeField] string startingPosition;
    public Square[] Cells;
    [SerializeField] Color DarkColor;
    [SerializeField] Color LightColor;
    [SerializeField] Color HighlightDarkColor;
    [SerializeField] Color HighlightLightColor;
    [SerializeField] Color PickedPieceLight;
    [SerializeField] Color PickedPieceDark;
    [SerializeField] Color DebugSquareLight;
    [SerializeField] Color DebugSquareDark;
    public Sprite[] PieceSpritesBlack;
    public Sprite[] PieceSpritesWhite;
    
    Piece heldPiece = null;
    int mouseHoverSquareID;

    List<Piece> _blackPieces = new List<Piece>();
    List<Piece> _whitePieces = new List<Piece>();


    void Awake()
    {
        if(Instance == null) Instance = this;
    }

    void Start()
    {
        PrepareBoard();
    }

    void Update()
    {
        if(heldPiece != null && Input.GetMouseButtonUp(0))
            DropPiece();
    }

    void PrepareBoard()
    {
        for(int rank = 0; rank < 8; rank++)
        {
            for(int file = 0; file < 8; file++)
            {
                int index = rank*8 + file;
                Cells[index].Initialize(index, ((file+rank) % 2 == 0) ? DarkColor : LightColor);
            }   
        }
        PlacePieces(startingPosition);
    }

    public void PrepareBoard(string FEN)
    {
        for(int rank = 0; rank < 8; rank++)
        {
            for(int file = 0; file < 8; file++)
            {
                int index = rank*8 + file;
                Cells[index].Initialize(index, ((file+rank) % 2 == 0) ? DarkColor : LightColor);
            }   
        }
        PlacePieces(FEN);
    }

    void PlacePieces(string FEN)
    {
        var SymbolToPiece = new Dictionary<char, int>() {
            ['p'] = PieceUtil.Pawn, ['n'] = PieceUtil.Knight, ['b'] = PieceUtil.Bishop,
            ['r'] = PieceUtil.Rook, ['q'] = PieceUtil.Queen,  ['k'] = PieceUtil.King
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
                file+=offset;
            }
            else if(Char.IsLetter(instruction))
            {
                int color = Char.IsUpper(pieces[x]) ? PieceUtil.White : PieceUtil.Black;
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


        int enPessant = (fenParts[3] == "-") ? -1 : (int)((fenParts[3][0] - 'a')*8 + (Char.GetNumericValue(fenParts[3][1])));
        GameManager.Instance.Initialize(_whitePieces, _blackPieces, (fenParts[1] == "w") ? PieceUtil.White : PieceUtil.Black, new int[][] { new int[2] { wl, ws }, new int[] { bl, bs } }, enPessant, int.Parse(fenParts[4]), int.Parse(fenParts[5]));
    }

    void PlacePiece(int piece, int cellIndex)
    {
        Piece p = new Piece(piece, cellIndex);
        if(p.IsWhite())
            _whitePieces.Add(p);
        else
            _blackPieces.Add(p);
    }

    public void HoldPiece(Piece piece, int position)
    {
        heldPiece = piece;
        foreach(Move move in heldPiece.legalMoves)
            HighlightSquare(move.TargetSquare, 1);
        HighlightSquare(position, 3);
        heldPieceSprite.sprite = piece.IsWhite() ? PieceSpritesWhite[piece.type - 1] : PieceSpritesBlack[piece.type - 1];          
    }

    void DropPiece()
    {
        bool legalMove = false;
        Move _move = new Move(0);
        foreach(Move move in heldPiece.legalMoves)
        {
            HighlightSquare(move.TargetSquare, 0);
            if(move.TargetSquare == mouseHoverSquareID)
            {
                legalMove = true;
                _move = move;
            }
        }
        HighlightSquare(heldPiece.position, 2);

        if(legalMove)
            GameManager.Instance.MakeAMove(_move);
        else
            Cells[heldPiece.position].ShowPiece();
        
        heldPieceSprite.sprite = null;
        heldPiece = null;
    }

    public void HighlightSquare(int id, int state)
    {
        Color oriColor = Cells[id].GetComponent<SpriteRenderer>().color;
        switch(state)
        {
            case 0:
                Cells[id].GetComponent<SpriteRenderer>().color = oriColor == HighlightLightColor ? LightColor : DarkColor;
                break;
            case 1:
                Cells[id].GetComponent<SpriteRenderer>().color = oriColor == LightColor ? HighlightLightColor : HighlightDarkColor;
                break;
            case 2:
                Cells[id].GetComponent<SpriteRenderer>().color = oriColor == PickedPieceLight ? LightColor : DarkColor;
                break;
            case 3:
                Cells[id].GetComponent<SpriteRenderer>().color = oriColor == LightColor ? PickedPieceLight : PickedPieceDark;
                break;
            case 4:
                Cells[id].GetComponent<SpriteRenderer>().color = oriColor == DebugSquareLight ? LightColor : DarkColor;
                break;
            case 5:
                Cells[id].GetComponent<SpriteRenderer>().color = oriColor == LightColor ? DebugSquareLight : DebugSquareDark;
                break;
        }

        
    }

    public void UpdateVisual()
    {
        foreach(Square cell in Board.Instance.Cells)
            cell.SetPiece(null);
        foreach(Piece piece in GameManager.Instance.blackPieces)
            Board.Instance.Cells[piece.position].SetPiece(piece);
        foreach(Piece piece in GameManager.Instance.whitePieces)
            Board.Instance.Cells[piece.position].SetPiece(piece);
    }

    public void SetMousePosition(int id)
    {
        mouseHoverSquareID = id;
    }
}
