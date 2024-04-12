using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Sokoban))]
public class SokobanEditor : Editor
{
    private Sokoban _target;
    // Start is called before the first frame update
    void OnEnable()
    {
        _target = (Sokoban)target;
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Init"))
        {
            _target.Init();
        }
 
        
        GUILayout.Label("---------------");
        
        if (GUILayout.Button("Improvment"))
        {
            _target.Improvement();
        }
        
        if (GUILayout.Button("Value Iteration"))
        {
            _target.ValueIteration();
        }
        
        GUILayout.Label("---------------");
        
        if (GUILayout.Button("Init MCTS"))
        {
            _target.InitMCTS();
        }
        
        if (GUILayout.Button("MCTS"))
        {
            _target.StartMCTS();
        }
        
        GUILayout.Label(_target.target.ToString());
        GUILayout.Space(10);
        
    }
}