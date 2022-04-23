using UnityEngine;

public static class Utilities
{
    public static Vector3Int GetIndex(Vector3 position, Vector3 dimension)
    {
        Debug.Log($"{position}, dim:{dimension}");
        Vector3 pos = position;
        //Vector3 dimension = GetRootArea().size;
        Vector3Int index = Vector3Int.zero;
        index.x = Mathf.FloorToInt(pos.x / dimension.x);
        index.y = Mathf.FloorToInt(pos.y / dimension.y);
        index.z = Mathf.FloorToInt(pos.z / dimension.z);

        return index;
    }

    public static Vector3Int GetIndex(Vector3 position, Bounds dimension)
    {
        Debug.Log($"{position}, dim:{dimension}");
        Vector3 pos = (position - dimension.min);
        //Vector3 dimension = GetRootArea().size;
        Vector3Int index = Vector3Int.zero;
        index.x = Mathf.FloorToInt(pos.x / 4);
        index.y = Mathf.FloorToInt(pos.y / 4);
        index.z = Mathf.FloorToInt(pos.z / 4);

        return index;
    }

    /// <summary>
    /// intializes bounds with min-max.
    /// <br>sets the extents to positive value. Becaause otherwise Bounds.Contains and checks like that will fail</br>    /// 
    /// </summary>
    /// <param name="regionMin"></param>
    /// <param name="regionMax"></param>
    /// <returns></returns>
    public static Bounds GetOctant(Vector3 regionMin, Vector3 regionMax)
    {
        Bounds bound = new Bounds();
        bound.SetMinMax(regionMin, regionMax);
        bound.extents = new Vector3(Mathf.Abs(bound.extents.x), Mathf.Abs(bound.extents.y), Mathf.Abs(bound.extents.z));
        return bound;
    }
}
