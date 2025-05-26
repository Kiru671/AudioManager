using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class TerrainEditor : MonoBehaviour
{
    public Vector3 terrainOrigin;
    public float terrainSize;
    public int terrainRes;
    public Terrain terrain;
    public Transform cube;
    public float cubeSize;
    public bool[,] overriden, inRange;
    public float[,] savedValue, weight, prevWeight, targetHeight, prevHeight;

    // Start is called before the first frame update
    void Start()
    {
        EditorApplication.quitting += OnQuit;
    }

    // Update is called once per frame
    void Update()
    {
        ChangeTerrain();
    }

    public void UpdateTerrainData()
    {
        terrainRes = terrain.terrainData.heightmapResolution;
        terrainOrigin = terrain.transform.position;
        if (inRange.GetLength(0) < terrainRes)
            inRange = new bool[terrainRes, terrainRes];
    }

    public void ChangeTerrain()
    {
        if (terrain == null)
            return;

        float terrainY = terrain.transform.position.y;

        float totalHeight = terrain.terrainData.heightmapScale.y;
        int size = terrain.terrainData.heightmapResolution;

        if(overriden == null || overriden.GetLength(0) < size || targetHeight == null || targetHeight.GetLength(0) < size)
        {
            overriden = new bool[size, size];
            weight = new float[size, size];
            prevWeight = new float[size, size];
            prevHeight = new float[size, size];
            savedValue = new float[size, size];
            inRange = new bool[size, size];
            targetHeight = new float[size, size];
        }

        float stepPerUnit = size / terrainSize;

        Debug.Log("Size : " + size);

        float[,] heights = terrain.terrainData.GetHeights(0, 0, size, size);
        for(int x = 0; x < size; x++)
        {
            for(int y = 0; y < size; y++)
            {
                if (inRange[x, y] && !overriden[x, y])
                {
                    savedValue[x, y] = heights[x, y];
                    overriden[x, y] = true;
                    heights[x, y] = Mathf.Lerp((targetHeight[x, y] - terrainY) * (1f / totalHeight), savedValue[x, y], weight[x, y]);
                }
                else if (inRange[x, y] && overriden[x, y] &&
                ((prevWeight != null && weight[x, y] != prevWeight[x, y]) || (prevHeight != null) && targetHeight[x,y] != prevHeight[x,y]))
                {
                    heights[x, y] = Mathf.Lerp((targetHeight[x, y] - terrainY) * (1f / totalHeight), savedValue[x, y], weight[x, y]);
                }
                if (!inRange[x, y] && overriden[x, y])
                {
                    heights[x, y] = savedValue[x, y];
                    overriden[x, y] = false;
                }
            }
        }
        prevHeight = targetHeight;
        prevWeight = weight;
        terrain.terrainData.SetHeights(0, 0, heights);
        Debug.Log("Weight 0 = " + weight[0, 0]);
    }

    public void ResetTerrain()
    {

        int size = terrain.terrainData.heightmapResolution;
        float[,] heights = terrain.terrainData.GetHeights(0, 0, size, size);

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (overriden[x, y])
                {
                    heights[x, y] = savedValue[x, y];
                }
            }
        }

        terrain.terrainData.SetHeights(0, 0, heights);

    }

    public void OnQuit()
    {
        ResetTerrain();
    }

}
