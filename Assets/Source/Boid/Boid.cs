using UnityEngine;

public class Boid : ITreeChild
{
    public Bounds Bounds
    {
        get { return mCollider.bounds; }
    }

    public OctTree<Boid>.OctNode ContainerNode { get; set; }
    public Vector3 Position => mTransform.position;
    public string ID { get; private set; }
    private SkinnedMeshRenderer mMeshRenderer;
    private BoxCollider mCollider;
    private GameObject mGameObject;
    private Transform mTransform;
    public Boid(GameObject gameObject, string ID)
    {
        this.ID = ID;
        mGameObject = gameObject;
        mTransform = mGameObject.transform;
        mMeshRenderer = mGameObject.GetComponentInChildren<SkinnedMeshRenderer>();
        mCollider = mGameObject.GetComponent<BoxCollider>();
    }

    public void Move(Vector3 force)
    {
       mTransform.position += (force * Time.deltaTime);
    }

    public override string ToString()
    {
        return base.ToString();
    }
}
