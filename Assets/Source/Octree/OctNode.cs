using System;
using System.Collections.Generic;
using UnityEngine;
public partial class OctTree<T> where T : ITreeChild
{
    private interface IOctNode
    {
        //void BuildLeafNodes(float smallestNodeSpan);
        bool Insert(T gameObject, float smallestNodeSpan, out OctNode container);
        //bool InsertDynamic(T gameObject, float smallestNodeSpan, out OctNode container);
        void BuildLeafNodes(List<T> treeChildren, float smallestNodeSpan, out List<OctNode> nodes);
        OctNode[] GetLeafNodes();
        List<T> GetChildren();
        bool RemoveChild(T child);
        bool Prune();
        Bounds BoundingBox { get; }
        Vector3 Centre { get; }
        OctNode Parent { get; }
        bool IsEmpty { get; }
        string NAME { get; }
        string GUID { get; }
        bool IsActive { get; }
        Vector3Int Index { get; }
    }

    //[Serializable]
    public class OctNode : IOctNode
    {
        private string mGUID;
        private string mName;

        public Bounds BoundingBox { get; private set; }
        public Vector3 Centre { get; private set; }
        public OctNode Parent { get; private set; }
        public bool IsEmpty { get; private set; }
        public string NAME
        {
            get
            {
                if (string.IsNullOrWhiteSpace(mName))
                {
                    mName = (Parent == null) ? $"root->[{BoundingBox.center} X {BoundingBox.extents}]" : $"{Parent.NAME}->[{BoundingBox.center} X {BoundingBox.extents}]";
                }
                return mName;
            }
        }
        public string GUID
        {
            get
            {
                if (string.IsNullOrWhiteSpace(mGUID))
                {
                    mGUID = BoundingBox.GetHashCode().ToString();
                }
                return mGUID;
            }
        }

        Bounds IOctNode.BoundingBox => this.BoundingBox;
        Vector3 IOctNode.Centre => this.Centre;
        OctNode IOctNode.Parent => this.Parent;
        bool IOctNode.IsEmpty => this.IsEmpty;
        string IOctNode.NAME => this.NAME;
        string IOctNode.GUID => this.GUID;
        bool IOctNode.IsActive => this.IsActive;
        Vector3Int IOctNode.Index => this.Index;
        public bool IsActive { get; private set; }
        public Vector3Int Index { get; private set; }

        private int mIndex;
        private Vector3[] mVertices;
        private readonly OctNode[] mLeafNodes = new OctNode[OctTree<T>.MAX_LEAF_NODES];// new List<OctNode>(OctTree<T>.MAX_LEAF_NODES);
        private readonly List<OctNode> mDynamicLeafNodes = new List<OctNode>(OctTree<T>.MAX_LEAF_NODES);
        private readonly List<T> mChildren = new List<T>(OctTree<T>.MAX_CONTAINER_SIZE);

        /// <summary>
        /// initalize root node
        /// </summary>
        /// <param name="centre"></param>
        /// <param name="span"></param>
        internal OctNode(Vector3 centre, float span)
        {
            Bounds bounds = new Bounds(centre, new Vector3(span * 2, span * 2, span * 2));
            bounds.extents = new Vector3(Mathf.Abs(bounds.extents.x), Mathf.Abs(bounds.extents.y), Mathf.Abs(bounds.extents.z));
            Initialize(bounds, Vector3Int.RoundToInt(centre), null);
        }

        /// <summary>
        /// initalize child nodes
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="regionMin"></param>
        /// <param name="regionMax"></param>
        OctNode(OctNode parent, Vector3 regionMin, Vector3 regionMax, Vector3Int index)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (regionMin == regionMax) throw new ArgumentException($"{nameof(regionMin)} and {nameof(regionMax)} can't be equal");

            Bounds bound = Utilities.GetOctant(regionMin, regionMax);
            Initialize(bound, index, parent);
        }

        /// <summary>
        /// initialize leaf node for a given octant
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="octantRegion"></param>
        /// <param name="index"></param>
        public OctNode(OctNode parent, Bounds octantRegion, Vector3Int index)
        {
            Initialize(octantRegion, index, parent);
        }

        /// <summary>
        /// initialize node
        /// </summary>
        /// <param name="area"></param>
        /// <param name="index"></param>
        /// <param name="parent"></param>
        private void Initialize(Bounds area, Vector3Int index, OctNode parent = null)
        {
            BoundingBox = area; //because Bounds is struct and is mutable
            Centre = BoundingBox.center;
            Parent = parent;
            IsEmpty = true;
            IsActive = false;
            Index = index;
        }

