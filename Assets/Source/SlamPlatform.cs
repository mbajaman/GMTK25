using StarterAssets;
using UnityEngine;

public class SlamPlatform : MonoBehaviour
{
    [SerializeField] [MinAttribute(0.1f)] private float _bounceHeight = 15f;

    private void Awake()
    {
        float PlayerJumpHeight = GameObject.Find("PlayerCapsule").GetComponent<FirstPersonController>().JumpHeight;
        if (PlayerJumpHeight > _bounceHeight)
        {
            Debug.LogWarning("SlamPlatform Warning: Player's jump height (" + PlayerJumpHeight + ") is greater than this platform's Bounce Height (" +  _bounceHeight + ")");
        }
    }

    public float BounceHeight
    {
        get => _bounceHeight;
    }
}
