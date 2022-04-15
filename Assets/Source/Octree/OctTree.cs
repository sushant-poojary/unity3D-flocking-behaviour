using System.Collections.Generic;
using UnityEngine;

public partial class OctTree<T>
{
    public const int MAX_LEAF_NODES = 8;
    public const int MIN_SPACE_SPAN = 1;
    public const int MAX_CONTAINER_SIZE = 50;

    public int MinSpaceSpan { get; private set; }
    private readonly OctNode mRoot;
    private Dictionary<T, Bounds> mBoundsByGameobject;
    public List<OctNode> mChildNodes;
    private List<OctNode> mFlattenedNodes;
    private List<Bounds> mFlattenedRegions;
    private List<OctNode> mFlattenedNonEmptyRegions = new List<OctNode>();

    public OctTree(Vector3 centre, int span, int smallestSpaceSpan)
    {
        mRoot = new OctNode(centre, span);
        MinSpaceSpan = Mathf.Clamp(smallestSpaceSpan * 2, MIN_SPACE_SPAN, span);
    }

    public Bounds GetRootArea()
    {
        return mRoot.BoundingBox;
    }

    private void BuildFlattenedRegions()
    {
        if (mFlattenedNodes == null)
        {
            mFlattenedNodes = new List<OctNode>(MAX_LEAF_NODES);
            mFlattenedRegions = new List<Bounds>(MAX_LEAF_NODES);
            mFlattenedNodes.Add(mRoot);
            int count = 0;
            int nodesToScan = mFlattenedNodes.Count;
            const int MAX_LOOP = 1000;
            while (count < nodesToScan && count < MAX_LOOP)
            {
                OctNode node = mFlattenedNodes[count];
                mFlattenedRegions.Add(node.BoundingBox);
                IEnumerable<OctNode> children = node.GetChildren();
                foreach (OctNode item in children)
                {
                    mFlattenedNodes.Add(item);
                    nodesToScan++;
                }
                count++;
            }
        }
    }

    public List<Bounds> GetAllRegions()
    {
        return mFlattenedRegions;
    }

    public List<OctNode> GetNonEmptyRegions()
    {
        mFlattenedNonEmptyRegions.Clear();
        foreach (var item in mFlattenedNodes)
        {
            if (!item.IsEmpty)
            {
                mFlattenedNonEmptyRegions.Add(item);
            }
        }
        return mFlattenedNonEmptyRegions;
    }

    public void BuildTree()
    {
        ((IOctNode)mRoot).BuildLeafNodes(MinSpaceSpan);
        BuildFlattenedRegions();
    }

    public bool Insert(ITreeChild gameObject, out OctNode node)
    {
        return ((IOctNode)mRoot).Insert(gameObject, out node);
    }

    public bool Update(ITreeChild child, OctTree<T>.OctNode currentNode, out OctTree<T>.OctNode newNode)
    {
        OctTree<T>.OctNode nodeToInsertIn;
        nodeToInsertIn = currentNode.Parent;
        if (nodeToInsertIn == null) nodeToInsertIn = mRoot;
        //if (currentNode.Parent == mRoot || currentNode.Parent == null)
        //{
        //    nodeToInsertIn = mRoot;
        //}
        //else
        //{
        //    nodeToInsertIn = currentNode.Parent.Parent;
        //}

        if (((IOctNode)nodeToInsertIn).Insert(child, out newNode))
        {
            if (newNode != currentNode)
                return currentNode.RemoveChild(child);
            //{
            //    throw new System.Exception("Failed to remove "+child+" from node:"+currentNode);
            //}
        }
        return false;

        //bounds = new Bounds();
        //Bounds objBounds = gameObject.GetComponent<BoxCollider>().bounds;
        //OctNode containingNode;
        //if (mRoot.Insert(gameObject, objBounds, out containingNode))
        //{
        //    bounds = containingNode.BoundingBox;
        //    return true;
        //}
        //return false;
    }
}
