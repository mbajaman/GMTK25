using StarterAssets;
using System;
using UnityEngine;

public class PlatformSlam : MonoBehaviour
{
    [SerializeField] private GameObject _player;
    [SerializeField] [MinAttribute(1f)] private float _minHeightToBounce = 5f;
    [SerializeField] [MinAttribute(1f)] private float _bounceMultiplier = 2f;

    private FirstPersonController _controller;
    private Rigidbody _rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (_player == null)
        {
            throw new NullReferenceException("PlatformSlam Error: Player field is null.");
        } 

        _controller = _player.GetComponent<FirstPersonController>();
        _rb = _player.GetComponent<Rigidbody>();

        if (_controller == null)
        {
            throw new NullReferenceException("PlatformSlam Error: PlayerCapsule does not have script 'FirstPersonController'.");
        } else if (_rb == null)
        {
            throw new NullReferenceException("PlatformSlam Error: PlayerCapsule does not have RigidBody Component.");
        }
            
    }

    void FixedUpdate()
    {
        _rb.AddForce(transform.right * 1000f, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject != _player)
        {
            return;
        }
        
        Debug.Log("hi");
    }
}
