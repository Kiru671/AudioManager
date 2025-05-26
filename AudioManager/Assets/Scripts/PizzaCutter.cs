using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PizzaCutter : MonoBehaviour
{
    public MeshFilter meshFilter;
    public float width;
    public float maxDiff;
    public DepthType type;
    public float textureLengthRatio;

    public void RearrangeVertices()
    {
        Mesh mesh = meshFilter.sharedMesh;
        Vector2[] uvs = mesh.uv;
        Vector3[] vertices = mesh.vertices;
        Vector3[] newVertices = new Vector3[vertices.Length];

        for(int i = 0; i < vertices.Length; i++)
        {
            float distRatio = vertices[i].x / width;
            float toMove = distRatio * maxDiff;
            float finalZ = Mathf.Clamp((Mathf.Abs(vertices[i].z) + toMove), 0, Mathf.Infinity) * Mathf.Sign(vertices[i].z);
            Vector3 newPos = new Vector3(vertices[i].x, vertices[i].y, finalZ);
            newVertices[i] = newPos;

            float zDiff = newVertices[i].z - vertices[i].z;
            float diffRatio = zDiff / (mesh.bounds.extents.z * 2);
            float uvMoveAmount = diffRatio * textureLengthRatio;
            if (type == DepthType.u)
                uvs[i] += new Vector2(uvMoveAmount, 0);
            else
                uvs[i] += new Vector2(0, uvMoveAmount);
        }

        mesh.uv = uvs;
        mesh.vertices = newVertices;
        mesh.RecalculateBounds();
    }



}

public enum DepthType
{
    u,
    v
}