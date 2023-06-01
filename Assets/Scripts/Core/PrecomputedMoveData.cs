using static System.Math;

public static class PrecomputedMoveData
{
    public static readonly int[] DirectionOffsets = { 8, -8, -1, 1, 7, -7, 9, -9 };
    public static readonly int[][] NumSquaresToEdge;
    public static readonly int[][][] CastleSquares;

    static PrecomputedMoveData()
    {
        NumSquaresToEdge = new int[64][];
        for(int file = 0; file < 8; file++)
        {
            for(int rank = 0; rank < 8; rank++)
            {
                int numNorth = 7 - rank;
                int numSouth = rank;
                int numWest = file;
                int numEast = 7 - file;

                int squareIndex = rank*8 + file;

                NumSquaresToEdge[squareIndex] = new int[] {
                    numNorth, numSouth, numWest, numEast,
                    Min(numNorth, numWest),
                    Min(numSouth, numEast),
                    Min(numNorth, numEast),
                    Min(numSouth, numWest)
                };
            }
        }
        CastleSquares = new int[2][][];

        CastleSquares[0] = new int[2][];
        CastleSquares[0][0] = new int[3] { 1, 2, 3 };
        CastleSquares[0][1] = new int[2] { 5, 6 };

        CastleSquares[1] = new int[2][];
        CastleSquares[1][0] = new int[3] { 57, 58, 59 };
        CastleSquares[1][1] = new int[2] { 61, 62 };

        
    }
}
