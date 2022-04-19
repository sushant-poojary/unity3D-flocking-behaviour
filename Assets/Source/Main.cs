using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Main : MonoBehaviour
{
    private const float BOUNDARY_BUFFER = 4;
    private bool mStartMovement;
    public GameObject BirdPrefab;
    [SerializeField]
    private SpatialOctree mSpatialTree;
    [SerializeField]
    private BoidConfig mBoidConfig;
    private List<ITreeChild> mTreeChildren;
    public int NumberOfBirds = 0;
    private int mBoidCount = 0;
    private OctTree<Boid>.OctNode topNode;
    private Bounds mSpaceBounds;

    public float minDistance = 3f;
    public float awarenessRadius = 6;
    // Start is called before the first frame update
    void Start()
    {
        mBoidCount = NumberOfBirds;
        StartCoroutine(GenerateBirds(mBoidCount));

        //Debug.LogError((-19 > -20));
    }

    private IEnumerator GenerateBirds(int count)
    {
        mSpaceBounds = mSpatialTree.SpaceTree.GetRootArea();
        mTreeChildren = new List<ITreeChild>(count);
        var y = Random.Range(mSpaceBounds.min.y + BOUNDARY_BUFFER, mSpaceBounds.max.y - BOUNDARY_BUFFER);
        for (int i = 0; i < count; i++)
        {
            GameObject bird = GameObject.Instantiate<GameObject>(BirdPrefab);
            Boid boid = new Boid(bird, i.ToString(), mBoidConfig);
            mTreeChildren.Add(boid);

            //var x = Random.Range(-1, 2);//Random.Range(mSpaceBounds.min.x + 2f, mSpaceBounds.max.x - 2f);
            //var y = Random.Range(-1, 2);//Random.Range(mSpaceBounds.min.y + 2f, mSpaceBounds.max.y - 2f);
            //var z = Random.Range(-1, 2);//Random.Range(mSpaceBounds.min.z + 2f, mSpaceBounds.max.z - 2f);

            var x = Random.Range(mSpaceBounds.min.x + BOUNDARY_BUFFER, mSpaceBounds.max.x - BOUNDARY_BUFFER);

            var z = Random.Range(mSpaceBounds.min.z + BOUNDARY_BUFFER, mSpaceBounds.max.z - BOUNDARY_BUFFER);

            Vector3 randomPosition = new Vector3(x, y, z);
            bird.transform.position = randomPosition;
            //Physics.SyncTransforms();
            OctTree<Boid>.OctNode container;
            if (mSpatialTree.SpaceTree.Insert(boid, out container))
            {
                boid.ContainerNode = container;
                //Debug.LogError("--------------------------[START]container: " + container.ID);
            }
            yield return new WaitForEndOfFrame();
        }
        mTreeChildren.TrimExcess();
        mStartMovement = true;
        //mStartMovement = true;
    }

    //private void GetSeperation()
    //{
    //    var desiredSeparation = getBoidViewDistance() / 2;

    //    var desired = new Victor(0, 0);

    //    // For every flockmate, check if it's too close
    //    for (var i = 0, l = boids.length; i < l; ++i)
    //    {
    //        var other = boids[i];
    //        var dist = this.position.distance(other.position);
    //        if (dist < desiredSeparation && dist > 0)
    //        {
    //            // Calculate vector pointing away from the flockmate, weighted by distance
    //            var diff = this.position.clone().subtract(other.position).normalize().divideScalar(dist);
    //            desired.add(diff);
    //        }
    //    }

    //    // If the boid had flockmates to separate from
    //    if (desired.length() > 0)
    //    {
    //        // We set the average vector to the length of our desired speed
    //        desired.normalize().multiplyScalar(getDesiredSpeed());

    //        // We then calculate the steering force needed to get to that desired velocity
    //        return this.steer(desired);
    //    }

    //    return desired;
    //}
    List<ITreeChild> neighs;
    // Update is called once per frame
    void Update()
    {
        if (!mStartMovement) return;
        for (int i = 0; i < mBoidCount; i++)
        {
            Boid boid = (Boid)mTreeChildren[i];
            //neighs = mSpatialTree.SpaceTree.FindNeighboringChildren(boid, boid.ContainerNode, 2);
            //neighs = mSpatialTree.SpaceTree.DebugFindNeighboringChildren(boid, boid.ContainerNode, 2, out topNode);

            CalculateMovement(boid, ref mTreeChildren);
            //CalculateMovement(boid, ref mTreeChildren, ref cohensionCount, ref seperationCount, ref allignmentCount, ref alignment, ref seperation, ref cohension);

            //Debug.Log($"POS:{boid.Position}   mSpaceBounds:{ mSpaceBounds.min} and max{mSpaceBounds.max}");
            if (boid.Position.x < mSpaceBounds.min.x + BOUNDARY_BUFFER)
            {
                Vector3 currentPos = boid.Position;
                //Debug.Log("Change vel MIN");
                //Debug.Log($"POS:{boid.Position}   mSpaceBounds:{ mSpaceBounds.min} and max{mSpaceBounds.max}");
                currentPos.x = mSpaceBounds.max.x - BOUNDARY_BUFFER;
                boid.ChangePosition(currentPos);
            }
            else if (boid.Position.y < mSpaceBounds.min.y + BOUNDARY_BUFFER)
            {
                Vector3 currentPos = boid.Position;
                currentPos.y = mSpaceBounds.max.y - BOUNDARY_BUFFER;
                boid.ChangePosition(currentPos);
            }
            else if (boid.Position.z < mSpaceBounds.min.z + BOUNDARY_BUFFER)
            {
                Vector3 currentPos = boid.Position;
                currentPos.z = mSpaceBounds.max.z - BOUNDARY_BUFFER;
                boid.ChangePosition(currentPos);
            }
            else if (boid.Position.x > mSpaceBounds.max.x - BOUNDARY_BUFFER)
            {
                //Debug.Log("Change vel MAX");
                //Debug.Log($"POS:{boid.Position}   mSpaceBounds:{ mSpaceBounds.min} and max{mSpaceBounds.max}");
                //boid.ChangeVelocity(boid.GetCurrentVelocity() * -1);
                Vector3 currentPos = boid.Position;
                currentPos.x = mSpaceBounds.min.x + BOUNDARY_BUFFER;
                boid.ChangePosition(currentPos);
            }
            else if (boid.Position.y > mSpaceBounds.max.y - BOUNDARY_BUFFER)
            {
                Vector3 currentPos = boid.Position;
                currentPos.y = mSpaceBounds.min.y + BOUNDARY_BUFFER;
                boid.ChangePosition(currentPos);
            }
            else if (boid.Position.z > mSpaceBounds.max.z - BOUNDARY_BUFFER)
            {
                Vector3 currentPos = boid.Position;
                currentPos.z = mSpaceBounds.min.z + BOUNDARY_BUFFER;
                boid.ChangePosition(currentPos);
            }

            OctTree<Boid>.OctNode container;
            //Debug.Log("Boid to bounds " + boid.GetBounds());
            if (mSpatialTree.SpaceTree.Update(boid, boid.ContainerNode, out container))
            {
                boid.ContainerNode = container;
                //Debug.Log("GOT IT!!!      [Update] container: " + container.ID);
            }
            else
            {
                //Debug.LogError("Failed to update " + boid + " from node:" + boid.ContainerNode);
            }
        }
    }

    private void CalculateMovement(Boid boid, ref List<ITreeChild> neighs)
    {
        int cohensionCount = 0;
        int seperationCount = 0;
        int alignmentCount = 0;
        Vector3 alignment = Vector3.zero;
        Vector3 seperation = Vector3.zero;
        Vector3 cohension = Vector3.zero;
        int length = neighs.Count;
        Vector3 boidPosition = boid.Position;
        Vector3 foward = boid.GetForwardVector();
        for (int neighCount = 0; neighCount < length; neighCount++)
        {
            ITreeChild neighbhor = neighs[neighCount];
            Vector3 position = neighbhor.Position;
            if (neighbhor != boid)
            {
                var difference = position - boidPosition;
                var dist = difference.sqrMagnitude;
                if (dist < awarenessRadius * awarenessRadius)
                {
                    if (dist < minDistance * minDistance)
                    {
                        seperation -= difference;
                        seperationCount++;
                    }
                    //Debug.Log($"dist:{dist}, angle:{angle}");

                    cohension += position;
                    cohensionCount++;
                    float angle = Vector3.Dot(foward, difference.normalized);
                    if (angle >= -0.5f)
                    {
                        alignment += ((Boid)neighbhor).GetCurrentVelocity();
                        alignmentCount++;

                    }
                }
            }
        }


        /*
        for (int j = 0; j < mBoidCount; j++)
        {
            var neighbhor = mTreeChildren[j];
            var position = neighbhor.Position;
            if (neighbhor != boid)
            {
                var difference = boidPosition - position;
                float angle = Vector3.Dot(fov, difference.normalized);
                if (angle > 0.7f)
                {
                    var dist = difference.sqrMagnitude;
                    //Debug.Log($"dist:{dist}, angle:{angle}");
                    cohension += position;
                    if (dist > 0 && dist < minDistance * minDistance)
                    {
                        seperation += difference;
                        seperationCount++;
                    }
                    alignment += ((Boid)neighbhor).GetCurrentVelocity();
                    count++;
                }
            }
        }
        */

        if (alignment != Vector3.zero)
        {
            alignment = alignment / (float)alignmentCount;
        }
        if (cohension != Vector3.zero)
        {
            cohension /= (float)cohensionCount;
        }
        //if (seperation != Vector3.zero)
        //{
        //    seperation = seperation / (float)seperationCount;
        //    //seperation *= -1;
        //}
        //Debug.Log($"alignment:{alignment}, cohension:{cohension}, seperation:{seperation} neighs.Count:{neighs.Count}");
        boid.Move(alignment, seperation, cohension);
    }

    private void OnGUI()
    {
        if (GUILayout.Button("stop"))
        {
            mStartMovement = false;
        }
        if (GUILayout.Button("Start"))
        {
            mStartMovement = true;
        }
        if (GUILayout.Button(" FInd Neighbhors"))
        {
            var boid = (Boid)mTreeChildren[5];
            var neighs = mSpatialTree.SpaceTree.DebugFindNeighboringChildren(boid, boid.ContainerNode, 2, out topNode);
        }
    }
    /*
    private void OnDrawGizmos()
    {
        Color originalColor = Gizmos.color;
        if (mTreeChildren != null)
        {
            foreach (var item in mTreeChildren)
            {
                if (((Boid)item).ContainerNode != null)
                {
                    Bounds bounds = ((Boid)item).ContainerNode.BoundingBox;
                    if (bounds.size != Vector3.zero)
                    {
                        Color yellow = Color.yellow;
                        yellow.a = 0.7f;
                        Gizmos.color = yellow;
                        Gizmos.DrawCube(bounds.center, bounds.size);
                        Gizmos.color = originalColor;
                    }
                }
            }
        }

        if (mTreeChildren != null && mTreeChildren.Count > 0 && topNode != null)
        {
            Bounds bds = topNode.BoundingBox;
            Color red = Color.red;
            red.a = 0.4f;
            Gizmos.color = red;
            Gizmos.DrawCube(bds.center, bds.size);
            Gizmos.color = originalColor;

            //var boid = mTreeChildren[5];
            //var bounds = new Bounds(boid.Position, new Vector3(3, 3, 3));
            //Color blue = Color.blue;
            //blue.a = 0.3f;
            //Gizmos.color = blue;
            //Gizmos.DrawCube(bounds.center, bounds.size);
            //Gizmos.color = originalColor;
        }
    
    }
    */
}
