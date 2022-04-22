using System;
using System.Collections.Generic;
using UnityEngine;
public partial class OctTree<T> where T : ITreeChild
{
    private interface IOctNode
    {
        void BuildLeafNodes(float smallestNodeSpan);
        bool Insert(T gameObject, out OctNode container);
        bool InsertDynamic(T gameObject, float smallestNodeSpan, out OctNode container);
        void BuildLeafNodes(List<T> treeChildren, float smallestNodeSpan, Vector3 treeDimension, out List<OctNode> nodes);
        OctNode[] GetLeafNodes();
        List<T> GetChildren();
        bool RemoveChild(T child);
        bool Prune();
        bool HasLeafNodes();
        Bounds BoundingBox { get; }
        Vector3 Centre { get; }
        OctNode Parent { get; }
        bool IsEmpty { get; }
        string NAME { get; }
        string GUID { get; }
        bool IsActive { get; }
        Vector3Int Index { get; }
    }

    [Serializable]
    public class OctNode : IOctNode
    {
        public Bounds BoundingBox { get; private set; }
        public Vector3 Centre { get; private set; }
        public OctNode Parent { get; private set; }
        public bool IsEmpty { get; private set; }
        public string NAME { get; private set; }
        public string GUID { get; private set; }

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
        private bool mNodesUpdated;



        /// <summary>
        /// initalize root node
        /// </summary>
        /// <param name="centre"></param>
        /// <param name="span"></param>
        internal OctNode(Vector3 centre, float span)
        {
            Bounds bounds = new Bounds(centre, new Vector3(span * 2, span * 2, span * 2));
            bounds.extents = new Vector3(Mathf.Abs(bounds.extents.x), Mathf.Abs(bounds.extents.y), Mathf.Abs(bounds.extents.z));

            //var index = Utilities.GetIndex(centre, bounds.size);

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

            Bounds bound = GetOctant(regionMin, regionMax);
            //bound.SetMinMax(regionMin, regionMax);
            //bound.extents = new Vector3(Mathf.Abs(bound.extents.x), Mathf.Abs(bound.extents.y), Mathf.Abs(bound.extents.z));
            //NAME = $"{parent.NAME}->[{bound.center} X {bound.extents }]";
            Initialize(bound, index, parent);
        }

        public OctNode(OctNode parent, Bounds octantRegion, Vector3Int index)
        {
            Initialize(octantRegion, index, parent);
        }

        private void Initialize(Bounds area, int index, OctNode parent = null)
        {
            BoundingBox = area; //because Bounds is struct and is mutable
            Centre = BoundingBox.center;
            Parent = parent;
            mNodesUpdated = false;
            IsEmpty = true;
            IsActive = false;
            mIndex = index;
            NAME = (parent == null) ? $"root->[{area.center} X {area.extents}]" : $"{parent.NAME}->[{area.center} X {area.extents}]";
            GUID = NAME.GetHashCode().ToString();
        }

        private void Initialize(Bounds area, Vector3Int index, OctNode parent = null)
        {
            BoundingBox = area; //because Bounds is struct and is mutable
            Centre = BoundingBox.center;
            Parent = parent;
            mNodesUpdated = false;
            IsEmpty = true;
            IsActive = false;
            Index = index;
            NAME = (parent == null) ? $"root->[{area.center} X {area.extents}]" : $"{parent.NAME}->[{area.center} X {area.extents}]";
            GUID = NAME.GetHashCode().ToString();
        }

        internal bool Contains(T child)
        {
            return mChildren.Contains(child);
        }

        OctNode[] IOctNode.GetLeafNodes()
        {
            return mLeafNodes;
        }

        List<T> IOctNode.GetChildren()
        {
            return mChildren;
        }

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

