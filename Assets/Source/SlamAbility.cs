using StarterAssets;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(FirstPersonController))]
[RequireComponent(typeof(StarterAssetsInputs))]
public class SlamAbility : MonoBehaviour
{
    [SerializeField] private InputActionAsset _inputActions;
    [SerializeField] private CharacterController _controller;
    [SerializeField] private GameObject _mainCamera;

    [Space][Space]
    [SerializeField][MinAttribute(0.1f)] private float _minHeightToSlam = 5f;

    [Tooltip("It's just gravity when player slams onto a viable platform. Positive numbers will be converted to negative.")]
    [SerializeField] private float _slamSpeed = -1000f;

    [Tooltip("Backup in case slam platform doesn't have its own bounce height value.")]
    [SerializeField][MinAttribute(0.1f)] private float _defaultBounceHeight = 15f;

    private InputAction _slamAction;
    private LayerMask _layerMask;
    private FirstPersonController _firstPersonController;
    private StarterAssetsInputs _starterAssetsInputs;
    private float _originalGravity;
    private float _originalJumpHeight;
    private float _bounceHeight;
    private bool _canSlam = false;

    private const float SLAM_COROUTINE_BUFFER = 0.5f; // Please don't fly off into space thanks.
    private const string ACTION_MAP_NAME = "Player";
    private const string ACTION_NAME = "Slam";
    private const string SLAMMABLE_LAYER_NAME = "Slammable"; // honestly this feels unnecessary, but I like typing "const"

    private void OnEnable()
    {
        _inputActions.FindActionMap(ACTION_MAP_NAME).Enable();
    }

    private void OnDisable()
    {
        _inputActions.FindActionMap(ACTION_MAP_NAME).Disable();
    }

    private void Awake()
    {
        _slamAction = _inputActions.FindActionMap(ACTION_MAP_NAME).FindAction(ACTION_NAME);
        _layerMask = LayerMask.GetMask(SLAMMABLE_LAYER_NAME);
        _firstPersonController = GetComponent<FirstPersonController>();
        _starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        _originalGravity = _firstPersonController.Gravity;
        _originalJumpHeight = _firstPersonController.JumpHeight;
        _bounceHeight = _defaultBounceHeight;
        _slamSpeed = _slamSpeed > 0 ? _slamSpeed * -1 : _slamSpeed;

        if (_inputActions == null)
        {
            throw new System.NullReferenceException("SlamAbility Error: Input Actions field is null, please make sure " +
                "\"StarterAssets\" in in there.");
        } else if (_controller == null) 
        {
            throw new System.NullReferenceException("SlamAbility Error: Controller field is null, please make sure PlayerCapsule's " +
                "Character Controller component is in there.");
        } else if (_mainCamera == null) 
        {
            throw new System.NullReferenceException("SlamAbility Error: Main Camera field is null, please make sure MainCamera " +
                "component is in there.");
        } else if (_slamAction == null)
        {
            throw new System.NullReferenceException("SlamAbility Error: Could not find action name \"" + ACTION_NAME + 
                "\" in action map \"" + ACTION_MAP_NAME + "\", please make sure they both exist.");
        }

        if (_slamSpeed >= _originalGravity)
        {
            Debug.LogWarning("SlamAbility Warning: Slam Speed (" + _slamSpeed + ") is greater than or equal to player's original gravity " +
                "value (" + _originalGravity + "). IE: Speed at which the player slams the platform will be SLOWER or the same as falling.");
        }
    }

    private void Update()
    {
        
        if (_slamAction.WasPressedThisFrame() && _canSlam)
        {
            StartCoroutine(SlamCoroutine());
        }
    }

    private void FixedUpdate()
    {
        RaycastHit hit;
        SlamPlatform hitComponent;

        if (Physics.Raycast(_mainCamera.transform.position, _mainCamera.transform.forward, out hit, Mathf.Infinity, _layerMask) &&
            transform.position.y - hit.transform.position.y >= _minHeightToSlam &&
            !_firstPersonController.Grounded)
        {
            Debug.DrawRay(_mainCamera.transform.position, _mainCamera.transform.forward * 10000f, Color.yellow);
            _canSlam= true;
            
            hitComponent = hit.transform.GetComponent<SlamPlatform>();
            _bounceHeight = hitComponent != null ? hitComponent.BounceHeight : _defaultBounceHeight;
        } else
        {
            _canSlam = false;
        }
    }

    private IEnumerator SlamCoroutine()
    {
        _firstPersonController.Gravity = _slamSpeed;
        _firstPersonController.JumpHeight = _bounceHeight;

        while (!_firstPersonController.Grounded)
        {
            yield return null;
        }

        _starterAssetsInputs.JumpInput(true);
        _firstPersonController.Gravity = _originalGravity;

        yield return new WaitForSeconds(SLAM_COROUTINE_BUFFER);
        _firstPersonController.JumpHeight = _originalJumpHeight;
    }
}
