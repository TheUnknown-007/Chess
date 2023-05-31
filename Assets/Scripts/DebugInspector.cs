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
    int targetSquare;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Debugger debugger = (Debugger)target;
        // typeName = EditorGUILayout.TextField(typeName);
        // varName = EditorGUILayout.TextField(varName);
        // varTypeName = EditorGUILayout.TextField(varTypeName);
        // iterativeTypeName = EditorGUILayout.TextField(iterativeTypeName);
        // index = EditorGUILayout.IntField(index);
        targetSquare = EditorGUILayout.IntField(targetSquare);
        
        if(GUILayout.Button("Get Attacked Square"))
            debugger.GetAttackedSquare(targetSquare);
        if(GUILayout.Button("Get Variable"))
            debugger.GetVariable(typeName, varName, varTypeName, iterativeTypeName, index);
    }
}
