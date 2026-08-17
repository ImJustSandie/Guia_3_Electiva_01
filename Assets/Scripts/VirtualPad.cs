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
    private Rigidbody rb;
    private Vector3 desiredMove;

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

        rb = character != null ? character.GetComponent<Rigidbody>() : null;
        if (rb == null)
            Debug.LogError("VirtualPad: el personaje no tiene un Rigidbody asignado.");

        if (camara != null)
        {
            Vector3 currentAngles = camara.transform.eulerAngles;
            yaw = currentAngles.y;
            pitch = currentAngles.x > 180f ? currentAngles.x - 360f : currentAngles.x;
        }
    }

    void Update()
    {
        if (character == null || camara == null)
            return;

        RotateCameraFromTouch();
        ReadMoveInput();
    }

    void FixedUpdate()
    {
        if (rb == null)
            return;

        Vector3 current = rb.linearVelocity;
        current.y = 0f;
        rb.AddForce(desiredMove - current, ForceMode.VelocityChange);
    }

    void ReadMoveInput()
    {
        moveInput = moveAction.ReadValue<Vector2>();

        if (moveInput.sqrMagnitude > 0.01f)
        {
            moveInput = moveInput.normalized;

            // Obtener la orientación horizontal (XZ) de la cámara
            Vector3 camForward = camara.transform.forward;
            Vector3 camRight = camara.transform.right;

            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            // Velocidad horizontal objetivo según hacia dónde mira la cámara
            desiredMove = (camForward * moveInput.y + camRight * moveInput.x) * speed;
        }
        else
        {
            desiredMove = Vector3.zero;
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