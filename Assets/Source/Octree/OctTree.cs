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
    private List<Bounds> mFlattenedRegions;
    private List<T> mChildren = new List<T>();
    private List<OctNode> mFlattenedNodes = new List<OctNode>();
    private Dictionary<OctNode, List<T>> mChildrenByNodes = new Dictionary<OctNode, List<T>>();
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

    private List<OctNode> GetFlattenedNodes(OctNode startingNode)
    {
        if (startingNode == null)
        {
            Debug.Log("STarting node is null:");
            return new List<OctNode>();
        }
        mFlattenedNodes.Clear();
        mFlattenedNodes.Add(startingNode);
        int count = 0;
        int nodesToScan = mFlattenedNodes.Count;
        const int MAX_LOOP = 1000;
        while (count < nodesToScan && count < MAX_LOOP)
        {
            IOctNode node = mFlattenedNodes[count];
            if (node != null && node.IsActive)
            {
                OctNode[] children = node.GetLeafNodes();
                //int length = children.Length;
                mFlattenedNodes.AddRange(children);
                nodesToScan += children.Length;
            }
            /*
            for (int i = 0; i < length; i++)
            {
                OctNode leaf = children[i];
                if (leaf != null && leaf.IsActive)
                {
                    flattenedNodes.Add(leaf);
                    nodesToScan++;
                }
            }*/
            count++;
        }
        //flattenedNodes.TrimExcess();
        return mFlattenedNodes;
    }

    public List<Bounds> GetAllRegions()
    {
        OctNode startingNode = mRoot;
        List<OctNode> flattenedNodes = new List<OctNode>();
        flattenedNodes.Add(startingNode);
        int count = 0;
        int nodesToScan = flattenedNodes.Count;
        const int MAX_LOOP = 1000;
        List<Bounds> flattenedBounds = new List<Bounds>();
        while (count < nodesToScan && count < MAX_LOOP)
        {
            OctNode node = flattenedNodes[count];
            flattenedBounds.Add(node.BoundingBox);
            //List<OctNode> children = ((IOctNode)node).GetLeafNodes();
            //int length = children.Count;
            //for (int i = 0; i < length; i++)
            //{
            //    flattenedNodes.Add(children[i]);
            //    nodesToScan++;
            //}
            OctNode[] children = ((IOctNode)node).GetLeafNodes();
            int length = children.Length;
            //flattenedNodes.AddRange(children);
            for (int i = 0; i < length; i++)
            {
                OctNode leaf = children[i];
                if (leaf != null && leaf.IsActive)
                {
                    flattenedNodes.Add(leaf);
                    nodesToScan++;
                }
            }
            //foreach (OctNode item in children)
            //{
            //    flattenedNodes.Add(item);
            //    nodesToScan++;
            //}
            count++;
        }
        //flattenedNodes.TrimExcess();
        return flattenedBounds;
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

    private bool AddChildToNode(OctNode node, T child)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }
        if (child == null)
        {
            throw new ArgumentNullException(nameof(child));
        }
        List<T> children;
        if (!mChildrenByNodes.TryGetValue(node, out children))
        {
            children = new List<T>();
            mChildrenByNodes.Add(node, children);
        }
        if (children.Count >= OctTree<T>.MAX_CONTAINER_SIZE) return false;
        if (!children.Contains(child)) children.Add(child);
        return true;
    }

    private bool RemoveChildFromNode(OctNode node, T child)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }
        if (child == null)
        {
            throw new ArgumentNullException(nameof(child));
        }
        List<T> children;
        if (mChildrenByNodes.TryGetValue(node, out children))
        {
            return children.Remove(child);
        }
        return false;
    }

    private bool NodeContains(OctNode node, T child)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }
        if (child == null)
        {
            throw new ArgumentNullException(nameof(child));
        }
        List<T> children;
        if (mChildrenByNodes.TryGetValue(node, out children))
        {
            return children.Contains(child);
        }
        return false;
    }

    public void BuildTree(List<T> treeChildren, out List<OctNode> nodes)
    {
        //int length = treeChildren.Count;
        //nodes = new List<OctNode>(length);
        //for (int i = 0; i < length; i++)
        //{
        //    T child = treeChildren[i];
        //    OctNode node;
        //    if (((IOctNode)mRoot).InsertDynamic(child, MinSpaceSpan, out node))
        //    {
        //        //Debug.Log($"Adding Child:{child.ID}.... To Node:{node.GUID}, {i}");
        //        nodes.Add(node);
        //        AddChildToNode(node, child);
        //    }
        //    else
        //    {
        //        Debug.LogWarning($"Failed to add Child:{child.ID}.... To Node:{node}, {i}");
        //    }
        //}

        ((IOctNode)mRoot).BuildLeafNodes(treeChildren, MinSpaceSpan, mRoot.BoundingBox.size, out nodes);
    }

    public bool Insert(T gameObject, out OctNode node)
    {
        return ((IOctNode)mRoot).Insert(gameObject, out node);
    }

    public bool Update(T child, OctNode currentNode, out OctNode newNode)
    {
        newNode = null;
        if (currentNode == null) throw new ArgumentNullException(nameof(currentNode));
        if (child == null) throw new ArgumentNullException(nameof(child));
        if (!currentNode.Contains(child))
            throw new System.Exception($"child:{child} does not exist in current Node:{currentNode.GUID}");

        //if (!NodeContains(currentNode, child))
        //    throw new System.Exception($"child:{child} does not exist in current Node:{currentNode.GUID}");

        OctNode nodeToInsertIn;
        //OctTree<T>.OctNode parentToPrune = currentNode.Parent;
        //Vector3 dimension = GetRootArea().size;
        nodeToInsertIn = currentNode.Parent;
        if (nodeToInsertIn == null) nodeToInsertIn = mRoot;
        while (nodeToInsertIn != null)
        {
            if (((IOctNode)nodeToInsertIn).InsertDynamic(child, MinSpaceSpan, out newNode))
            {
                if (newNode != currentNode)
                {
                    bool s = ((IOctNode)currentNode).RemoveChild(child);
                }
                nodeToInsertIn = null;
                return true;
            }
            else
            {
                nodeToInsertIn = nodeToInsertIn.Parent;
            }
        }
        return false;
    }

    public void Prune()
    {
        mRoot.Prune();
    }

    public List<T> DebugFindNeighboringChildren(T child, OctNode currentNode, float radius, out OctNode topNode)
    {
        topNode = null;
        if (!currentNode.Contains(child)) throw new System.Exception($"child:{child} does not exist in current Node:{currentNode}");
        Bounds bounds = new Bounds(child.Position, new Vector3(radius, radius, radius));
        OctNode containingNode = GetNodeContaining(bounds, currentNode);
        topNode = containingNode;
        Debug.Log($"[FindNeighboringChildren] getting children from node:{containingNode.NAME}");
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

    public List<T> FindNeighboringChildren(T child, OctNode currentNode, float radius)
    {
        //if (!currentNode.Contains(child)) throw new System.Exception($"child:{child} does not exist in current Node:{currentNode}");
        Bounds bounds = new Bounds(child.Position, new Vector3(radius, radius, radius));
        mChildren.Clear();
        OctNode containingNode = GetNodeContaining(bounds, currentNode);
        if (containingNode == null) return mChildren;

        mFlattenedNodes.Clear();
        mFlattenedNodes.Add(containingNode);
        int count = 0;
        int nodesToScan = mFlattenedNodes.Count;
        const int MAX_LOOP = 1000;
        while (count < nodesToScan && count < MAX_LOOP)
        {
            IOctNode node = mFlattenedNodes[count];
            mChildren.AddRange(node.GetChildren());
            OctNode[] leafNodes = node.GetLeafNodes();
            int length = leafNodes.Length;
            //flattenedNodes.AddRange(leafNodes);
            //nodesToScan += leafNodes.Length;
            for (int i = 0; i < length; i++)
            {
                OctNode leaf = leafNodes[i];
                if (leaf != null && leaf.IsActive)
                {
                    mFlattenedNodes.Add(leaf);
                    nodesToScan++;
                }
            }
            count++;
        }
        return mChildren;
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
