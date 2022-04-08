using System.Collections.Generic;
using UnityEngine;

public partial class OctTree
{
    private const int MAX_LEAF_NODES = 8;
    private const int MIN_SPACE_SPAN = 1;
    private const int MAX_CONTAINER_SIZE = 100;

    public int MinSpaceSpan { get; private set; }
    private readonly OctNode mRoot;

    public OctTree(Vector3 centre, int span, int smallestSpaceSpan)
    {
        mRoot = new OctNode(centre, span);
        MinSpaceSpan = Mathf.Clamp(smallestSpaceSpan * 2, MIN_SPACE_SPAN, span);
    }

    public Bounds GetRootArea()
    {
        return mRoot.BoundingBox;
    }

    public List<Bounds> GetAllRegions()
    {
        return mRoot.GetChildRegions();
    }

    public void BuildTree()
    {
        mRoot.BuildLeafNodes(MinSpaceSpan);
    }

    public bool Insert(GameObject gameObject, out Bounds bounds)
    {
        bounds = new Bounds();
        Bounds objBounds = gameObject.GetComponent<BoxCollider>().bounds;
        OctNode containingNode;
        if (mRoot.Insert(gameObject, objBounds, out containingNode))
        {
            bounds = containingNode.BoundingBox;
            return true;
        }
        return false;
    }
}
