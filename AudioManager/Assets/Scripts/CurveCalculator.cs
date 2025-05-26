using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CurveCalculator : MonoBehaviour
{
    public Vector2 resolution;
    public int stepsPerPixel;
    public Texture2D texture;
    
    public void Start()
    {
        float start = Time.realtimeSinceStartup;
        CalculateScreenUVs();
        float end = Time.realtimeSinceStartup;
        Debug.Log("Finished in : " + (end - start) + "seconds");
    }

    public float CalculateCurvePos(float x)
    {
        if (x == 1)
            return 0;
        
        float y = Mathf.Sqrt(Mathf.Sin(Mathf.PI * x)) / 4f;
        //Debug.Log("Curve pos at " + x +" is : " + y);
        return y;
    }

    public void CalculateScreenUVs()
    {
        float[] xPositions = new float[(int)resolution.x];
        float totalXDistance = 0.0f;
        
        CalculateCurveLengthAndPoints((int)resolution.x, stepsPerPixel, out xPositions, out totalXDistance);
        
        float[] yPositions = new float[(int)resolution.y];
        float totalYDistance = 0.0f;
        
        CalculateCurveLengthAndPoints((int)resolution.y, stepsPerPixel, out yPositions, out totalYDistance);
        
        Debug.Log("Total x dist : " + totalXDistance);
        Debug.Log("Total y dist : " + totalYDistance);
        
        WriteToTexture(xPositions, yPositions, totalXDistance, totalYDistance);
    }

    public void WriteToTexture(float[] xPositions, float[] yPositions, float xDist, float yDist)
    {
        texture.Reinitialize((int)resolution.x, (int)resolution.y);
        texture.Apply();
        for (int i = 0; i < xPositions.Length; ++i)
        {
            for (int j = 0; j < yPositions.Length; ++j)
            {
                float x = i == 0 ? 0 : xPositions[i] / xDist;
                float y = j == 0 ? 0 : yPositions[j] / yDist;
                texture.SetPixel((int)i, (int)j, new Color(x, y, 0, 1));
            }
        }
        texture.Apply();
    }
    
    public void CalculateCurveLengthAndPoints(int length, int steps, out float[] points, out float totalLength)
    {
        float[] tempPoints = new float[length];
        float tempTotalLength = 0.0f;
        Vector2 prevPos = Vector2.zero;
        for (int i = 0; i < length; ++i)
        {
            float start = i / (float)length;
            float end = (i + 1) / (float)length;
            for (int j = 0; j <= steps; j++)
            {
                float x = Mathf.Lerp(start,end,j/(float)steps);
                float y = CalculateCurvePos(x);
                tempTotalLength += Vector2.Distance(prevPos, new Vector2(x, y));
                prevPos = new Vector2(x, y);
            }
            tempPoints[i] = tempTotalLength;
        }
        points = tempPoints;
        totalLength = tempTotalLength;
    }
    
}
