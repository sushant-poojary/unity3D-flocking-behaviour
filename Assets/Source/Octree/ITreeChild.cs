using UnityEngine;

public interface ITreeChild
{
    public Bounds GetBounds();
    public Vector3 Position { get; }
    public string ToString();
    public string ID { get; }
}