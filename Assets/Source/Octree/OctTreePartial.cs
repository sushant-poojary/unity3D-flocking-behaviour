using System;
using System.Collections.Generic;
using UnityEngine;

public partial class OctTree
{
    private class OctNode
    {
        public Bounds BoundingBox { get; private set; }
        public Vector3 Centre { get; private set; }
        public OctNode Parent { get; private set; }
        private Vector3[] mVertices;
        private readonly OctNode[] mChildNodes;
        private List<GameObject> mContainer;
        /// <summary>
        /// initalize root node
        /// </summary>
        /// <param name="centre"></param>
        /// <param name="span"></param>
        internal OctNode(Vector3 centre, float span)
        {
            BoundingBox = new Bounds(centre, new Vector3(span * 2, span * 2, span * 2));
            Debug.Log($"[OctNode]BoundingBox:{BoundingBox}");
            Centre = BoundingBox.center;
            mChildNodes = new OctNode[OctTree.MAX_LEAF_NODES];
            Parent = null;
        }

        internal List<Bounds> GetChildRegions()
        {
            List<Bounds> regions = new List<Bounds>(mChildNodes.Length);
            foreach (OctNode item in mChildNodes)
            {
                if (item != null)
                {
                    regions.Add(item.BoundingBox);
                    regions.AddRange(item.GetChildRegions());
                }
            }
            return regions;
        }

        internal OctNode Insert(Bounds objBounds)
        {
            if (!this.BoundingBox.Intersects(objBounds))
            {
                return null;
            }

            foreach (var item in mChildNodes)
            {
                if (item != null)
                {
                    OctNode node = item.Insert(objBounds);
                    if (node != null)
                    {
                        Debug.Log($"inser node valid:{node.BoundingBox}");
                        return node;
                    }
                }
            }


            return this;
        }

        /// <summary>
        /// initalize child nodes
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="regionMin"></param>
        /// <param name="regionMax"></param>
        OctNode(OctNode parent, Vector3 regionMin, Vector3 regionMax)
        {
            Bounds bound = new Bounds();
            bound.SetMinMax(regionMin, regionMax);
            bound.extents = new Vector3(Mathf.Abs(bound.extents.x), Mathf.Abs(bound.extents.y), Mathf.Abs(bound.extents.z));
            //Debug.Log($"[OctNode][regionMin:{regionMin}, regionMax:{regionMax}],  bound:{bound}");
            BoundingBox = bound; //because Bounds is struct and is mutable
            Centre = BoundingBox.center;
            mChildNodes = new OctNode[OctTree.MAX_LEAF_NODES];
            Parent = parent;
        }


        internal List<Bounds> GetChildRegions()
        {
            List<Bounds> regions = new List<Bounds>(mChildNodes.Length);
            foreach (OctNode item in mChildNodes)
            {
                if (item != null)
                {
                    regions.Add(item.BoundingBox);
                    regions.AddRange(item.GetChildRegions());
                }
            }
            return regions;
        }

        internal OctNode Insert(Bounds objBounds)
        {
            if (!this.BoundingBox.Intersects(objBounds))
            {
                return null;
            }

            foreach (var item in mChildNodes)
            {
                if (item != null)
                {
                    OctNode node = item.Insert(objBounds);
                    if (node != null)
                    {
                        Debug.Log($"inser node valid:{node.BoundingBox}");
                        return node;
                    }
                }
            }


            return this;
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
                    mChildNodes[i] = new OctNode(this, this.Centre, edge);
                    mChildNodes[i].BuildLeafNodes(smallestNodeSpan);
                }
            }
        }

        public void BuildLeafNodes(float smallestNodeSpan)
        {
            SubDivideRegion(smallestNodeSpan);
        }
    }
}
