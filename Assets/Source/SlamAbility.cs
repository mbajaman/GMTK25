using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Experimental.GraphView.GraphView;

public class SlamAbility : MonoBehaviour
{
    public InputActionAsset InputActions;

    private InputAction _slamAction;
    private Rigidbody _rb;
    private const string ACTION_MAP_NAME = "Player";
    private const string ACTION_NAME = "Slam";

    private void OnEnable()
    {
        InputActions.FindActionMap(ACTION_MAP_NAME).Enable();
    }

    private void OnDisable()
    {
        InputActions.FindActionMap(ACTION_MAP_NAME).Disable();
    }

    private void Awake()
    {
        _slamAction = InputActions.FindActionMap(ACTION_MAP_NAME).FindAction(ACTION_NAME);
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (_slamAction.WasPressedThisFrame())
        {
            Slam();
        }
    }

    private void Slam()
    {
        Debug.Log("Slam");
    }
}
