using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereNormalTransfer : MonoBehaviour
{
    public Transform center;
    public MeshFilter[] meshes;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            for(int i = 0; i < meshes.Length; i++)
            {
                SpherizeNormals(meshes[i]);
            }
        }
    }

    public void SpherizeNormals(MeshFilter mesh)
    {
        Mesh target = mesh.mesh;
        Vector3[] vertices = target.vertices;
        Vector3[] normals = target.normals;

        for(int i = 0; i < vertices.Length; i++)
        {
            Vector3 realPos = vertices[i] + mesh.transform.position;
            normals[i] = (realPos - center.position);
        }
        target.normals = normals;
    }


}
