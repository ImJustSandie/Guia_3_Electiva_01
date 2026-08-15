using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

[RequireComponent(typeof(PlayerInput))]
public class VirtualPad : MonoBehaviour
{
    public GameObject character;
    public float speed = 0.5f;
    public Camera camara;
    public float cameraSpeed = 0.1f;
    public float pitchClamp = 85f;

    private PlayerInput playerInput;
    private InputAction moveAction;
    private Vector2 moveInput;

    private float pitch;
    private float yaw;

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null || playerInput.actions == null)
        {
            Debug.LogError("VirtualPad: falta un PlayerInput con una InputActionAsset asignada.");
            return;
        }

        moveAction = playerInput.actions["Move"];
        if (moveAction == null)
            Debug.LogError("VirtualPad: no existe la accion 'Move' en la InputActionAsset.");
    }

    void Update()
    {
        if (character == null || camara == null)
            return;

        RotateCameraFromTouch();
        MoveCharacter();
    }

    void MoveCharacter()
    {
        moveInput = moveAction.ReadValue<Vector2>();

        if (moveInput.sqrMagnitude > 0.01f)
        {
            moveInput = moveInput.normalized;
            Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);
            character.transform.Translate(movement * speed * Time.deltaTime, Space.World);
        }
    }

    void RotateCameraFromTouch()
    {
        foreach (var touch in Touch.activeTouches)
        {
            if (touch.screenPosition.x < Screen.width / 2f)
                continue;

            Vector2 delta = touch.delta;
            yaw += delta.x * cameraSpeed;
            pitch -= delta.y * cameraSpeed;
        }

        pitch = Mathf.Clamp(pitch, -pitchClamp, pitchClamp);
        camara.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}