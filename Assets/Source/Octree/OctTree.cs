using System;
using System.Collections.Generic;
using UnityEngine;

public partial class OctTree<T> where T : ITreeChild
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

    List<OctNode> flattenedNodes = new List<OctNode>();

    public OctTree(Vector3 centre, int span, int smallestSpaceSpan)
    {
        mRoot = new OctNode(centre, span);
        MinSpaceSpan = Mathf.Clamp(smallestSpaceSpan * 2, MIN_SPACE_SPAN, span);
    }

    public Bounds GetRootArea()
    {
        return mRoot.BoundingBox;
    }

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
            IOctNode node = flattenedNodes[count];
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

    public void BuildTree(List<T> treeChildren, out List<OctNode> nodes)
    {
        ((IOctNode)mRoot).BuildLeafNodes(treeChildren, MinSpaceSpan, out nodes);
        mFlattenedRegions = new List<Bounds>(nodes.Count);
        foreach (var item in nodes)
        {
            mFlattenedRegions.Add(item.BoundingBox);
        }
    }

    public bool Insert(T gameObject, out OctNode node)
    {
        return ((IOctNode)mRoot).Insert(gameObject, out node);
    }

    public bool Update(T child, OctTree<T>.OctNode currentNode, out OctTree<T>.OctNode newNode)
    {
        newNode = null;
        if (!currentNode.Contains(child)) throw new System.Exception($"child:{child} does not exist in current Node:{currentNode.ID}");
        OctTree<T>.OctNode nodeToInsertIn;
        OctTree<T>.OctNode parentToPrune = currentNode.Parent;
        nodeToInsertIn = currentNode.Parent;
        if (nodeToInsertIn == null) nodeToInsertIn = mRoot;
        while (nodeToInsertIn != null)
        {
            if (((IOctNode)nodeToInsertIn).InsertDynamic(child, MinSpaceSpan, out newNode))
            {
                if (newNode != currentNode)
                {
                    bool s = ((IOctNode)currentNode).RemoveChild(child);
                    ((IOctNode)parentToPrune).Prune(currentNode);
                }
                nodeToInsertIn = null;
            }
            else
            {
                nodeToInsertIn = nodeToInsertIn.Parent;
            }
        }
        return false;
    }

    public List<T> DebugFindNeighboringChildren(T child, OctNode currentNode, float radius, out OctNode topNode)
    {
        topNode = null;
        if (!currentNode.Contains(child)) throw new System.Exception($"child:{child} does not exist in current Node:{currentNode}");
        Bounds bounds = new Bounds(child.Position, new Vector3(radius, radius, radius));
        OctNode containingNode = GetNodeContaining(bounds, currentNode);
        topNode = containingNode;
        Debug.Log($"[FindNeighboringChildren] getting children from node:{containingNode.ID}");
        List<OctNode> allLeadNodes = GetFlattenedNodes(containingNode);
        List<T> children = new List<T>(allLeadNodes.Count * OctTree<T>.MAX_CONTAINER_SIZE);
        foreach (OctNode item in allLeadNodes)
        {
            children.AddRange(((IOctNode)item).GetChildren());
            if (item == currentNode)
                children.Remove(child);
        }
        return children;
    }

    List<T> children = new List<T>();
    public List<T> FindNeighboringChildren(T child, OctNode currentNode, float radius)
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
            children.AddRange(((IOctNode)allLeadNodes[i]).GetChildren());
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

/*		unsigned int x = (unsigned int)(vCenter.x / treeSize);
		unsigned int y = (unsigned int)(vCenter.y / treeSize);
		unsigned int z = (unsigned int)(vCenter.z / treeSize);
		Node<T>* pNode = &m_node;
		//iteratively choose next child node
		for(unsigned int i = 0; i != depth; i++)
		{
			//pick node if it isn't split
			if(!pNode->IsSplit())
				break;

			unsigned int currentDepth = depth - i;
			//calculate position at current depth
			//todo: check for invalid positions
			unsigned int currentDepthX = x >> currentDepth;
            unsigned int currentDepthY = y >> currentDepth;
            unsigned int currentDepthZ = z >> currentDepth;
			//calculate childs index
            unsigned int childIndex = currentDepthX + (currentDepthZ << 1) + (currentDepthY << 2);
            unsigned int childIndex = currentDepthX + (currentDepthZ << 1) + (currentDepthY << 2);*/