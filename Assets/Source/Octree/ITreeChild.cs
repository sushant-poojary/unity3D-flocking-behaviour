using UnityEngine;

public interface ITreeChild
{
    public Bounds Bounds { get; }
    public Vector3 Position { get; }
    public string ToString();
    public string ID { get; }
}