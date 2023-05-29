using static System.Math;

public static class PrecomputedMoveData
{
    public static readonly int[] DirectionOffsets = { 8, -8, -1, 1, 7, -7, 9, -9 };
    public static readonly int[][] NumSquaresToEdge;

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
    }
}
