using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(PizzaCutter))]
public class PizzaCutterInspector : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        PizzaCutter cutter = (PizzaCutter)target;

        if (GUILayout.Button("RearrangeVertices"))
        {
            cutter.RearrangeVertices();
        }
    }

}
