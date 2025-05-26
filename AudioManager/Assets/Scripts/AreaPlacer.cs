#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AreaPlacer : MonoBehaviour
{
    public Transform[] points;
    public Vertex[] vertices;
    public Triangle[] tris;
    public float totalArea;
    public int maxPlacementTry;
    [SerializeField]
    public SpawnObject[] spawnObjects;
    [SerializeField]
    public List<GameObject>[] spawnedObjects;
    public GameObject[] parents;
    public GameObject parent;

    // Start is called before the first frame update
    void Start()
    {
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (tris != null && tris.Length > 0)
        {
            for(int i = 0; i < tris.Length; i++)
            {
                Gizmos.DrawLine(tris[i].verts[0].tr.position, tris[i].verts[1].tr.position);
                Gizmos.DrawLine(tris[i].verts[1].tr.position, tris[i].verts[2].tr.position);
                Gizmos.DrawLine(tris[i].verts[2].tr.position, tris[i].verts[0].tr.position);
            }
        }

        Gizmos.color = Color.blue;
        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawLine(vertices[i].tr.position, vertices[i].next.tr.position);
        }

    }

    public void DeleteObjects()
    {
        for (int j = 0; j < spawnedObjects.Length; j++)
        {
            for (int l = 0; l < spawnedObjects[j].Count; l++)
            {
                Undo.DestroyObjectImmediate(spawnedObjects[j][l]);
            }
        }

        Undo.RegisterCompleteObjectUndo(this, "Changed Array");
        EditorUtility.SetDirty(this);
        for (int i = 0; i < spawnedObjects.Length; i++)
        {
            spawnedObjects[i] = new List<GameObject>();
        }
    }
     
    public void SpawnObjects(int id)
    {
        SpawnObject obj = spawnObjects[id];

        if(parents == null)
        {
            parents = new GameObject[spawnObjects.Length];
        }
        for(int i = 0; i < parents.Length; i++)
        {
            parents[i] = Instantiate(parent, transform);
        }

        if(spawnedObjects == null)
        {
            spawnedObjects = new List<GameObject>[spawnObjects.Length];
        }
        for(int i = 0; i < spawnedObjects.Length; i++)
        {
            if(spawnedObjects[i] == null)
            {
                spawnedObjects[i] = new List<GameObject>();
            }
        }

        if(totalArea / obj.minDistance < obj.density)
        {
            obj.density = totalArea / obj.minDistance;
            Debug.LogWarning("Area was too small for given density, reducing density to " + obj.density);
        }

        int toSpawn = Mathf.FloorToInt(totalArea * obj.density);
        Debug.Log("Spawn Count : " + toSpawn);

        for(int i = 0; i < toSpawn; i++)
        {
            int tries = maxPlacementTry;
            while(tries > 0)
            {
                bool conflict = false;
                Triangle spawnIn = tris[Random.Range(0, tris.Length)];
                float toScale = Random.Range(spawnObjects[id].scaleRange.x, spawnObjects[id].scaleRange.y);
                float toRotate = Random.Range(spawnObjects[id].yRotationRange.x, spawnObjects[id].yRotationRange.y);
                // Select a random position in a random triangles bounding box
                Vector3 rayPosition = new Vector3(Random.Range(spawnIn.left, spawnIn.right), transform.position.y + 11, Random.Range(spawnIn.down, spawnIn.up));
                Debug.Log("Ray Position : " + rayPosition);

                // Raycast down to see where it lands
                RaycastHit hit;
                if(Physics.Raycast(rayPosition, Vector3.down, out hit))
                {
                    Vector3 spawnPoint = hit.point;
                    Debug.Log(spawnPoint);

                    //Check to see if it is actually inside the triangle
                    if (PointInTriangle(spawnPoint, spawnIn.verts[0].tr.position, spawnIn.verts[1].tr.position, spawnIn.verts[2].tr.position))
                    {

                        // Check to see if the point is too close to any already exisiting objects
                        for (int j = 0; j < spawnedObjects.Length; j++)
                        {
                            for(int l = 0; l < spawnedObjects[j].Count; l++)
                            {
                                float posDif = (spawnedObjects[j][l].transform.position - spawnPoint).magnitude;
                                float minOther = spawnObjects[j].minDistance * spawnedObjects[j][l].transform.localScale.x;
                                float minSelf = spawnObjects[id].minDistance * toScale;
                                if (posDif < minOther || posDif < minSelf)
                                {
                                    conflict = true;
                                    tries--;
                                }
                            }
                        }
                        if (!conflict)
                        {
                            GameObject spawn = Instantiate(obj.spawnObject, transform);
                            Undo.RegisterCreatedObjectUndo(spawn, "SpawnedObject");
                            spawn.transform.position = spawnPoint;
                            spawn.transform.localScale = new Vector3(toScale, toScale, toScale);
                            Vector3 prevRot = spawn.transform.localEulerAngles;
                            spawn.transform.localEulerAngles = new Vector3(prevRot.x, toRotate, prevRot.z);
                            spawnedObjects[id].Add(spawn);
                            break;
                        }
                    }
                }
            }
        }
    }

    public void EnableCollider(GameObject obj)
    {
        if(obj.GetComponent<BoxCollider>() != null)
        {
            obj.GetComponent<BoxCollider>().enabled = true;
        }
    }

    public void InitVertices()
    {
        vertices = new Vertex[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            vertices[i] = new Vertex(points[i]);
            if (i != 0)
            {
                vertices[i].prev = vertices[i - 1];
                vertices[i - 1].next = vertices[i];
            }
            if (i == points.Length - 1)
            {
                vertices[i].next = vertices[0];
                vertices[0].prev = vertices[i];
            }
        }
    }

    public void Triangulate()
    {
        InitVertices();
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].CalculateAngle();
        }

        int remaining = vertices.Length;
        Vertex current = vertices[0];
        List<Triangle> triangles = new List<Triangle>();
        while(remaining > 3)
        {
            if (CheckEar(current, vertices))
            {
                // Add this vert and its neighbours as a triangle
                triangles.Add(new Triangle(current.prev, current, current.next));
                //-----------------------------------------------------------

                // Remove this vertex from the loop and advance to the next one
                remaining--;
                current.prev.next = current.next;
                current.next.prev = current.prev;
                current = current.next;
                //------------------------------------------------------------
            }
            else
            {
                current = current.next;
            }
        }
        // Add the last triangle and send the result back, then reset the vertices
        triangles.Add(new Triangle(current.prev, current, current.next));
        tris = triangles.ToArray();
        InitVertices();
        //------------------------------------------------------------

        // Calculate the total area
        totalArea = 0;
        for(int i = 0; i < tris.Length; i++)
        {
            tris[i].CalculateArea();
            totalArea += tris[i].area;
        }
        //------------------------------------------------------------
    }

    public bool CheckEar(Vertex vert, Vertex[] all)
    {

        if(vert.angle > 0) // Checking to see if the vertex triangle in convex
        {
            return false;
        }

        for (int i = 0; i < all.Length; i++) // Looping through other verts to see if they are contained in the triangle
        {
            if (all[i] != vert.prev && all[i] != vert && all[i] != vert.next) // Ignoring verts of the trianlge
            {
                if (PointInTriangle(all[i].tr.position, vert.prev.tr.position, vert.tr.position, vert.next.tr.position))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public static bool PointInTriangle(Vector3 p, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        var s = (p0.x - p2.x) * (p.z - p2.z) - (p0.z - p2.z) * (p.x - p2.x);
        var t = (p1.x - p0.x) * (p.z - p0.z) - (p1.z - p0.z) * (p.x - p0.x);

        if ((s < 0) != (t < 0) && s != 0 && t != 0)
            return false;

        var d = (p2.x - p1.x) * (p.z - p1.z) - (p2.z - p1.z) * (p.x - p1.x);
        return d == 0 || (d < 0) == (s + t <= 0);
    }

}

public class Triangle
{
    public Vertex[] verts;
    public float area;
    public float left, right, up, down;

    public Triangle(Vertex one, Vertex two, Vertex three)
    {
        verts = new Vertex[3];
        verts[0] = one;
        verts[1] = two;
        verts[2] = three;
    }

    public void CalculateArea()
    {
        left = verts[0].tr.position.x;
        right = verts[0].tr.position.x;
        up = verts[0].tr.position.z;
        down = verts[0].tr.position.z;
        for (int i = 1; i < verts.Length; i++)
        {
            left = verts[i].tr.position.x < left ? verts[i].tr.position.x : left;
            right = verts[i].tr.position.x > right ? verts[i].tr.position.x : right;
            up = verts[i].tr.position.z > up ? verts[i].tr.position.z : up;
            down = verts[i].tr.position.z < down ? verts[i].tr.position.z : down;
        }

        float horizontal = right - left;
        float vertical = up - down;
        area = (vertical * horizontal) / 2;
    }
}

public class Vertex
{
    public Vertex prev, next;
    public Transform tr;
    public float angle;

    public Vertex(Transform transform)
    {
        tr = transform;
    }

    public Vertex(Transform transform, Vertex previous, Vertex nextVert)
    {
        tr = transform;
        prev = previous;
        next = nextVert;
    }

    public void CalculateAngle()
    {
        Vector2 pos = new Vector2(tr.position.x, tr.position.z);
        Vector2 prevPos = new Vector2(prev.tr.position.x, prev.tr.position.z);
        Vector2 nextPos = new Vector2(next.tr.position.x, next.tr.position.z);
        Vector2 prevDir = prevPos - pos;
        Vector2 nextDir = nextPos - pos;
        float dot = Vector2.Dot(prevDir, nextDir);
        float det = prevDir.x * nextDir.y - prevDir.y * nextDir.x;
        angle = Mathf.Atan2(det, dot);
        angle *= Mathf.Rad2Deg;
    }
}

[System.Serializable]
public class SpawnObject
{
    public GameObject spawnObject;
    public float density;
    public float minDistance;
    public Vector2 scaleRange;
    public Vector2 yRotationRange;
}

public class DistCheck : MonoBehaviour
{
    public float minDist;
}

[CustomEditor(typeof(AreaPlacer))]
public class PolygonShaperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AreaPlacer shaper = (AreaPlacer)target;

        if(GUILayout.Button("Draw Lines"))
        {
            shaper.InitVertices();
        }

        if(GUILayout.Button("Place Objects"))
        {
            shaper.Triangulate();
            for(int i = 0; i < shaper.spawnObjects.Length; i++)
            {
                shaper.SpawnObjects(i);
            }
        }

        if(GUILayout.Button("Delete Objects"))
        {
            shaper.DeleteObjects();
        }

    }
}
#endif