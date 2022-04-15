using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Main : MonoBehaviour
{
    private bool mStartMovement;
    public GameObject BirdPrefab;
    [SerializeField]
    private SpatialOctree mSpatialTree;
    private List<Boid> mTreeChildren;
    public int NumberOfBirds = 0;
    private int mBoidCount = 0;
    private OctTree<Boid>.OctNode topNode;
    private Bounds mSpaceBounds;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GenerateBirds(NumberOfBirds));
        mBoidCount = NumberOfBirds;

        //Debug.LogError((-19 > -20));
    }

    private IEnumerator GenerateBirds(int count)
    {
        mSpaceBounds = mSpatialTree.SpaceTree.GetRootArea();
        mTreeChildren = new List<Boid>(count);
        for (int i = 0; i < count; i++)
        {
            GameObject bird = GameObject.Instantiate<GameObject>(BirdPrefab);
            Boid boid = new Boid(bird, i.ToString());
            mTreeChildren.Add(boid);

            var x = Random.Range(-1, 2);//Random.Range(mSpaceBounds.min.x + 2f, mSpaceBounds.max.x - 2f);
            var y = Random.Range(-1, 2);//Random.Range(mSpaceBounds.min.y + 2f, mSpaceBounds.max.y - 2f);
            var z = Random.Range(-1, 2);//Random.Range(mSpaceBounds.min.z + 2f, mSpaceBounds.max.z - 2f);
            Vector3 randomPosition = new Vector3(x, y, z);
            bird.transform.position = randomPosition;
            Physics.SyncTransforms();
            OctTree<Boid>.OctNode container;
            if (mSpatialTree.SpaceTree.Insert(boid, out container))
            {
                boid.ContainerNode = container;
                Debug.LogError("--------------------------[START]container: " + container.ID);
            }
            yield return new WaitForEndOfFrame();
        }

        mTreeChildren.TrimExcess();
        //mStartMovement = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!mStartMovement) return;
        for (int i = 0; i < mBoidCount; i++)
        {
            Boid boid = mTreeChildren[i];
            var neighs = mSpatialTree.SpaceTree.FindNeighboringChildren(boid, boid.ContainerNode, 2);

            var totalVel = new Vector3();
            foreach (ITreeChild item in neighs)
            {
                if (item != boid)
                {
                    totalVel += ((Boid)item).GetForwardVector();
                }
            }

            var avgForce = totalVel / neighs.Count;

            boid.Move(avgForce);

            //Debug.Log($"POS:{boid.Position}   mSpaceBounds:{ mSpaceBounds.min} and max{mSpaceBounds.max}");
            if (boid.Position.x < mSpaceBounds.min.x + 2f || boid.Position.y < mSpaceBounds.min.y + 2f || boid.Position.z < mSpaceBounds.min.z + 2f)
            {
                Debug.Log("Change vel MIN");
                Debug.Log($"POS:{boid.Position}   mSpaceBounds:{ mSpaceBounds.min} and max{mSpaceBounds.max}");
                boid.ChangeVelocity(boid.GetForwardVector() * -2);
            }
            else if (boid.Position.x > mSpaceBounds.max.x - 2f || boid.Position.y > mSpaceBounds.max.y - 2f || boid.Position.z > mSpaceBounds.max.z - 2f)
            {
                Debug.Log("Change vel MAX");
                Debug.Log($"POS:{boid.Position}   mSpaceBounds:{ mSpaceBounds.min} and max{mSpaceBounds.max}");
                boid.ChangeVelocity(boid.GetForwardVector() * -2);
            }

            OctTree<Boid>.OctNode container;
            //Debug.Log("Boid to bounds " + boid.Bounds);
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
            var boid = mTreeChildren[5];
            var neighs = mSpatialTree.SpaceTree.DebugFindNeighboringChildren(boid, boid.ContainerNode, 2, out topNode);
        }
    }

    private void OnDrawGizmos()
    {
        Color originalColor = Gizmos.color;
        if (mTreeChildren != null)
        {
            foreach (var item in mTreeChildren)
            {
                if (item.ContainerNode != null)
                {
                    Bounds bounds = item.ContainerNode.BoundingBox;
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

            var boid = mTreeChildren[5];
            var bounds = new Bounds(boid.Position, new Vector3(3, 3, 3));
            Color blue = Color.blue;
            blue.a = 0.3f;
            Gizmos.color = blue;
            Gizmos.DrawCube(bounds.center, bounds.size);
            Gizmos.color = originalColor;
        }
    }
}
