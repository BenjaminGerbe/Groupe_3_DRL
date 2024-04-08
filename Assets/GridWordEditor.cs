using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridWorld))]
public class GridWordEditor : Editor
{
    private GridWorld _target;
    // Start is called before the first frame update
    void OnEnable()
    {
        _target = (GridWorld)target;
    }
   public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if(GUILayout.Button("Init")){
           _target.Evaluate();
        }
        
        if(GUILayout.Button("Improve")){
            _target.Improvement();
        }
    }
}
