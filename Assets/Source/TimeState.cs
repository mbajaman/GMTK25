using UnityEngine;

public struct TimeState
{
    public Vector3 position;
    public Quaternion rotation;

    public TimeState(Vector3 pos, Quaternion rot)
    {
        position = pos;
        rotation = rot;
    }
}
