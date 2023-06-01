using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class Debugger : MonoBehaviour
{
    Dictionary<string, object> objects = new Dictionary<string, object>();

    void Start()
    {
        objects.Add("Board", Board.Instance);
        objects.Add("GameManager", GameManager.Instance);
    }

    public void GetAttackedSquare(int targetSquare)
    {
        Debug.Log(GameManager.Instance.attackedSquares.ContainsKey(targetSquare));
        if(GameManager.Instance.attackedSquares.ContainsKey(targetSquare))
            Debug.Log(GameManager.Instance.attackedSquares[targetSquare]);
    }

    public void GetPinnedPiece(int targetSquare)
    {
        Debug.Log(Board.Instance.Cells[targetSquare].piece.isPinned);
        foreach(int square in Board.Instance.Cells[targetSquare].piece.pinLine) Debug.Log(square);
    }

    public void CheckIfPinnedPieceLegalMove(int pinnedPiece, int target)
    {
        Debug.Log(Board.Instance.Cells[pinnedPiece].piece.pinLine.Contains(target));
    }

    public void GetVariable(string typeName, string varName, string varTypeName, string iterativeType, int index)
    {
        // Type MT = Type.GetType(varTypeName);
        // var newVar = objects[typeName].GetType().GetField(varName).GetValue(objects[typeName]);
        // Debug.Log(Convert.ChangeType(newVar, MT)[index]);
    }
}
