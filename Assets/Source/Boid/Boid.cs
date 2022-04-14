using UnityEngine;

public class Boid : ITreeChild
{
    public Bounds Bounds
    {
        get { return mBoxCollider.bounds; }
    }
    private BoxCollider mBoxCollider;
    public Boid(GameObject gameObject)
    {
        mBoxCollider = gameObject.GetComponent<BoxCollider>();
    }
}
