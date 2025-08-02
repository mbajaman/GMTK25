using UnityEngine;
using System.Collections.Generic;

public class LoopRecorder : MonoBehaviour
{
    private struct State 
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    private List<State> history = new List<State>();
    public float recordTime = 60f;
    private float timePerFrame;
    private int maxFrames;

    private bool rewinding = false;
    public void StartRewind() => rewinding = true;
    public void StopRewind() => rewinding = false;

    void Start()
    {
        timePerFrame = Time.fixedDeltaTime;
        maxFrames = Mathf.RoundToInt(recordTime / timePerFrame);
    }

    void FixedUpdate()
    {
        if (!rewinding)
        {
            if (history.Count >= maxFrames)
            {
                history.RemoveAt(0);
            }
            history.Add(new State { position = transform.position, rotation = transform.rotation });
        }
    }

    void Update()
    {
        if (rewinding && history.Count > 0)
        {
            var state = history[history.Count - 1];
            transform.position = state.position;
            transform.rotation = state.rotation;
            history.RemoveAt(history.Count - 1);
        }
    }
}
