using System;
using System.Collections.Generic;
using UnityEngine;

public partial class OctTree
{
    private class OctNode
    {
        public Vector3 Centre { get; private set; }
        public Bounds Bounds { get; private set; }
        public OctNode Parent { get; private set; }
        private readonly OctNode[] mOctNodes;

        /// <summary>
        /// initalize root node
        /// </summary>
        /// <param name="centre"></param>
        /// <param name="span"></param>
        internal OctNode(Vector3 centre, float span) : this(null, centre, span)
        {
            Parent = null;
        }

        internal List<Bounds> GetChildrenRegions()
        {
            List<Bounds> regions = new List<Bounds>(mOctNodes.Length);

            foreach (var item in mOctNodes)
            {
                if (item != null)
                {
                    regions.Add(item.Bounds);
                    regions.AddRange(item.GetChildrenRegions());
                }
            }
            return regions;
        }

        /// <summary>
        /// initialize child node
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="centre"></param>
        /// <param name="span"></param>
        OctNode(OctNode parent, Vector3 centre, float span)
        {
            Centre = centre;
            Bounds = new Bounds(centre, new Vector3(span * 2, span * 2, span * 2));
            mOctNodes = new OctNode[OctTree.MAX_LEAF_NODES];
            Parent = parent;
        }

        OctNode(OctNode parent, Vector3 regionMin, Vector3 regionMax)
        {
            Debug.Log("regionMin.:" + regionMin + "regionMax:" + regionMax);
            Bounds = new Bounds();
            Bounds.SetMinMax(regionMax, regionMin);
            Centre = Bounds.center;
            Debug.Log("Bounds:" + Bounds);
            mOctNodes = new OctNode[OctTree.MAX_LEAF_NODES];
            Parent = parent;
        }

        private Vector3[] GetEdgePointsOfCube(Bounds bound)
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

            Vector3[] edgePoints = new Vector3[8] { pairA1, pairB1, pairC1, pairD1, pairA2, pairB2, pairC2, pairD2 };

            return edgePoints;
        }

        private void SubDivideRegion()
        {
            //Vector3 min = Bounds.min;
            //Vector3 max = Bounds.max;
            //Vector3 pairA1 = new Vector3(min.x, max.y, min.z);
            //Vector3 pairB1 = min;
            //Vector3 pairC1 = new Vector3(max.x, min.y, min.z);
            //Vector3 pairD1 = new Vector3(max.x, max.y, min.z);

            //Vector3 pairA2 = new Vector3(min.x, max.y, max.z);
            //Vector3 pairB2 = new Vector3(min.x, min.y, max.z);
            //Vector3 pairC2 = new Vector3(max.x, min.y, max.z);
            //Vector3 pairD2 = max;


            if (Bounds.extents.sqrMagnitude <= OctTree.MIN_SPACE_SPAN * OctTree.MIN_SPACE_SPAN) return;

            Vector3[] edgePoints = GetEdgePointsOfCube(Bounds);
            //Vector3[] face1Points = new Vector3[4]
            //{
            //    new Vector3(min.x, max.y, min.z),
            //    min,
            //    new Vector3(max.x, min.y, min.z),
            //    new Vector3(max.x, max.y, min.z)
            //};
            //Vector3[] face2Points = new Vector3[4]
            //{
            //    new Vector3(min.x, max.y, max.z),
            //    new Vector3(min.x, min.y, max.z),
            //    new Vector3(max.x, min.y, max.z),
            //    max
            //};
            //float span = Bounds.extents.x / 2;
            int len = OctTree.MAX_LEAF_NODES;
            for (int i = 0; i < len; i++)
            {
                if (i < edgePoints.Length)
                {
                    Vector3 edge = edgePoints[i];
                    Debug.Log($"edge:{ edge}, Bounds.center:{Bounds}");
                    mOctNodes[i] = new OctNode(this, Bounds.center, edge);
                    //mOctNodes[i].BuildTree();
                }
            }
            //Debug.Log("[OctNode][SubDivideRegion]:" + Bounds.size / 8);
        }

        public void BuildTree()
        {
            SubDivideRegion();
        }
    }
}
