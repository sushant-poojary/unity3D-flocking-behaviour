using System;
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
    //private List<OctNode> mFlattenedNodes;
    private List<Bounds> mFlattenedRegions;
    //private List<OctNode> mFlattenedNonEmptyRegions = new List<OctNode>();

    public OctTree(Vector3 centre, int span, int smallestSpaceSpan)
    {
        mRoot = new OctNode(centre, span);
        MinSpaceSpan = Mathf.Clamp(smallestSpaceSpan * 2, MIN_SPACE_SPAN, span);
    }

    public Bounds GetRootArea()
    {
        return mRoot.BoundingBox;
    }
    List<OctNode> flattenedNodes = new List<OctNode>();
    private List<OctNode> GetFlattenedNodes(OctNode startingNode)
    {
        if (startingNode == null)
        {
            Debug.Log("STarting node is null:");
            return new List<OctNode>();
        }
        flattenedNodes.Clear();
        flattenedNodes.Add(startingNode);
        int count = 0;
        int nodesToScan = flattenedNodes.Count;
        const int MAX_LOOP = 1000;
        while (count < nodesToScan && count < MAX_LOOP)
        {
            OctNode node = flattenedNodes[count];
            List<OctNode> children = node.GetLeafNodes();
            //int length = children.Count;
            flattenedNodes.AddRange(children);
            nodesToScan += children.Count;
            //for (int i = 0; i < length; i++)
            //{
            //    flattenedNodes.Add(children[i]);
            //    nodesToScan++;
            //}
            //foreach (OctNode item in children)
            //{
            //    flattenedNodes.Add(item);
            //    nodesToScan++;
            //}
            count++;
        }
        //flattenedNodes.TrimExcess();
        return flattenedNodes;
    }

    public List<Bounds> GetAllRegions()
    {
        return mFlattenedRegions;
    }

    //public List<OctNode> GetNonEmptyRegions()
    //{
    //    mFlattenedNonEmptyRegions.Clear();
    //    foreach (var item in mFlattenedNodes)
    //    {
    //        if (!item.IsEmpty)
    //        {
    //            mFlattenedNonEmptyRegions.Add(item);
    //        }
    //    }
    //    return mFlattenedNonEmptyRegions;
    //}

    public void BuildTree()
    {
        ((IOctNode)mRoot).BuildLeafNodes(MinSpaceSpan);
        var flattenedNodes = GetFlattenedNodes(mRoot);
        mFlattenedRegions = new List<Bounds>(flattenedNodes.Count);
        foreach (var item in flattenedNodes)
        {
            mFlattenedRegions.Add(item.BoundingBox);
        }
    }

    public bool Insert(ITreeChild gameObject, out OctNode node)
    {
        return ((IOctNode)mRoot).Insert(gameObject, out node);
    }

    public bool Update(ITreeChild child, OctTree<T>.OctNode currentNode, out OctTree<T>.OctNode newNode)
    {
        newNode = null;
        if (!currentNode.Contains(child)) throw new System.Exception($"child:{child} does not exist in current Node:{currentNode}");
        OctTree<T>.OctNode nodeToInsertIn;
        nodeToInsertIn = currentNode.Parent;
        if (nodeToInsertIn == null) nodeToInsertIn = mRoot;
        while (nodeToInsertIn != null)
        {
            if (((IOctNode)nodeToInsertIn).Insert(child, out newNode))
            {
                if (newNode != currentNode)
                    return currentNode.RemoveChild(child);
                nodeToInsertIn = null;
            }
            else
            {
                nodeToInsertIn = nodeToInsertIn.Parent;
            }
        }
        return false;
    }

    public IEnumerable<ITreeChild> DebugFindNeighboringChildren(ITreeChild child, OctNode currentNode, float radius, out OctNode topNode)
    {
        topNode = null;
        if (!currentNode.Contains(child)) throw new System.Exception($"child:{child} does not exist in current Node:{currentNode}");
        Bounds bounds = new Bounds(child.Position, new Vector3(radius, radius, radius));
        OctNode containingNode = GetNodeContaining(bounds, currentNode);
        topNode = containingNode;
        Debug.Log($"[FindNeighboringChildren] getting children from node:{containingNode.ID}");
        List<OctNode> allLeadNodes = GetFlattenedNodes(containingNode);
        List<ITreeChild> children = new List<ITreeChild>(allLeadNodes.Count * OctTree<T>.MAX_CONTAINER_SIZE);
        foreach (OctNode item in allLeadNodes)
        {
            children.AddRange(item.GetChildren());
            if (item == currentNode)
                children.Remove(child);
        }
        return children;
    }

    List<ITreeChild> children = new List<ITreeChild>();
    public List<ITreeChild> FindNeighboringChildren(ITreeChild child, OctNode currentNode, float radius)
    {
        //if (!currentNode.Contains(child)) throw new System.Exception($"child:{child} does not exist in current Node:{currentNode}");
        Bounds bounds = new Bounds(child.Position, new Vector3(radius, radius, radius));
        children.Clear();
        OctNode containingNode = GetNodeContaining(bounds, currentNode);
        if (containingNode == null) return children;
        //Debug.Log($"[FindNeighboringChildren] getting children from node:{containingNode.ID}");
        List<OctNode> allLeadNodes = GetFlattenedNodes(containingNode);
        //List<ITreeChild> children = new List<ITreeChild>(allLeadNodes.Count);
        int length = allLeadNodes.Count;
        for (int i = 0; i < length; i++)
        {
            children.AddRange(allLeadNodes[i].GetChildren());
        }
        //foreach (OctNode item in allLeadNodes)
        //{
        //    children.AddRange(item.GetChildren());
        //    //if (item == currentNode)
        //    //    children.Remove(child);
        //}
        return children;
    }

    private OctNode GetNodeContaining(Bounds bounds, OctNode currentNode)
    {
        //return currentNode;
        while (currentNode != null)
        {
            if (currentNode.Contains(bounds))
            {
                return currentNode;
            }
            else
            {
                currentNode = currentNode.Parent;
            }
        }
        return null;
    }
}
