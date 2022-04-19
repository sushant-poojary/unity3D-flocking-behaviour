using System;
using System.Collections.Generic;
using UnityEngine;
public partial class OctTree<T>
{
    private interface IOctNode
    {
        void BuildLeafNodes(float smallestNodeSpan);
        bool Insert(ITreeChild gameObject, out OctNode container);
    }

    [Serializable]
    public class OctNode : IOctNode
    {
        public Bounds BoundingBox { get; private set; }
        public Vector3 Centre { get; private set; }
        public OctNode Parent { get; private set; }
        public bool IsEmpty { get; private set; }
        public string ID { get; private set; }

        private Vector3[] mVertices;
        private readonly List<OctNode> mLeafNodes = new List<OctNode>(OctTree<T>.MAX_LEAF_NODES);
        private readonly List<ITreeChild> mChildren = new List<ITreeChild>(OctTree<T>.MAX_CONTAINER_SIZE);
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
            ID = $"root->[{bounds.center} X {bounds.extents }]";
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
            ID = $"{parent.ID}->[{bound.center} X {bound.extents }]";
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

        internal bool Contains(ITreeChild child)
        {
            return mChildren.Contains(child);
        }

        internal List<OctNode> GetLeafNodes()
        {
            return mLeafNodes;
        }

        internal List<ITreeChild> GetChildren()
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


        internal bool RemoveChild(ITreeChild child)
        {
            bool s = mChildren.Remove(child);
            //Debug.Log($"Removing child from:{ID}. Success?{s}. Mcontainer count:({mContainer.Count})");
            return s;
        }

        private bool Insert(ITreeChild child, out OctNode container)
        {
            container = null;
            bool success = false;
            Bounds bounds = this.BoundingBox;
            Bounds objBounds = child.GetBounds();
#if DEBUG_OCTTREE
            Debug.Log($"CHecking node {this.ID}. Child Bound:{objBounds}");
#endif
            if (bounds.size.sqrMagnitude > objBounds.size.sqrMagnitude)
            {
                //Debug.Log(ID + "--- :" + bounds + "------ to check Centre:" + objBounds + ":" + mContainer.Count);
                if (bounds.Contains(objBounds.center) && mChildren.Count < OctTree<T>.MAX_CONTAINER_SIZE)
                {
                    if (bounds.Intersects(objBounds))
                    {
                        if (!InsertInLeafNodes(child, out container))
                        {
                            //if the object doesn't fit in any of the children then add it to this node's contaier
#if DEBUG_OCTTREE
                            Debug.Log($"Adding child to {ID}");
#endif
                            container = this;
                            if (!mChildren.Contains(child)) mChildren.Add(child);
                        }
                        success = true;
                    }
                    else
                    {
#if DEBUG_OCTTREE
                        Debug.LogWarning($"FAILED! in region:{ID}  bounds don't intersect or mContainer(count:{mChildren.Count}) already has the child.");
#endif
                    }
                }
                else
                {
#if DEBUG_OCTTREE
                    Debug.LogWarning($"FAILED! in region:{ID}. Either centree is outside bounds or mContainer(count:{mChildren.Count}) is full max capacity:{ OctTree<T>.MAX_CONTAINER_SIZE}.");
#endif
                }
            }
            else
            {
#if DEBUG_OCTTREE
                Debug.LogWarning($"FAILED! in region:{ID}. bounds.size.sqrMagnitude > objBounds.size.sqrMagnitude:");
#endif
            }
            IsEmpty = (mChildren.Count < 1);
            return success;
        }

        private bool InsertInLeafNodes(ITreeChild child, out OctNode container)
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
                    mLeafNodes.Add(new OctNode(this, this.Centre, edge));
                    mLeafNodes[i].BuildLeafNodes(smallestNodeSpan);
                }
            }
        }

        private void BuildLeafNodes(float smallestNodeSpan)
        {
            SubDivideRegion(smallestNodeSpan);
            mNodesUpdated = true;
        }

        void IOctNode.BuildLeafNodes(float smallestNodeSpan)
        {
            BuildLeafNodes(smallestNodeSpan);
        }

        bool IOctNode.Insert(ITreeChild gameObject, out OctNode container)
        {
            return this.Insert(gameObject, out container);
        }
    }
}
