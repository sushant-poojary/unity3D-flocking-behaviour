﻿using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OctNode
{
    public Bounds BoundingBox { get; private set; }
    public Vector3 Centre { get; private set; }
    public OctNode Parent { get; private set; }
    public bool IsEmpty { get; private set; }

    private Vector3[] mVertices;
    private readonly List<OctNode> mChildNodes = new List<OctNode>(OctTree.MAX_LEAF_NODES);
    private List<GameObject> mContainer = new List<GameObject>(OctTree.MAX_CONTAINER_SIZE);
    private List<OctNode> mAllChildNodes;
    private List<Bounds> mAllChildAreas;
    private bool mNodesUpdated;

    internal IEnumerable<OctNode> GetChildren()
    {
        return mChildNodes;
    }

    /// <summary>
    /// initalize root node
    /// </summary>
    /// <param name="centre"></param>
    /// <param name="span"></param>
    internal OctNode(Vector3 centre, float span)
    {
        Bounds bounds = new Bounds(centre, new Vector3(span * 2, span * 2, span * 2));
        bounds.extents = new Vector3(Mathf.Abs(bounds.extents.x), Mathf.Abs(bounds.extents.y), Mathf.Abs(bounds.extents.z));
        Initialize(bounds, null);
    }

    /// <summary>
    /// initalize child nodes
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="regionMin"></param>
    /// <param name="regionMax"></param>
    OctNode(OctNode parent, Vector3 regionMin, Vector3 regionMax)
    {
        if (parent == null) throw new ArgumentNullException(nameof(parent));
        if (regionMin == regionMax) throw new ArgumentException($"{nameof(regionMin)} and {nameof(regionMax)} can't be equal");

        Bounds bound = new Bounds();
        bound.SetMinMax(regionMin, regionMax);
        bound.extents = new Vector3(Mathf.Abs(bound.extents.x), Mathf.Abs(bound.extents.y), Mathf.Abs(bound.extents.z));
        Initialize(bound, parent);
    }

    private void Initialize(Bounds area, OctNode parent = null)
    {
        BoundingBox = area; //because Bounds is struct and is mutable
        Centre = BoundingBox.center;
        Parent = parent;
        mNodesUpdated = false;
        IsEmpty = true;
    }


    //internal List<Bounds> GetChildRegions()
    //{
    //    if (mAllChildAreas == null || mNodesUpdated)
    //    {
    //        mAllChildAreas = new List<Bounds>(OctTree.MAX_LEAF_NODES);
    //        foreach (OctNode item in mChildNodes)
    //        {
    //            if (item != null)
    //            {
    //                mAllChildAreas.Add(item.BoundingBox);
    //                mAllChildAreas.AddRange(item.GetChildRegions());
    //            }
    //        }
    //        mAllChildAreas.TrimExcess();
    //    }
    //    return mAllChildAreas;
    //}

    //private List<OctNode> FlattenBranches()
    //{
    //    if (mAllChildNodes == null)
    //    {
    //        mAllChildNodes = new List<OctNode>(OctTree.MAX_LEAF_NODES);
    //        foreach (OctNode item in mChildNodes)
    //        {
    //            if (item != null)
    //            {
    //                mAllChildNodes.AddRange(item.FlattenBranches());
    //            }
    //        }
    //    }
    //    return mAllChildNodes;
    //}


    //internal List<OctNode> GetNonEmptyChildRegions()
    //{
    //    if (mAllChildAreas == null) return null;
    //    foreach (var item in mAllChildAreas)
    //    {
    //        item.
    //    }
    //}


    internal bool Insert(GameObject gameObject, Bounds objBounds, out OctNode container)
    {
        container = null;
        bool success = false;
        Bounds bounds = this.BoundingBox;
        if (bounds.size.sqrMagnitude > objBounds.size.sqrMagnitude)
        {
            if (bounds.Contains(objBounds.center))
            {
                if (this.BoundingBox.Intersects(objBounds))
                {
                    if (!InsertInChildNodes(gameObject, objBounds, out container))
                    {
                        //if the object doesn't fit in any of the children then add it to this node's contaier
                        container = this;
                        mContainer.Add(gameObject);
                    }
                    success = true;
                }
            }
        }
        IsEmpty = (mContainer.Count < 1);
        return success;
    }

    private bool InsertInChildNodes(GameObject gameObject, Bounds objBounds, out OctNode container)
    {
        foreach (OctNode item in mChildNodes)
        {
            if (item != null)
            {
                if (item.Insert(gameObject, objBounds, out container))
                {
                    return true;
                }
            }
        }
        container = null;
        return false;
    }

    private Vector3[] GetEdgeVerticesOfCube(Bounds bound)
    {
        if (mVertices == null)
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

            mVertices = new Vector3[8] { pairA1, pairB1, pairC1, pairD1, pairA2, pairB2, pairC2, pairD2 };
        }
        return mVertices;
    }

    private void SubDivideRegion(float smallestNodeSpan)
    {
        Vector3 smallestSize = new Vector3(smallestNodeSpan, smallestNodeSpan, smallestNodeSpan);

        //Debug.Log($"Bounds.size:{ Bounds.size}, sqg mag:{Bounds.size.sqrMagnitude}, smallestSzie:{smallestSize}  sqrMag:{smallestSize.sqrMagnitude}");
        if (BoundingBox.size.sqrMagnitude <= smallestSize.sqrMagnitude) return;

        Vector3[] vertices = GetEdgeVerticesOfCube(BoundingBox);
        for (int i = 0; i < OctTree.MAX_LEAF_NODES; i++)
        {
            if (i < vertices.Length)
            {
                Vector3 edge = vertices[i];
                //Debug.Log($"index:{i}, edge:{ edge}, Bounds.center:{Bounds}");
                mChildNodes.Add(new OctNode(this, this.Centre, edge));
                mChildNodes[i].BuildLeafNodes(smallestNodeSpan);
            }
        }
    }

    public void BuildLeafNodes(float smallestNodeSpan)
    {
        SubDivideRegion(smallestNodeSpan);
        mNodesUpdated = true;
    }
}
