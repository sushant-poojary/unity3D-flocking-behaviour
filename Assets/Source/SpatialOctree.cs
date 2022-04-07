using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class SpatialOctree : MonoBehaviour
{
    public Transform Boid;
    OctTree sdom;
    Bounds sDmonBounds;
    List<Bounds> allRegions;
    List<GameObject>[,,] spatialBoxes = new List<GameObject>[0, 0, 0];

    bool flag = false;
    // Start is called before the first frame update
    void Start()
    {
        sdom = new OctTree(transform.position, 50, 10);

        sDmonBounds = sdom.GetRootArea();
        //int cellX = Mathf.RoundToInt(sDmonBounds.size.x / 10f);
        //int cellY = Mathf.RoundToInt(sDmonBounds.size.y / 10f);
        //int cellZ = Mathf.RoundToInt(sDmonBounds.size.z / 10f);
        //spatialBoxes = new List<GameObject>[0, 0, 0];



    }

    private void LateUpdate()
    {
        if (!flag)
        {
            sdom.BuildTree();
            allRegions = sdom.GetAllRegions();
            flag = true;
        }
    }

    // Update is called once per frame
    void Update()
    {

        //var diff = Boid.position - sDmonBounds.min;
        //var indexPos = Vector3Int.FloorToInt(diff / 10);

        //Debug.Log(indexPos);

        /*     boundPoint3 = Vector3(boundPoint1.x, boundPoint1.y, boundPoint2.z);
     boundPoint4 = Vector3(boundPoint1.x, boundPoint2.y, boundPoint1.z);
     boundPoint5 = Vector3(boundPoint2.x, boundPoint1.y, boundPoint1.z);
     boundPoint6 = Vector3(boundPoint1.x, boundPoint2.y, boundPoint2.z);
     boundPoint7 = Vector3(boundPoint2.x, boundPoint1.y, boundPoint2.z);
     boundPoint8 = Vector3(boundPoint2.x, boundPoint2.y, boundPoint1.z);*/
    }

    private void OnDrawGizmos()
    {
        Color originalColor = Gizmos.color;
        //if (allRegions != null)
        //{
        //    foreach (var item in allRegions)
        //    {
        //        Gizmos.DrawSphere(item.center, 0.5f);
        //        Gizmos.DrawWireCube(item.center, sDmonBounds.size);
        //    }
        //}

        Vector3 min = sDmonBounds.min;
        Vector3 max = sDmonBounds.max;
        Vector3 pairA1 = new Vector3(min.x, max.y, min.z);
        Vector3 pairB1 = min;
        Vector3 pairC1 = new Vector3(max.x, min.y, min.z);
        Vector3 pairD1 = new Vector3(max.x, max.y, min.z);

        Vector3 pairA2 = new Vector3(min.x, max.y, max.z);
        Vector3 pairB2 = new Vector3(min.x, min.y, max.z);
        Vector3 pairC2 = new Vector3(max.x, min.y, max.z);
        Vector3 pairD2 = max;

        Vector3[] edgePoints = new Vector3[8] { pairA1, pairB1, pairC1, pairD1, pairA2, pairB2, pairC2, pairD2 };

        foreach (var item in edgePoints)
        {
            Bounds box3 = new Bounds();
            box3.SetMinMax(sDmonBounds.center, item);

            //Debug.Log(item + ",:"+sDmonBounds.center+", box3:"+box3);
            Gizmos.DrawWireCube(box3.center, box3.size);
            Gizmos.DrawSphere(box3.center, 1);
        }

        /*
        Gizmos.color = Color.red;
        Bounds box1 = new Bounds();
        box1.SetMinMax(pairA1, sDmonBounds.center);
        Gizmos.DrawWireCube(box1.center, box1.size);

        Gizmos.color = Color.blue;
        Bounds box2 = new Bounds();
        box2.SetMinMax(pairB1, sDmonBounds.center);
        Gizmos.DrawWireCube(box2.center, box2.size);

        Gizmos.color = Color.yellow;
        Bounds box3 = new Bounds();
        box3.SetMinMax(pairC1, sDmonBounds.center);
        Gizmos.DrawWireCube(box3.center, box3.size);

        Gizmos.color = Color.black;
        Bounds box4 = new Bounds();
        box4.SetMinMax(pairD1, sDmonBounds.center);
        Gizmos.DrawWireCube(box4.center, box4.size);
        */
        /*
        Vector3[] face1Points = new Vector3[4]
                    {
                new Vector3(min.x, max.y, min.z),
                min,
                new Vector3(max.x, min.y, min.z),
                new Vector3(max.x, max.y, min.z)
                    };
        Vector3[] face2Points = new Vector3[4]
        {
                new Vector3(min.x, max.y, max.z),
                new Vector3(min.x, min.y, max.z),
                new Vector3(max.x, min.y, max.z),
                max
        };

        Vector3[] subface1Points = new Vector3[4];
        Vector3[] subface2Points = new Vector3[4];
        Vector3[] subface3Points = new Vector3[4];

        //float span = Bounds.extents.x / 2;
        int len = face1Points.Length;
        for (int i = 0; i < len; i++)
        {
            int nextIndex = i + 1;
            if (nextIndex >= len) nextIndex = 0;
            Vector3 face1P1 = face1Points[i];
            Vector3 face1P2 = face1Points[nextIndex];
            Gizmos.DrawLine(face1P1, face1P2);

            Vector3 diff = face1P2 - face1P1;
            Vector3 p1 = face1P1 + (diff / 2);

            subface1Points[i] = p1;
            Gizmos.DrawSphere(p1, 1);

            Vector3 face2P1 = face2Points[i];
            Vector3 face2P2 = face2Points[nextIndex];
            Gizmos.DrawLine(face2P1, face2P2);


            diff = face2P2 - face2P1;
            p1 = face2P1 + (diff / 2);
            subface2Points[i] = p1;
            Gizmos.DrawSphere(p1, 1);

            //CONNECTING TWO FACES
            Gizmos.DrawLine(face1P1, face2P1);
            diff = face1P1 - face2P1;
            p1 = face2P1 + (diff / 2);
            subface3Points[i] = p1;
            Gizmos.DrawSphere(p1, 1);
        }

        for (int i = 0; i < len; i++)
        {
            int nextIndex = i + 2;
            Vector3 face1P1 = subface1Points[i];
            Vector3 face2P1 = subface2Points[i];
            if (nextIndex < len)
            {
                Vector3 face1P2 = subface1Points[nextIndex];
                Gizmos.DrawLine(face1P1, face1P2);

                Vector3 face2P2 = subface2Points[nextIndex];
                Gizmos.DrawLine(face2P1, face2P2);

            }
            Gizmos.DrawLine(face1P1, face2P1);

            nextIndex = i + 1;
            if (nextIndex >= len) nextIndex = 0;
            Vector3 face3P1 = subface3Points[i];
            Vector3 face3P2 = subface3Points[nextIndex];
            Gizmos.DrawLine(face3P1, face3P2);

        }
        */

        Gizmos.color = Color.red;
        {
            Gizmos.DrawSphere(pairA1, 1);
            Gizmos.DrawSphere(pairB1, 1.0f);
            Gizmos.DrawSphere(pairC1, 1.0f);
            Gizmos.DrawSphere(pairD1, 1.0f);

            //Gizmos.DrawLine(pairA1, pairB1);
            //Gizmos.DrawLine(pairB1, pairC1);
            //Gizmos.DrawLine(pairC1, pairD1);
            //Gizmos.DrawLine(pairD1, pairA1);
        }

        Gizmos.color = originalColor;
        Gizmos.color = Color.yellow;
        {
            Gizmos.DrawSphere(max, 1.0f);
            Gizmos.DrawSphere(new Vector3(max.x, min.y, max.z), 1.0f);
            Gizmos.DrawSphere(new Vector3(min.x, max.y, max.z), 1.0f);
            Gizmos.DrawSphere(new Vector3(min.x, min.y, max.z), 1.0f);

            //Gizmos.DrawLine(pairA2, pairB2);
            //Gizmos.DrawLine(pairB2, pairC2);
            //Gizmos.DrawLine(pairC2, pairD2);
            //Gizmos.DrawLine(pairD2, pairA2);
        }
        Gizmos.color = originalColor;

    }
}