        /// <summary>
        /// Check if node contains the child
        /// </summary>
        /// <param name="child"></param>
        /// <returns>true or false</returns>
        internal bool Contains(T child)
        {
            return mChildren.Contains(child);
        }

        /// <summary>
        /// returns leaf nodes. A bit unsafe because it returns original array, so do not modify the array
        /// </summary>
        /// <returns></returns>
        OctNode[] IOctNode.GetLeafNodes()
        {
            return mLeafNodes;
        }

        /// <summary>
        /// returns original children list. do not modify
        /// </summary>
        /// <returns></returns>
        List<T> IOctNode.GetChildren()
        {
            return mChildren;
        }

        /// <summary>
        /// check if node encapusaltes an area
        /// </summary>
        /// <param name="bounds"></param>
        /// <returns>true or false</returns>
        internal bool Contains(Bounds bounds)
        {
            Bounds currentNodeBounds = this.BoundingBox;
            Bounds childBound = bounds;
            if (currentNodeBounds.size.sqrMagnitude > childBound.size.sqrMagnitude)
            {
                return currentNodeBounds.ContainBounds(childBound);
            }
            return false;
        }

        /// <summary>
        /// remove child from this node
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        bool IOctNode.RemoveChild(T child)
        {
            bool s = mChildren.Remove(child);
            IsEmpty = (mChildren.Count < 1);
            //Debug.Log($"Removing child from:{NAME}. Success?{s}. Mcontainer count:({mChildren.Count})");
            return s;
        }

        /// <summary>
        /// insert all and children and Build tree
        /// </summary>
        /// <param name="treeChildren"></param>
        /// <param name="smallestNodeSpan"></param>
        /// <param name="nodes"></param>
        void IOctNode.BuildLeafNodes(List<T> treeChildren, float smallestNodeSpan, out List<OctNode> nodes)
        {
            BuildLeafNodes(treeChildren, smallestNodeSpan, out nodes);
        }

        /// <summary>
        /// insert children in the node
        /// </summary>
        /// <param name="child"></param>
        /// <param name="smallestNodeSpan"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        bool IOctNode.Insert(T child, float smallestNodeSpan, out OctNode container)
        {
            return this.Insert(child, smallestNodeSpan, out container);
        }

        /// <summary>
        /// Prune empty leaves
        /// </summary>
        /// <returns></returns>
        public bool Prune()
        {
            return Prune(this);
        }

        private bool Insert(T child, float smallestNodeSpan, out OctNode container)
        {
            container = null;
            bool success = false;
            bool insertedInLeadNode = false;
            Bounds bounds = this.BoundingBox;
            Bounds objBounds = child.GetBounds();
#if DEBUG_OCTTREE
            Debug.Log($"CHecking node {this.GUID}. Child:{child.ID}  Bound:{objBounds}");
#endif
            Vector3 smallestSize = new Vector3(smallestNodeSpan, smallestNodeSpan, smallestNodeSpan);
            if (bounds.size.sqrMagnitude <= smallestSize.sqrMagnitude)
            {
                container = this;
                if (!mChildren.Contains(child)) mChildren.Add(child);
#if DEBUG_OCTTREE
                Debug.Log($"smallest possible space found. So Adding child:{child.ID} to container:{container.GUID}, count:{mChildren.Count}, leafCount:{mDynamicLeafNodes.Count}");
#endif
                success = true;
            }
            else
            {
                if (bounds.size.sqrMagnitude > objBounds.size.sqrMagnitude)
                {
                    //Debug.Log(NAME + "--- :" + bounds + "------ to check Centre:" + objBounds + ":" + mContainer.Count);
                    if (bounds.Contains(objBounds.center) && mChildren.Count < OctTree<T>.MAX_CONTAINER_SIZE)
                    {
                        if (bounds.Intersects(objBounds))
                        {
                            if (!mChildren.Contains(child))
                            {
                                insertedInLeadNode = TryInsertChildInLeafNodes(child, smallestNodeSpan, ref bounds, ref objBounds, out container);

                                if (!insertedInLeadNode)
                                {
                                    container = this;
                                    if (!mChildren.Contains(child)) mChildren.Add(child);
#if DEBUG_OCTTREE
                                    Debug.Log($"!insertedInLeadNode Adding child:{child.ID} to this container:{container.GUID}, count:{mChildren.Count}, leafCount:{mDynamicLeafNodes.Count}");
#endif
                                }

#if DEBUG_OCTTREE
                                else
                                {

                                    Debug.Log($"Adding child:{child.ID} to:{container.GUID},\n started look-up in container:{this.GUID},\n count:{mChildren.Count}, leafCount:{mDynamicLeafNodes.Count}");
                                }
#endif
                                success = true;
                            }
                            else
                            {
                                //this child object is already in this node. so no need to look further
                                container = this;
#if DEBUG_OCTTREE
                                Debug.Log($"child:{child.ID} object is already in this:{container.GUID} node. so no need to look further. count:{mChildren.Count}, leafCount:{mDynamicLeafNodes.Count}");
#endif

                                success = true;
                            }
                        }
                        else
                        {
#if DEBUG_OCTTREE
                        Debug.LogWarning($"FAILED! in region:{GUID}  bounds don't intersect or mContainer(count:{mChildren.Count}) already has the child.");
#endif
                        }
                    }
                    else
                    {
#if DEBUG_OCTTREE
                    Debug.LogWarning($"FAILED! in region:{GUID}. Either centree is outside bounds or mContainer(count:{mChildren.Count}) is full max capacity:{ OctTree<T>.MAX_CONTAINER_SIZE}.");
#endif
                    }
                }
                else
                {
#if DEBUG_OCTTREE
                Debug.LogWarning($"FAILED! in region:{GUID}. bounds.size.sqrMagnitude > objBounds.size.sqrMagnitude:");
#endif
                }
            }
#if DEBUG_OCTTREE
            if (success && container == null)
            {
                Debug.LogWarning($"success!:{success}. but container is NULL!!, this:{this.GUID}");
            }
#endif
            //if (!success)
            //{
            //    Debug.LogWarning($"FAILED TO ADD child:{child.ID} to node this:{this.GUID}");
            //}
            IsEmpty = (mChildren.Count < 1);
            IsActive = !IsEmpty || insertedInLeadNode;
            return success;
        }

