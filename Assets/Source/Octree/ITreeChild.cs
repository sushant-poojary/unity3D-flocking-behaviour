using UnityEngine;

public interface ITreeChild/*<T> where T : ITreeChild<T>*/
{
    public Bounds GetBounds();
    public Vector3 Position { get; }
    public string ToString();
    public string ID { get; }
    //public OctTree<T>.OctNode ContainerNode { get; }
}