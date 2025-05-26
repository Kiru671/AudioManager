using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SplineTool)), CanEditMultipleObjects]
public class SplineToolInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SplineTool tool = (SplineTool)target;

        if (GUILayout.Button("PlaceRoads"))
        {
            tool.PlaceRoads();
        }

        if(GUILayout.Button("Add Handle"))
        {
            AddHandle();
        }
    }

    protected virtual void OnSceneGUI()
    {
        SplineTool spline = (SplineTool)target;
        List<Transform> handleTransforms = new List<Transform>(spline.handles);
        if(spline.start != null && !spline.startSnapped)
            handleTransforms.Insert(0,spline.start);
        if(spline.end != null && !spline.endSnapped)
            handleTransforms.Add(spline.end);


        GUIStyle style = new GUIStyle();
        style.fontSize = 30;
        for(int i = 0; i < handleTransforms.Count; i++)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.PositionHandle(handleTransforms[i].position, Quaternion.identity);
            Handles.Label(newPos, "" + (i + 1),style);
            if (EditorGUI.EndChangeCheck())
            {
                Debug.Log("Detected Change");
                Undo.RegisterCompleteObjectUndo(handleTransforms[i], "Change Handle Position");
                handleTransforms[i].position = newPos;
                EditorUtility.SetDirty(handleTransforms[i]);
            }
        }
    }

    public void AddHandle()
    {
        SplineTool spline = (SplineTool)target;

        GameObject newHandle = Instantiate(spline.handlePrefab, spline.transform);
        Undo.RegisterCreatedObjectUndo(newHandle, "Create Handle");
        Vector3 handlePos;
        if (spline.handles.Length > 0)
            handlePos = (spline.handles[spline.handles.Length - 1].position + spline.end.position) / 2f;
        else
            handlePos = (spline.start.position + spline.end.position) / 2f;

        newHandle.transform.position = handlePos;
        Undo.RecordObject(spline, "Added Handle");
        List<Transform> handles = new List<Transform>(spline.handles);
        handles.Add(newHandle.transform);
        spline.handles = handles.ToArray();

    }

}