        private bool TryInsertChildInLeafNodes(T child, float smallestNodeSpan, ref Bounds bounds, ref Bounds objBounds, out OctNode container)
        {
            container = null;
            bool insertedInLeadNode = false;
            //the order of elements which GetEdgeVertices() returns is imporant.
            //because the insert leaf nodes in the exact same order / same index.

            int length = mLeafNodes.Length;
            //Bounds[] octants = new Bounds[length];
            for (int i = 0; i < length; i++)
            {
                OctNode leaf = mLeafNodes[i];
                if (leaf == null)
                {
                    if (mVertices == null) mVertices = bounds.GetEdgeVertices();
                    Bounds octantRegion = Utilities.GetOctant(this.Centre, mVertices[i]);
                    if (octantRegion.Contains(objBounds.center))
                    {
                        Vector3Int index = Vector3Int.RoundToInt(octantRegion.center);
                        leaf = new OctNode(this, octantRegion, index);
                        //mLeafNodes.Add(leaf);
                        mLeafNodes[i] = leaf;
                        insertedInLeadNode = leaf.Insert(child, smallestNodeSpan, out container);
                    }
                }
                else
                {
                    insertedInLeadNode = leaf.Insert(child, smallestNodeSpan, out container);
                }
                if (insertedInLeadNode)
                {
                    return insertedInLeadNode;
                }
            }
            return insertedInLeadNode;
        }

        private void BuildLeafNodes(List<T> treeChildren, float smallestNodeSpan, out List<OctNode> nodes)
        {
            nodes = new List<OctNode>();
            int length = treeChildren.Count;
            for (int i = 0; i < length; i++)
            {
                var child = treeChildren[i];
                OctNode node;
                if (Insert(child, smallestNodeSpan, out node))
                {
                    //Debug.Log($"Adding Child:{child.ID}.... To Node:{node.GUID}, {i}");
                    nodes.Add(node);
                }
                else
                {
                    Debug.LogWarning($"Failed to add Child:{child.ID}.... To Node:{node}, {i}");
                }
            }
        }

        private bool Prune(OctNode node)
        {
            bool isDead = node.mChildren.Count < 1 && !node.IsActive;
            if (!isDead)
            {
                var length = node.mLeafNodes.Length;
                var pruned = 0;
                for (int i = 0; i < length; i++)
                {
                    var leaf = node.mLeafNodes[i];
                    if (leaf != null && Prune(leaf))
                    {
                        leaf.IsActive = false;
                        pruned++;
                    }
                }
                isDead = (node.mChildren.Count < 1 && (pruned == length - 1));
            }
            return isDead;
        }
    }
}
