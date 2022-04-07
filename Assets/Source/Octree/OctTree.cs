using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class OctTree
{
    private const int MAX_LEAF_NODES = 8;
    private const int MIN_SPACE_SPAN = 1;
    
    public int MinSpaceSpan { get; private set; }
    private readonly OctNode mRoot;

    public OctTree(Vector3 centre, int span, int smallestSpaceSpan)
    {
        mRoot = new OctNode(centre, span);
        MinSpaceSpan = Mathf.Clamp(smallestSpaceSpan * 2, MIN_SPACE_SPAN, span); 
    }

    public Bounds GetRootArea()
    {
        return mRoot.Bounds;
    }

    public List<Bounds> GetAllRegions()
    {
        return mRoot.GetChildrenRegions();
    }

    public void BuildTree()
    {
        mRoot.BuildTree();
    }
}