        internal void MarkInactive()
        {
            IsActive = false;
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

        private Bounds GetOctant(Vector3 regionMin, Vector3 regionMax)
        {
            Bounds bound = new Bounds();
            bound.SetMinMax(regionMin, regionMax);
            bound.extents = new Vector3(Mathf.Abs(bound.extents.x), Mathf.Abs(bound.extents.y), Mathf.Abs(bound.extents.z));
            return bound;
        }


        bool IOctNode.RemoveChild(T child)
        {
            bool s = mChildren.Remove(child);
            IsEmpty = (mChildren.Count < 1);
            //Debug.Log($"Removing child from:{NAME}. Success?{s}. Mcontainer count:({mContainer.Count})");
            return s;
        }

        //        bool IOctNode.Prune(OctNode leaf)
        //        {
        //            if (leaf.IsEmpty)
        //            {
        //                //mLeafNodes[leaf.mIndex] = null;
        //                bool s = mDynamicLeafNodes.Remove(leaf);
        //                //IsEmpty = (mChildren.Count < 1);
        //#if DEBUG_OCTTREE
        //                Debug.Log($"Removing leaf:{leaf.GUID} from parent:{GUID}. Success?{s}. mDynamicLeafNodes count:({mDynamicLeafNodes.Count})");
        //#endif
        //                return s;
        //            }
        //            return false;
        //        }

        private bool Insert(T child, out OctNode container)
        {
            container = null;
            bool success = false;
            Bounds bounds = this.BoundingBox;
            Bounds objBounds = child.GetBounds();
#if DEBUG_OCTTREE
            Debug.Log($"CHecking node {this.GUID}. Child Bound:{objBounds}");
#endif
            if (bounds.size.sqrMagnitude > objBounds.size.sqrMagnitude)
            {
                //Debug.Log(NAME + "--- :" + bounds + "------ to check Centre:" + objBounds + ":" + mContainer.Count);
                if (bounds.Contains(objBounds.center) && mChildren.Count < OctTree<T>.MAX_CONTAINER_SIZE)
                {
                    if (bounds.Intersects(objBounds))
                    {
                        if (!InsertInLeafNodes(child, out container))
                        {
                            //if the object doesn't fit in any of the children then add it to this node's contaier
#if DEBUG_OCTTREE
                            Debug.Log($"Adding child to {GUID}");
#endif
                            container = this;
                            if (!mChildren.Contains(child)) mChildren.Add(child);
                        }
                        success = true;
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
            IsEmpty = (mChildren.Count < 1);
            return success;
        }

        private bool InsertInLeafNodes(T child, out OctNode container)
        {
            foreach (OctNode item in mLeafNodes)
            {
                if (item != null)
                {
                    if (item.Insert(child, out container))
                    {
                        return true;
                    }
                }
            }
            container = null;
            return false;
        }

        private bool InsertDynamic(T child, float smallestNodeSpan, out OctNode container)
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
                                //the order of elements which GetEdgeVertices() returns is imporant.
                                //because the insert leaf nodes in the exact same order / same index.
                                Vector3[] edges = null;
                                int length = mLeafNodes.Length;
                                //Bounds[] octants = new Bounds[length];
                                for (int i = 0; i < length; i++)
                                {
                                    if (!insertedInLeadNode)
                                    {
                                        OctNode leaf = mLeafNodes[i];
                                        if (leaf == null)
                                        {
                                            if (edges == null) edges = bounds.GetEdgeVertices();
                                            Bounds octantRegion = GetOctant(this.Centre, edges[i]);
                                            if (octantRegion.Contains(objBounds.center))
                                            {
                                                Vector3Int index = Vector3Int.RoundToInt(octantRegion.center);
                                                leaf = new OctNode(this, octantRegion, index);
                                                //mLeafNodes.Add(leaf);
                                                mLeafNodes[i] = leaf;
                                                insertedInLeadNode = leaf.InsertDynamic(child, smallestNodeSpan, out container);
                                            }
                                        }
                                        else
                                        {
                                            insertedInLeadNode = leaf.InsertDynamic(child, smallestNodeSpan, out container);
                                        }
                                    }
                                }

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
            if (!success)
            {
                Debug.LogWarning($"FAILED TO ADD child:{child.ID} to node this:{this.GUID}");
            }
            IsEmpty = (mChildren.Count < 1);
            IsActive = !IsEmpty || insertedInLeadNode;
            return success;
        }

        //private Vector3[] GetEdgeVerticesOfCube(Bounds bound)
        //{
        //    if (mVertices == null)
        //    {
        //        Vector3 min = bound.min;
        //        Vector3 max = bound.max;
        //        Vector3 pairA1 = new Vector3(min.x, max.y, min.z);
        //        Vector3 pairB1 = min;
        //        Vector3 pairC1 = new Vector3(max.x, min.y, min.z);
        //        Vector3 pairD1 = new Vector3(max.x, max.y, min.z);

        //        Vector3 pairA2 = new Vector3(min.x, max.y, max.z);
        //        Vector3 pairB2 = new Vector3(min.x, min.y, max.z);
        //        Vector3 pairC2 = new Vector3(max.x, min.y, max.z);
        //        Vector3 pairD2 = max;

        //        mVertices = new Vector3[8] { pairA1, pairB1, pairC1, pairD1, pairA2, pairB2, pairC2, pairD2 };
        //    }
        //    return mVertices;
        //}

        private void SubDivideRegion(float smallestNodeSpan)
        {
            Vector3 smallestSize = new Vector3(smallestNodeSpan, smallestNodeSpan, smallestNodeSpan);

            //Debug.Log($"Bounds.size:{ Bounds.size}, sqg mag:{Bounds.size.sqrMagnitude}, smallestSzie:{smallestSize}  sqrMag:{smallestSize.sqrMagnitude}");
            if (BoundingBox.size.sqrMagnitude <= smallestSize.sqrMagnitude) return;

            Vector3[] vertices = BoundingBox.GetEdgeVertices(); //GetEdgeVerticesOfCube(BoundingBox);
            for (int i = 0; i < OctTree<T>.MAX_LEAF_NODES; i++)
            {
                if (i < vertices.Length)
                {
                    Vector3 edge = vertices[i];
                    //Debug.Log($"index:{i}, edge:{ edge}, Bounds.center:{Bounds}");
                    mLeafNodes[i] = new OctNode(this, this.Centre, edge, new Vector3Int(i, i, i));
                    mLeafNodes[i].BuildLeafNodes(smallestNodeSpan);
                    mDynamicLeafNodes.Add(mLeafNodes[i]);
                }
            }
        }

        private void BuildLeafNodes(float smallestNodeSpan)
        {
            SubDivideRegion(smallestNodeSpan);
            mNodesUpdated = true;
        }

        private void BuildLeafNodes(List<T> treeChildren, float smallestNodeSpan, Vector3 treeDimension, out List<OctNode> nodes)
        {
            nodes = new List<OctNode>();
            int length = treeChildren.Count;
            for (int i = 0; i < length; i++)
            {
                var child = treeChildren[i];
                OctNode node;
                if (InsertDynamic(child, smallestNodeSpan, out node))
                {
                    //Debug.Log($"Adding Node:{node}, {i}");
                    nodes.Add(node);
                }
            }
            mNodesUpdated = true;
        }

        void IOctNode.BuildLeafNodes(float smallestNodeSpan)
        {
            BuildLeafNodes(smallestNodeSpan);
        }

        void IOctNode.BuildLeafNodes(List<T> treeChildren, float smallestNodeSpan, Vector3 treeDimension, out List<OctNode> nodes)
        {
            BuildLeafNodes(treeChildren, smallestNodeSpan, treeDimension, out nodes);
        }

        bool IOctNode.Insert(T gameObject, out OctNode container)
        {
            return this.Insert(gameObject, out container);
        }

        bool IOctNode.InsertDynamic(T child, float smallestNodeSpan, out OctNode container)
        {
            return this.InsertDynamic(child, smallestNodeSpan, out container);
        }

        public bool HasLeafNodes()
        {
            throw new NotImplementedException();
        }

        public bool Prune()
        {
            return Prune(this);
        }

        private bool Prune(OctNode node)
        {
            bool isDead = node.mChildren.Count < 1 && node.IsActive;
            if (!isDead)
            {
                var length = node.mLeafNodes.Length;
                var pruned = 0;
                for (int i = 0; i < length; i++)
                {
                    var leaf = node.mLeafNodes[i];
                    if (leaf != null && Prune(leaf))
                    {
                        leaf.MarkInactive();
                        pruned++;
                    }
                }
                isDead = (node.mChildren.Count < 1 && (pruned == length - 1));
            }
            return isDead;
        }
    }
}
