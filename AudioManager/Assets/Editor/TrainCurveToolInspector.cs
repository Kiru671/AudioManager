using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR

[CustomEditor(typeof(TrainCurveTool))]
public class TrainCurveToolInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        TrainCurveTool tool = (TrainCurveTool)target;

        if (GUILayout.Button("Plot Curves"))
        {
            tool.PlotAllCurves();
        }
    }
}


#endif