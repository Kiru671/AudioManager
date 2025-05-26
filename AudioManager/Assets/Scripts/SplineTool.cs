#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class SplineTool : MonoBehaviour
{
    public Transform start, end;
    public Transform[] handles;

    public float stepSize, width, outerWidth, pieceLength, heightOffset;
    public GameObject roadPiece, snapPiece;
    public GameObject handlePrefab;
    public PizzaCutter cutter;
    public bool smooth;
    public float maxSmoothAngle;
    public TerrainEditor[] editors;

    public bool snap;
    public Transform startSnap, endSnap;
    private Transform selfStartSnap, selfEndSnap;
    private Vector3 selfStartLocal, selfEndLocal;

    [HideInInspector]
    public bool startSnapped, endSnapped;
    private float[] distances, rightDistances;
    public Vector3[] linePoints, rightLinePoints, leftLinePoints, outerRights, outerLefts;

    private Vector3[] pieceRightBounds;
    private GameObject[] currentRoadPieces;
    private GameObject generatedRoad;
    public int[] backSnapVertices, frontSnapVertices;
    public int[] backSnappedTo, frontSnappedTo;

    private Vector3[] lastHandlePositions;
    private float lastStepSize, lastWidth, lastOuterWidth, lastOffset;


    public Vector3 GetCurvePosition(float t) // Get world position of the curve at progress t(0-1)
    {
        Vector3[] currentPoints = new Vector3[handles.Length + 2];

        currentPoints[0] = start.position;
        for (int i = 0; i < handles.Length; i++)
        {
            currentPoints[i + 1] = handles[i].position;
        }
        currentPoints[currentPoints.Length - 1] = end.position;

        while(currentPoints.Length > 1)
        {
            Vector3[] temp = new Vector3[currentPoints.Length - 1];
            for(int i = 0; i < currentPoints.Length - 1; i++)
            {
                temp[i] = Vector3.Lerp(currentPoints[i], currentPoints[i + 1], t);
            }
            currentPoints = temp;
        }

        return currentPoints[0];
    }

    public void GetCurrentLine() // Draw and store current line
    {
        float currentLength = 0;
        List<Vector3> tempPoints = new List<Vector3>();
        tempPoints.Add(start.position);
        while(currentLength < 1 && stepSize > 0)
        {
            currentLength = Mathf.Clamp(currentLength + stepSize, 0, 1);
            tempPoints.Add(GetCurvePosition(currentLength));
        }
        linePoints = tempPoints.ToArray();

        distances = new float[linePoints.Length];
        for(int i = 0; i < linePoints.Length - 1; i++)
        {
            distances[i] = Vector3.Distance(linePoints[i], linePoints[i + 1]);
        }
        
    }

    public void GetSideLines()
    {
        outerRights = new Vector3[linePoints.Length];
        outerLefts = new Vector3[linePoints.Length];
        rightLinePoints = new Vector3[linePoints.Length];
        leftLinePoints = new Vector3[linePoints.Length];
        Vector3 currentDirection = Vector3.zero;
        for (int i = 0; i < linePoints.Length; i++)
        {
            if(i < linePoints.Length - 1) // Dont update for last point
            currentDirection = (linePoints[i + 1] - linePoints[i]).normalized;

            Vector3 rightDirection = new Vector3(currentDirection.z, 0, -currentDirection.x).normalized;
            Vector3 leftDirection = new Vector3(-currentDirection.z, 0, currentDirection.x).normalized;

            rightLinePoints[i] = linePoints[i] + (width * rightDirection);
            leftLinePoints[i] = linePoints[i] + (width * leftDirection);
            outerRights[i] = linePoints[i] + ((width + outerWidth) * rightDirection);
            outerLefts[i] = linePoints[i] + ((width + outerWidth) * leftDirection);
        }

    }

    public void GetTerrainOverlap(TerrainEditor editor)
    {
        editor.UpdateTerrainData();
        int dim = editor.terrainRes;
        float stepSize = editor.terrainSize / dim;
        bool[,] included = new bool[dim, dim];
        float[,] weight = new float[dim, dim];
        float[,] heights = editor.terrain.terrainData.GetHeights(0, 0, dim, dim);
        // CHECK EACH RECTANGLE THATS CONTAINED WITHIN THE LINE TO FIND ALL POINTS
        for (int i = 0; i < rightLinePoints.Length - 1; i++)
        {
            // DEFINE LINES THAT CONSTURCT THE RECTANGLE
            Vector3[,] lines = new Vector3[4,2];

            lines[0, 0] = outerRights[i];
            lines[0, 1] = outerRights[i + 1];

            lines[1, 0] = outerRights[i + 1];
            lines[1, 1] = outerLefts[i + 1];

            lines[2, 0] = outerLefts[i + 1];
            lines[2, 1] = outerLefts[i];

            lines[3, 0] = outerLefts[i];
            lines[3, 1] = outerRights[i];

            Vector3 midPoint = linePoints[i];

            // DEFINE THE BOUNDING BOX TO SKIP CHECKING IMPOSSIBLE POINTS
            Vector2 xBounds = new Vector2(lines[0, 0].x, lines[0,0].x);
            Vector2 yBounds = new Vector2(lines[0, 0].z, lines[0, 0].z);
            for(int j = 0; j < 4; j++)
            {
                if (lines[j, 0].x < xBounds.x)
                    xBounds.x = lines[j, 0].x;
                if (lines[j, 0].x > xBounds.y)
                    xBounds.y = lines[j, 0].x;
                if (lines[j, 1].x < xBounds.x)
                    xBounds.x = lines[j, 1].x;
                if (lines[j, 1].x > xBounds.y)
                    xBounds.y = lines[j, 1].x;

                // Get x length and use curvature at that length to find height
                // Get height difference here
                // Use diff in height changes to calculate height of road at point
                // Send all gathered height to editor arrays after calculating
                // Change all height amounts after gathering data

                if (lines[j, 0].z < yBounds.x)
                    yBounds.x = lines[j, 0].z;
                if (lines[j, 0].z > yBounds.y)
                    yBounds.y = lines[j, 0].z;
                if (lines[j, 1].z < yBounds.x)
                    yBounds.x = lines[j, 1].z;
                if (lines[j, 1].z > yBounds.y)
                    yBounds.y = lines[j, 1].z;
            }

            // CHECK EACH POINT

            for(int x = 0; x < dim; x++)
            {
                for (int y = 0; y < dim; y++)
                {
                    Vector2 pos = new Vector2(editor.terrainOrigin.x + (x * stepSize), editor.terrainOrigin.z + (y * stepSize));

                    if(xBounds.x < pos.x && pos.x < xBounds.y && yBounds.x < pos.y && pos.y < yBounds.y) // IF POINT IS WITHIN BOUNDS
                    {
                        
                        // CHECK CONTAINMENT WITH RAYCAST ALGORITHM
                        int intersection = 0;
                        for (int j = 0; j < 4; j++)
                        {
                            Vector3 diff = lines[j, 1] - lines[j, 0];

                            float a = diff.z / diff.x;
                            float b = lines[j, 0].z - (a * lines[j, 0].x);

                            float lineX = (pos.y - b) / a;
                            if (pos.x < lineX)
                                intersection++;
                            if (diff.x < 0.001f || diff.z < 0.001f)
                            {
                                intersection = 1;
                            }

                        }
                        if (intersection % 2 == 1)
                        {
                            included[y, x] = true;
                            // ax+b is the line and cx+d is the normal
                            float a = (lines[3, 1].z - lines[3, 0].z) / (lines[3, 1].x - lines[3, 0].x);
                            float b = lines[3, 0].z - (a * lines[3, 0].x);

                            float c = (1f / a) * -1f;
                            float d = pos.y - c * pos.x;

                            float intersectionX = (b - d) / (c - a);
                            Vector3 intersectionPoint = new Vector3(intersectionX, midPoint.y, (a * intersectionX) + b);

                            float dist = Vector3.Distance(intersectionPoint, midPoint);
                            weight[y, x] = dist <= width ? 0 : (dist - width) / outerWidth;

                            // Finding height
                            float rightDist = Vector2.Distance(new Vector2(lines[0, 0].x, lines[0,0].z), new Vector2(lines[0, 1].x, lines[0, 1].z));
                            float leftDist = Vector2.Distance(new Vector2(lines[2, 0].x, lines[2, 0].z), new Vector2(lines[2, 1].x, lines[2, 1].z));
                            float heightDiff = lines[0, 0].y - lines[0, 1].y;
                            float rightSlope = heightDiff / rightDist;
                            float leftSlope = heightDiff / leftDist;
                            float interToRight = Vector3.Distance(intersectionPoint, lines[0, 0]);
                            float slope = Mathf.Lerp(rightSlope, leftSlope, interToRight / outerWidth);
                            float slopeTravelled = Vector2.Distance(pos, new Vector2(intersectionPoint.x, intersectionPoint.z));
                            float height = lines[0, 0].y + (slope * slopeTravelled);
                            heights[y, x] = height + heightOffset;
                        }
                    }
                }
            }
            editor.weight = weight;
            editor.inRange = included;
            editor.targetHeight = heights;
        }
    }

    public void PlaceRoads()
    {
        if(generatedRoad != null)
            DestroyImmediate(generatedRoad);
        
        float targetLength = pieceLength/2f;
        float currentDistance = 0;
        List<GameObject> pieces = new List<GameObject>();
        // Place and orient all road pieces at close to perfect intervals
        for(int i = 0; i < distances.Length; i++)
        {
            currentDistance += distances[i];
            if(currentDistance > targetLength)
            {
                targetLength += pieceLength;
                GameObject piece = Instantiate(roadPiece, transform);
                CreateUniqueMesh(piece.GetComponent<MeshFilter>());
                pieces.Add(piece);
                piece.transform.position = linePoints[i];
                Vector3 forward = linePoints[i + 1] - linePoints[i];
                piece.transform.forward = forward;
            }
        }
        currentRoadPieces = pieces.ToArray();

        // Get front and back snap vertex ids for currently used road piece
        GameObject testPiece = Instantiate(roadPiece, transform);
        CreateUniqueMesh(testPiece.GetComponent<MeshFilter>());
        GameObject snap = Instantiate(snapPiece, testPiece.transform);
        CreateUniqueMesh(snap.GetComponent<MeshFilter>());
        
        selfEndLocal = testPiece.transform.GetChild(1).localPosition;
        snap.transform.localPosition = selfEndLocal;
        backSnappedTo = new int[1];
        backSnapVertices = FindSnapVertices(testPiece, snap, ref backSnappedTo);
        
        selfStartLocal = testPiece.transform.GetChild(0).localPosition;
        snap.transform.localPosition = selfStartLocal;
        frontSnappedTo = new int[1];
        frontSnapVertices = FindSnapVertices(testPiece, snap, ref frontSnappedTo);
        
        DestroyImmediate(testPiece);
        DestroyImmediate(snap);

        // Get points to the right of the pieces at the start and end points to use for shortening
        targetLength = pieceLength;
        currentDistance = 0;
        int index = 0;
        pieceRightBounds = new Vector3[currentRoadPieces.Length];
        pieceRightBounds[index] = rightLinePoints[0];
        index++;
        for(int i = 0; i < distances.Length; i++)
        {
            currentDistance += distances[i];
            if (currentDistance > targetLength && index < pieceRightBounds.Length)
            {
                targetLength += pieceLength;
                pieceRightBounds[index] = rightLinePoints[i];
                index++;
            }
        }
        
        rightDistances = new float[pieceRightBounds.Length - 1];
        for(int i = 0; i < pieceRightBounds.Length - 1; i++)
        {
            rightDistances[i] = Vector3.Distance(pieceRightBounds[i], pieceRightBounds[i + 1]);
        }

        for(int i = 0; i < currentRoadPieces.Length - 1; i++)
        {
            cutter.meshFilter = currentRoadPieces[i].GetComponent<MeshFilter>();
            cutter.width = width;
            cutter.maxDiff = (rightDistances[i] - pieceLength) / 2f;
            cutter.RearrangeVertices();
        }

        StitchMeshes();
        GenerateSelfSnaps();
        CombineMeshes();

        selfStartSnap.transform.parent = generatedRoad.transform;
        selfEndSnap.transform.parent = generatedRoad.transform;
    }

    public void CreateUniqueMesh(MeshFilter filter)
    {
        filter.sharedMesh = Instantiate(filter.sharedMesh);
    }
    
    public int[] FindSnapVertices(GameObject piece, GameObject snap, ref int[] targetList)
    {
        Mesh pieceMesh = piece.GetComponent<MeshFilter>().sharedMesh;
        Mesh snapMesh = snap.GetComponent<MeshFilter>().sharedMesh;
        List<int> snapVertices = new List<int>();
        List<int> snappedTo = new List<int>();

        for(int i = 0; i < snapMesh.vertexCount; i++)
        {
            Vector3 snapPos = snap.transform.TransformPoint(snapMesh.vertices[i]);
            for(int j = 0; j < pieceMesh.vertexCount; j++)
            {
                Vector3 piecePos = piece.transform.TransformPoint(pieceMesh.vertices[j]);
                if (Vector3.Distance(snapPos, piecePos) < 0.0001f)
                {
                    snapVertices.Add(j);
                    snappedTo.Add(i);
                }
            }
        }
        targetList = snappedTo.ToArray();
        int[] ret = snapVertices.ToArray();
        return ret;
    }

    public int[] FindSnapVertices(GameObject piece, GameObject snap)
    {
        Mesh pieceMesh = piece.GetComponent<MeshFilter>().sharedMesh;
        Mesh snapMesh = snap.GetComponent<MeshFilter>().sharedMesh;
        List<int> snapVertices = new List<int>();

        for(int i = 0; i < snapMesh.vertexCount; i++)
        {
            Vector3 snapPos = snap.transform.TransformPoint(snapMesh.vertices[i]);
            for(int j = 0; j < pieceMesh.vertexCount; j++)
            {
                Vector3 piecePos = piece.transform.TransformPoint(pieceMesh.vertices[j]);
                if (Vector3.Distance(snapPos, piecePos) < 0.0001f)
                {
                    snapVertices.Add(j);
                }
            }
        }
        int[] ret = snapVertices.ToArray();
        return ret;
    }
    
    public void StitchMeshes()
    {
        for(int i = 0; i < currentRoadPieces.Length - 1; i++)
        {
            SnapMesh(currentRoadPieces[i], currentRoadPieces[i + 1], frontSnapVertices, backSnapVertices);
        }
        if (startSnapped)
        {
            int[] targetVerts = FindSnapVertices(startSnap.parent.gameObject, startSnap.gameObject);
            SnapMesh(currentRoadPieces[0], startSnap.parent.gameObject, backSnapVertices, targetVerts);
        }

        if (endSnapped)
        {
            int[] targetVerts = FindSnapVertices(endSnap.parent.gameObject, endSnap.gameObject);
            SnapMesh(currentRoadPieces[^1], endSnap.parent.gameObject, frontSnapVertices, targetVerts);
        }
    }

    public void GenerateSelfSnaps()
    {
        GameObject firstPiece = currentRoadPieces[0];
        GameObject lastPiece = currentRoadPieces[^1];
        
        GameObject selfStart = Instantiate(snapPiece, firstPiece.transform);
        CreateUniqueMesh(selfStart.GetComponent<MeshFilter>());
        selfStart.name = "StartSnap";
        // Generate a base array of consecutive integers the length of the mesh vertices and get their counterparts
        int vertCount = selfStart.GetComponent<MeshFilter>().sharedMesh.vertexCount;
        int[] frontVerts = new int[vertCount];
        int[] backVerts = new int[vertCount];
        int[] meshVerts = new int[vertCount];
        for (int i = 0; i < frontVerts.Length; i++)
        {
            meshVerts[i] = i;
            for (int j = 0; j < frontSnappedTo.Length; j++)
            {
                if(frontSnappedTo[j] == i)
                    frontVerts[i] = frontSnapVertices[j];
                if(backSnappedTo[j] == i)
                    backVerts[i] = backSnapVertices[j];
            }
        }
        
        SnapMesh(selfStart, firstPiece, meshVerts, backVerts);
        selfStart.transform.parent = firstPiece.transform.parent;
        
        GameObject selfEnd = Instantiate(snapPiece, lastPiece.transform);
        CreateUniqueMesh(selfEnd.GetComponent<MeshFilter>());
        SnapMesh(selfEnd, lastPiece, meshVerts, frontVerts);
        selfEnd.transform.parent = lastPiece.transform.parent;
        selfEnd.name = "EndSnap";
        
        selfEndSnap = selfEnd.transform;
        selfStartSnap = selfStart.transform;

    }
    
    public void SnapMesh(GameObject baseObj, GameObject target, int[] baseVerts, int[] targetVerts)
    {
        Mesh mesh1 = baseObj.GetComponent<MeshFilter>().sharedMesh;
        Mesh mesh2 = target.GetComponent<MeshFilter>().sharedMesh;


        Vector3[] verts1 = mesh1.vertices;
        Vector3[] verts2 = mesh2.vertices;

        Vector3[] normals1 = mesh1.normals;
        Vector3[] normals2 = mesh2.normals;

        for (int j = 0; j < baseVerts.Length; j++)
        {
            Vector3 snapTo = target.transform.TransformPoint(verts2[targetVerts[j]]);
            verts1[baseVerts[j]] = baseObj.transform.InverseTransformPoint(snapTo);

            if (smooth)
            {
                Vector3 normal1 = baseObj.transform.TransformDirection(normals1[baseVerts[j]]);
                Vector3 normal2 = target.transform.TransformDirection(normals2[targetVerts[j]]);
                if (Mathf.Abs(Vector3.Angle(normal1, normal2)) < maxSmoothAngle)
                {
                    Vector3 newNormal = (normal1 + normal2) / 2f;
                    normals1[baseVerts[j]] = baseObj.transform.InverseTransformDirection(newNormal);
                    normals2[targetVerts[j]] = target.transform.InverseTransformDirection(newNormal);
                }
            }
        }
        mesh1.normals = normals1;
        mesh2.normals = normals2;
        mesh1.vertices = verts1;
        mesh1.RecalculateBounds();
    }

    public void CombineMeshes()
    {
        GameObject final = Instantiate(roadPiece, transform);
        CreateUniqueMesh(final.GetComponent<MeshFilter>());
        final.name = "Road";
        DestroyImmediate(final.transform.GetChild(0).gameObject);
        DestroyImmediate(final.transform.GetChild(0).gameObject);
        final.transform.localScale = new Vector3(1, 1, 1);
        final.transform.localPosition = -transform.position;
        final.transform.localRotation = transform.rotation;
        Mesh mesh = new Mesh();
        mesh.name = "Road";

        var combine = new CombineInstance[currentRoadPieces.Length];

        for(int i = 0; i < currentRoadPieces.Length; i++)
        {
            combine[i].mesh = currentRoadPieces[i].GetComponent<MeshFilter>().sharedMesh;
            combine[i].transform = currentRoadPieces[i].transform.localToWorldMatrix;
            DestroyImmediate(currentRoadPieces[i]);
        }
        currentRoadPieces = new GameObject[0];

        mesh.CombineMeshes(combine,true,true);

        final.GetComponent<MeshFilter>().sharedMesh = mesh;

        mesh.RecalculateBounds();

        generatedRoad = final;

    }

    public void UpdateSelfSnapTransforms()
    {
        selfStartSnap.transform.localPosition = Vector3.zero;
        Vector3 faceDir = linePoints[1] - linePoints[0];
        selfStartSnap.transform.forward = faceDir;

        selfEndSnap.transform.localPosition = Vector3.zero;
        faceDir = linePoints[linePoints.Length - 1] - linePoints[linePoints.Length - 2];
        selfEndSnap.transform.forward = faceDir;
    }

    public void CheckSnapping()
    {
        if(!startSnapped && snap && startSnap != null)
        {
            SnapToStart();
        }
        if(startSnapped && (!snap || startSnap == null))
        {
            UnsnapToStart();
        }
        if (startSnapped && startSnap.position != start.position)
        {
            start.position = startSnap.position;
        }

        if(!endSnapped && snap && endSnap != null)
        {
            SnapToEnd();
        }
        if(endSnapped && (!snap || endSnap == null))
        {
            UnsnapToEnd();
        }
        if(endSnapped && endSnap.position != end.position)
        {
            end.position = endSnap.position;
        }
    }

    public void SnapToStart()
    {
        startSnapped = true;
        start.GetComponent<MeshRenderer>().enabled = false;
        start.position = startSnap.position;
    }

    public void SnapToEnd()
    {
        endSnapped = true;
        end.GetComponent<MeshRenderer>().enabled = false;
        end.position = endSnap.position;
    }

    public void UnsnapToStart()
    {
        startSnapped = false;
        start.GetComponent<MeshRenderer>().enabled = true;
    }

    public void UnsnapToEnd()
    {
        endSnapped = false;
        end.GetComponent<MeshRenderer>().enabled = true;
    }

    public bool CheckLineChange()
    {
        if (start == null)
            return false;

        bool ret = false;
        Vector3[] currentHandlePositions = GetHandlePositions();

        if(stepSize != lastStepSize || width != lastWidth || outerWidth != lastOuterWidth || heightOffset != lastOffset)
        {
            ret = true;
        }
        else if (lastHandlePositions.Length < 1)
        {
            ret = true;
        }
        else if(currentHandlePositions.Length != lastHandlePositions.Length)
        {
            ret = true;
        }
        else
        {
            for(int i = 0; i < currentHandlePositions.Length; i++)
            {
                if (currentHandlePositions[i] != lastHandlePositions[i])
                {
                    ret = true;
                    break;
                }
            }
        }
        lastOuterWidth = outerWidth;
        lastWidth = width;
        lastOffset = heightOffset;
        lastStepSize = stepSize;
        lastHandlePositions = currentHandlePositions;
        return ret;
    }

    public Vector3[] GetHandlePositions()
    {
        Vector3[] handlePositions = new Vector3[handles.Length + 2];
        handlePositions[0] = start.position;
        for (int i = 0; i < handles.Length; i++)
        {
            handlePositions[i + 1] = handles[i].position;
        }
        handlePositions[handlePositions.Length - 1] = end.position;
        return handlePositions;
    }

    public void UpdateLine()
    {
        Debug.Log("Updating The Line");
        GetCurrentLine();
        GetSideLines();
        for(int i = 0; i < editors.Length; i++)
        {
            GetTerrainOverlap(editors[i]);
        }
    }

    public void OnDrawGizmosSelected()
    {
        CheckSnapping();
        if (CheckLineChange())
        {
            UpdateLine();
        }

        if (start != null && end != null && stepSize > 0)
        {
            for (int i = 0; i < linePoints.Length - 1; i++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(linePoints[i], linePoints[i + 1]);
		Gizmos.color = new Color(0.5f,0.5f,1f,1);
                Gizmos.DrawLine(rightLinePoints[i], rightLinePoints[i + 1]);
                Gizmos.DrawLine(leftLinePoints[i], leftLinePoints[i + 1]);
		Gizmos.color = Color.blue;
                Gizmos.DrawLine(outerRights[i], outerRights[i + 1]);
                Gizmos.DrawLine(outerLefts[i], outerLefts[i + 1]);
            }

        }
    }

}
#endif