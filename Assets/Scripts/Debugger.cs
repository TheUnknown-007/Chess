using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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

    public void GetVariable(string typeName, string varName, string varTypeName, string iterativeType, int index)
    {
        // Type MT = Type.GetType(varTypeName);
        // var newVar = objects[typeName].GetType().GetField(varName).GetValue(objects[typeName]);
        // Debug.Log(Convert.ChangeType(newVar, MT)[index]);
    }
}
