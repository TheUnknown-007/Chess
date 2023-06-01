using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Debugger))]
public class DebugInspector : Editor
{
    string typeName;
    string varName;
    string varTypeName;
    string iterativeTypeName;
    int index;
    int data1;
    int data2;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Debugger debugger = (Debugger)target;
        // typeName = EditorGUILayout.TextField(typeName);
        // varName = EditorGUILayout.TextField(varName);
        // varTypeName = EditorGUILayout.TextField(varTypeName);
        // iterativeTypeName = EditorGUILayout.TextField(iterativeTypeName);
        // index = EditorGUILayout.IntField(index);
        data1 = EditorGUILayout.IntField(data1);
        data2 = EditorGUILayout.IntField(data2);
        
        if(GUILayout.Button("Get Attacked Square"))
            debugger.GetAttackedSquare(data1);
        if(GUILayout.Button("Check if Piece is Pinned"))
            debugger.GetPinnedPiece(data1);
        if(GUILayout.Button("Check Square in Pin Line"))
            debugger.CheckIfPinnedPieceLegalMove(data1, data2);
        if(GUILayout.Button("Get Variable"))
            debugger.GetVariable(typeName, varName, varTypeName, iterativeTypeName, index);
    }
}
