using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extentions
{
    public static Vector3[] GetEdgeVerticesOfCube(Bounds bound)
    {
        Vector3 min = bound.min;
        Vector3 max = bound.max;
        Vector3 pairA1 = new Vector3(min.x, max.y, min.z);
        Vector3 pairB1 = min;
        Vector3 pairC1 = new Vector3(max.x, min.y, min.z);
        Vector3 pairD1 = new Vector3(max.x, max.y, min.z);

        Vector3 pairA2 = new Vector3(min.x, max.y, max.z);
        Vector3 pairB2 = new Vector3(min.x, min.y, max.z);
        Vector3 pairC2 = new Vector3(max.x, min.y, max.z);
        Vector3 pairD2 = max;

        return new Vector3[8] { pairA1, pairB1, pairC1, pairD1, pairA2, pairB2, pairC2, pairD2 };
    }

    public static Vector3[] GetEdgeVertices(this Bounds bound)
    {
        Vector3 min = bound.min;
        Vector3 max = bound.max;
        Vector3 pairA1 = new Vector3(min.x, max.y, min.z);
        Vector3 pairB1 = min;
        Vector3 pairC1 = new Vector3(max.x, min.y, min.z);
        Vector3 pairD1 = new Vector3(max.x, max.y, min.z);

        Vector3 pairA2 = new Vector3(min.x, max.y, max.z);
        Vector3 pairB2 = new Vector3(min.x, min.y, max.z);
        Vector3 pairC2 = new Vector3(max.x, min.y, max.z);
        Vector3 pairD2 = max;

        return new Vector3[8] { pairA1, pairB1, pairC1, pairD1, pairA2, pairB2, pairC2, pairD2 };
    }

    public static bool ContainBounds(this Bounds bounds, Bounds target)
    {
        //Debug.Log($"[ContainBounds]bounds:{bounds} contains target:{target}");
        return bounds.Contains(target.min) && bounds.Contains(target.max);
    }
}
