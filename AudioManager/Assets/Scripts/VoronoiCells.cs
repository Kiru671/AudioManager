using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoronoiCells : MonoBehaviour
{
    public Vector2 cellCounts;
    public Vector2 texSize;
    public Texture2D tex;
    private Vector2[,] sites;
    private List<Vector2> siteIds;
    private List<Arc> currentArcs;
    private List<Vector2> prevColorCoords;
    private List<Color> prevColors;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            GenerateSites();
            StartCoroutine(VoronoiGeneration());
        }
    }

    public void GenerateSites()
    {
        sites = new Vector2[(int)cellCounts.x, (int)cellCounts.y];
        float xStepSize = texSize.x / cellCounts.x;
        float yStepSize = texSize.y / cellCounts.y;
        for(int i = 0; i < (int)cellCounts.x; i++)
        {
            for(int j = 0; j < (int)cellCounts.y; j++)
            {
                Vector2 pos = new Vector2((int)Random.Range(i * xStepSize, (i + 1) * xStepSize), (int)Random.Range(j * yStepSize, (j + 1) * yStepSize));
                if (pos.x >= texSize.x)
                    pos.x -= 1;
                if (pos.y >= texSize.y)
                    pos.y -= 1;
                sites[i, j] = pos;
            }
        }

        for (int i = 0; i < texSize.x; i++)
        {
            for (int j = 0; j < texSize.y; j++)
            {
                tex.SetPixel(i, j, Color.white);
            }
        }

        for (int i = 0; i < sites.GetLength(0); i++)
        {
            for(int j = 0; j < sites.GetLength(1); j++)
            {
                DrawPoint((int)sites[i, j].x, (int)sites[i, j].y, 4);
            }
        }

        tex.Apply();

    }

    public void GenerateEvents()
    {
        List<VoronoiEvent> events = new List<VoronoiEvent>();
        List<BeachlineObject> beachline = new List<BeachlineObject>();

        Vector2[] singleSites = new Vector2[sites.GetLength(0) * sites.GetLength(1)];
        for(int x = 0; x < sites.GetLength(0); x++) // Add all sites into a single file list
        {
            for(int y = 0; y < sites.GetLength(1); y++)
            {
                singleSites[x + (y * sites.GetLength(0))] = new Vector2(x,y);
            }
        }

        
        for(int s = 0; s < singleSites.Length; s++) // Sort from bottom to top
        {
            float lowest = 999999;
            int lowestId = -1;
            for (int i = s; i < singleSites.Length; i++)
            {
                if (sites[(int)singleSites[i].x, (int)singleSites[i].y].y < lowest)
                {
                    lowestId = i;
                    lowest = sites[(int)singleSites[i].x, (int)singleSites[i].y].y;
                }
            }

            Vector2 temp = singleSites[s];
            singleSites[s] = singleSites[lowestId];
            singleSites[lowestId] = singleSites[s];
        }


        for (int i = 0; i < singleSites.Length; i++)
        {
            events.Add(new VoronoiEvent(EventType.SiteEvent, singleSites[i].y));
            Arc newArc = new Arc(singleSites[i]);
            beachline.Add(newArc);

            bool contained = false;

            for(int j = beachline.Count - 2; j > 0; j--) // Check to see if arc is contained by another arc
            {
                if (beachline[j].arc)
                {
                    Arc toCheck = (Arc)beachline[j];
                    float newCenter = sites[(int)newArc.focusId.x, (int)newArc.focusId.y].x;
                    if (toCheck.limits.x < newCenter && newCenter < toCheck.limits.y)
                    {
                        contained = true;
                        Arc newLeft = new Arc(sites[(int)toCheck.focusId.x, (int)toCheck.focusId.y]);
                        Arc newRight = new Arc(sites[(int)toCheck.focusId.x, (int)toCheck.focusId.y]);
                        newLeft.left = toCheck.left;
                        newLeft.right = newArc;
                        newRight.right = toCheck.right;
                        newRight.left = newArc;
                        newArc.left = newLeft;
                        newArc.right = newRight;
                        Edge leftEdge = new Edge(singleSites[i]);
                        Edge rightEdge = new Edge(singleSites[i]);
                        // Find edge direction
                    }
                }
            }
            if (!contained)
            {
                if(beachline.Count > 0)
                {
                    Arc toCheck = (Arc)beachline[0];
                    float checkX = sites[(int)toCheck.focusId.x, (int)toCheck.focusId.y].x;
                    if(singleSites[i].x < checkX)
                    {
                        
                    }
                }

            }
        }
    }


    /*
     Arc toCheck = (Arc)beachline[j];
                    if (sites[(int)toCheck.focusId.x, (int)toCheck.focusId.y].x < sites[(int)newArc.focusId.x, (int)newArc.focusId.y].x)
                    {
                        beachline.Remove(newArc);
                        beachline.Insert(j, newArc);
                        if(j > 0)
                        {
                            newArc.left = (Arc)beachline[j - 1];
                            // TODO add edges and relations
                        }
                        if(j < beachline.Count - 1)
                        {

                        }
                    }
    */

    public void DrawPoint(int x, int y, int size)
    {
        for(int i = Mathf.Max(x - size, 0);i < Mathf.Min(texSize.x-1, x + size); i++)
        {
            for (int j = Mathf.Max(y - size, 0); j < Mathf.Min(texSize.y - 1, y + size); j++)
            {
                tex.SetPixel(i, j, Color.red);
            }
        }
    }

    public void DrawPointWithPrevSaving(int x, int y, int size, Color color)
    {
        for (int i = Mathf.Max(x - size, 0); i < Mathf.Min(texSize.x - 1, x + size); i++)
        {
            for (int j = Mathf.Max(y - size, 0); j < Mathf.Min(texSize.y - 1, y + size); j++)
            {
                prevColorCoords.Add(new Vector2(i, j));
                prevColors.Add(tex.GetPixel(i, j));

                tex.SetPixel(i, j, color);
            }
        }
    }

    public void AddArc(Vector2 siteId)
    {
        Vector2 pos = sites[(int)siteId.x, (int)siteId.y];
        Arc newArc = new Arc();
        currentArcs.Add(newArc);
        newArc.focusId = siteId;
        newArc.limits = new Vector2(pos.x, pos.x); // The arc starts out as a point

        if(currentArcs.Count == 1)
        {
            newArc.limits = new Vector2(0, texSize.x);
            newArc.left = null;
            newArc.right = null;
            return;
        }

        for(int i = 0; i < currentArcs.Count; i++)
        {
            if(currentArcs[i].limits.x < pos.x && pos.x < currentArcs[i].limits.y) // If the arc is contained by some other arc
            {
                Vector2 prevArcSiteId = currentArcs[i].focusId;
                Vector2 prevArcLimits = currentArcs[i].limits;
                Arc prevArcLeft = currentArcs[i].left;
                Arc prevArcRight = currentArcs[i].right;

                //Split the containing arc into two and insert new arc into the middle
                currentArcs.RemoveAt(i);

                Arc newLeft = new Arc();
                newLeft.focusId = prevArcSiteId;
                newLeft.left = prevArcLeft;
                newLeft.right = newArc;
                newLeft.limits = new Vector2(prevArcLimits.x, pos.x);
                currentArcs.Add(newLeft);

                Arc newRight = new Arc();
                newRight.focusId = prevArcSiteId;
                newRight.right = prevArcRight;
                newRight.left = newArc;
                newRight.limits = new Vector2(pos.x, prevArcLimits.y);
                currentArcs.Add(newRight);

                newArc.left = newLeft;
                newArc.right = newRight;

                currentArcs.Add(newArc);

                break;
            }
        }
        
    }

    public float FindIntersectionX(Vector2 p1, Vector2 p2, float c)
    {
        Vector2 A, B;
        if(p1.y > p2.y)
        {
            A = p1;
            B = p2;
        }
        else
        {
            A = p2;
            B = p1;
        }
        Vector2 AB = new Vector2(B.x - A.x, B.y - A.y);
        float slope = AB.y / AB.x;

        float xDirectrixIntersect = A.x + (c - A.y) / slope;
        Vector2 D = new Vector2(xDirectrixIntersect, c);
        float ADDist = Vector2.Distance(A, D);
        float BDDist = Vector2.Distance(B, D);
        float DtoX = Mathf.Sqrt(ADDist * BDDist);
        float X = D.x + DtoX * Mathf.Sign(slope);
        return X;
    }

    public Vector2 FindIntersectionPoints(Vector2 p1, Vector2 p2, float yd)
    {
        Vector2 ret = new Vector2(0,0);

        float a1 = 1f / (2f * (p1.y - yd));
        float a2 = 1f / (2f * (p2.y - yd));

        float b1 = (-1f * p1.x) / (p1.y - yd);
        float b2 = (-1f * p2.x) / (p2.y - yd);

        float c1 = (p1.x * p1.x) / (2f * (p1.y - yd)) + ((p1.y + yd) / 2f);
        float c2 = (p2.x * p2.x) / (2f * (p2.y - yd)) + ((p2.y + yd) / 2f);

        float a = a1 - a2;
        float b = b1 - b2;
        float c = c1 - c2;

        float x1 = (-1 * b + Mathf.Sqrt((b * b) - (4 * a * c))) / (2 * a);
        float x2 = (-1 * b - Mathf.Sqrt((b * b) - (4 * a * c))) / (2 * a);

        ret = new Vector2(x1, x2);

        return ret;
    }


    IEnumerator VoronoiGeneration()
    {
        int lineProg = 0;
        int stepPerFrame = 10;

        prevColorCoords = new List<Vector2>();
        prevColors = new List<Color>();
        currentArcs = new List<Arc>();
        siteIds = new List<Vector2>();

        while(lineProg < texSize.y)
        {
            // Calculation phase

            for(int i = 0; i < stepPerFrame; i++)
            {
                lineProg += 1;

                for(int x = 0; x < sites.GetLength(0); x++)
                {
                    for(int y = 0; y < sites.GetLength(1); y++)
                    {
                        if(sites[x,y].y == lineProg)
                        {
                            siteIds.Add(new Vector2(x, y));
                            AddArc(new Vector2(x,y));
                            Debug.Log("Added Site X = " + x + " Y = " + y);
                        }
                    }
                }

                for(int j = 0; j < currentArcs.Count; j++)
                {
                    if (currentArcs[j].left != null && currentArcs[j].right != null && currentArcs[j].left.focusId == currentArcs[j].right.focusId)
                    {
                        Vector2 site1 = sites[(int)currentArcs[j].focusId.x, (int)currentArcs[j].focusId.y];
                        Vector2 site2 = sites[(int)currentArcs[j].left.focusId.x, (int)currentArcs[j].left.focusId.y];
                        Vector2 intersects = FindIntersectionPoints(site1, site2, lineProg);
                        if(intersects.x > intersects.y)
                        {
                            float temp = intersects.x;
                            intersects.x = intersects.y;
                            intersects.y = temp;
                        }
                        currentArcs[j].limits = intersects;
                    }
                    else
                    {
                        if(currentArcs[j].left != null)
                        {
                            Vector2 site1 = sites[(int)currentArcs[j].focusId.x, (int)currentArcs[j].focusId.y];
                            Vector2 site2 = sites[(int)currentArcs[j].left.focusId.x, (int)currentArcs[j].left.focusId.y];
                            Vector2 intersects = FindIntersectionPoints(site1, site2, lineProg);
                            float midPoint = (currentArcs[j].limits.x + currentArcs[j].limits.y) / 2f;
                            float toSet = Mathf.Abs(midPoint - intersects.x) < Mathf.Abs(midPoint - intersects.y) ? intersects.x : intersects.y;
                            currentArcs[j].limits.x = toSet;
                            Debug.Log("Left limit of the arc at " + midPoint + " found to be " + currentArcs[j].limits.x);
                        }
                        if (currentArcs[j].right != null)
                        {
                            Vector2 site1 = sites[(int)currentArcs[j].focusId.x, (int)currentArcs[j].focusId.y];
                            Vector2 site2 = sites[(int)currentArcs[j].right.focusId.x, (int)currentArcs[j].right.focusId.y];
                            Vector2 intersects = FindIntersectionPoints(site1, site2, lineProg);
                            float midPoint = (currentArcs[j].limits.x + currentArcs[j].limits.y) / 2f;
                            float toSet = Mathf.Abs(midPoint - intersects.x) < Mathf.Abs(midPoint - intersects.y) ? intersects.x : intersects.y;
                            currentArcs[j].limits.y = toSet;
                            Debug.Log("Right limit of the arc at " + midPoint + " found to be " + currentArcs[j].limits.y);
                        }
                    }

                    if(currentArcs[j].left != null && currentArcs[j].right != null && currentArcs[j].left.limits.y > currentArcs[j].right.limits.x)
                    {
                        currentArcs[j].left.right = currentArcs[j].right;
                        currentArcs[j].right.left = currentArcs[j].left;
                        currentArcs.RemoveAt(j);
                    }
                }








            }

            // Cleaning Phase

            if(prevColorCoords.Count > 0)
            {
                for(int i = prevColorCoords.Count - 1; i >= 0; i--)
                {
                    tex.SetPixel((int)prevColorCoords[i].x, (int)prevColorCoords[i].y, prevColors[i]);
                }
            }

            prevColorCoords.Clear();
            prevColors.Clear();

            // Drawing Phase

            for (int i = 0; i < texSize.x; i++)
            {
                DrawPointWithPrevSaving(i, lineProg, 1, Color.green);
            }

            /* This fully draws all parabolas which is needed only for debug purposes
            for(int i = 0; i < siteIds.Count; i++)
            {
                if(sites[(int)siteIds[i].x, (int)siteIds[i].y].y != lineProg) // The parabola is a line if directrix and focus are on the same height
                {
                    Vector2 focus = sites[(int)siteIds[i].x, (int)siteIds[i].y];
                    for(int x = 0; x < texSize.x; x += 2)
                    {
                        //f(x) formula for the parabola  v
                        float height = ((1f / (2f * (focus.y - lineProg))) * ((x - focus.x) * (x - focus.x))) + ((focus.y + lineProg) / 2f);
                        if(height >= 0 && height < texSize.y) // Only draw if within the texture
                        {
                            DrawPointWithPrevSaving(x, (int)height, 1, Color.black);
                        }
                    }
                }
            } */

            for(int i = 0; i < currentArcs.Count; i++)
            {
                Vector2 focus = sites[(int)currentArcs[i].focusId.x, (int)currentArcs[i].focusId.y];

                if(focus.y != lineProg)
                {
                    for(int x = (int)currentArcs[i].limits.x; x < (int)currentArcs[i].limits.y; x+= 2)
                    {
                        //f(x) formula for the parabola  v
                        float height = ((1f / (2f * (focus.y - lineProg))) * ((x - focus.x) * (x - focus.x))) + ((focus.y + lineProg) / 2f);
                        if (height >= 0 && height < texSize.y) // Only draw if within the texture
                        {
                            DrawPointWithPrevSaving(x, (int)height, 1, Color.black);
                        }
                    }
                }
            }

            tex.Apply();
            yield return new WaitForSeconds(1/60f);
        }
    }
}

public class VoronoiEvent
{
    public VoronoiEvent(EventType type, float height)
    {
        this.type = type;
        this.height = height;
    }

    public EventType type;
    public float height;
}

public enum EventType
{
    SiteEvent,
    EdgeIntersectionEvent
}

public class Arc : BeachlineObject
{
    public Arc()
    {

    }

    public Arc(Vector2 focus)
    {
        focusId = focus;
        arc = true;
    }

    public Vector2 limits;
    public Arc left, right;
    public Edge leftEdge, rightEdge;
    public Vector2 focusId;
}

public class Edge : BeachlineObject
{
    public Edge(Vector2 origin)
    {
        this.origin = origin;
    }
    public Vector2 origin;
    public Vector2 direction;
    public Vector2 endPoint;
}

public class BeachlineObject
{
    public bool arc;
}