using UnityEngine;
using UnityEngine.InputSystem;

// Colocar en el jugador (o en un objeto hijo de la cámara).
// Permite tomar y dejar cubos mediante botones de UI (OnClick) o acciones del Input System.
public class PlayerCubeInteractor : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform carryPoint;                // vacío frente a la cámara

    [Header("Acciones de Entrada (Input System)")]
    [SerializeField] private InputActionReference interactAction; // Botón general de interacción (opcional)
    [SerializeField] private InputActionReference grabAction;     // Botón específico para Tomar (opcional)
    [SerializeField] private InputActionReference dropAction;     // Botón específico para Dejar/Apilar (opcional)

    [Header("Configuración")]
    [SerializeField] private float maxGrabDistance = 3f;
    [SerializeField] private LayerMask cubeLayer = ~0;
    [SerializeField] private LayerMask stackZoneLayer = ~0;
    [SerializeField] private float carrySmoothing = 12f;

    [Header("Depuración / Mensajes")]
    [SerializeField] private bool showDebugLogs = true;

    private CubeItem heldCube;
    private CubeItem hoveredCube;
    private StackZone hoveredZone;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (carryPoint == null && playerCamera != null)
        {
            GameObject cp = new GameObject("DefaultCarryPoint");
            cp.transform.SetParent(playerCamera.transform);
            cp.transform.localPosition = new Vector3(0, -0.2f, 1.5f);
            carryPoint = cp.transform;
        }
    }

    private void OnEnable()
    {
        EnableAction(interactAction, OnInteractActionCallback);
        EnableAction(grabAction, OnGrabActionCallback);
        EnableAction(dropAction, OnDropActionCallback);
    }

    private void OnDisable()
    {
        DisableAction(interactAction, OnInteractActionCallback);
        DisableAction(grabAction, OnGrabActionCallback);
        DisableAction(dropAction, OnDropActionCallback);
    }

    private void EnableAction(InputActionReference actionRef, System.Action<InputAction.CallbackContext> callback)
    {
        if (actionRef != null && actionRef.action != null)
        {
            actionRef.action.Enable();
            actionRef.action.performed += callback;
        }
    }

    private void DisableAction(InputActionReference actionRef, System.Action<InputAction.CallbackContext> callback)
    {
        if (actionRef != null && actionRef.action != null)
        {
            actionRef.action.performed -= callback;
        }
    }

    private void OnInteractActionCallback(InputAction.CallbackContext ctx) => OnInteractButtonPressed();
    private void OnGrabActionCallback(InputAction.CallbackContext ctx) => OnGrabButtonPressed();
    private void OnDropActionCallback(InputAction.CallbackContext ctx) => OnDropButtonPressed();

    // =========================================================================
    // MÉTODOS PÚBLICOS PARA ASIGNAR EN EL INSPECTOR A LOS BOTONES DE UI (OnClick)
    // =========================================================================

    /// <summary>
    /// Asignar al evento OnClick() de un Botón "TOMAR" en la UI.
    /// </summary>
    public void OnGrabButtonPressed()
    {
        if (heldCube != null)
        {
            if (showDebugLogs) Debug.LogWarning("[PlayerCubeInteractor] Botón Tomar presionado, pero ya sostienes un cubo.");
            return;
        }

        if (showDebugLogs) Debug.Log("[PlayerCubeInteractor] Botón Tomar presionado -> Intentando agarrar...");
        TryGrab();
    }

    /// <summary>
    /// Asignar al evento OnClick() de un Botón "DEJAR" / "APILAR" en la UI.
    /// </summary>
    public void OnDropButtonPressed()
    {
        if (heldCube == null)
        {
            if (showDebugLogs) Debug.LogWarning("[PlayerCubeInteractor] Botón Dejar presionado, pero no sostienes ningún cubo.");
            return;
        }

        if (showDebugLogs) Debug.Log("[PlayerCubeInteractor] Botón Dejar presionado -> Intentando soltar/apilar...");
        TryPlaceOrDrop();
    }

    /// <summary>
    /// Asignar al evento OnClick() de un Botón General de "INTERACTUAR".
    /// Si no sostiene nada -> Toma el cubo. Si sostiene un cubo -> Lo suelta o apila.
    /// </summary>
    public void OnInteractButtonPressed()
    {
        if (heldCube == null)
            OnGrabButtonPressed();
        else
            OnDropButtonPressed();
    }

    // Compatibilidad con PlayerInput (Send Messages / Unity Events)
    public void OnGrab(InputValue value) { if (value.isPressed) OnGrabButtonPressed(); }
    public void OnDrop(InputValue value) { if (value.isPressed) OnDropButtonPressed(); }
    public void OnInteract(InputValue value) { if (value.isPressed) OnInteractButtonPressed(); }

    private void Update()
    {
        if (heldCube != null)
        {
            if (carryPoint != null)
            {
                heldCube.transform.position = Vector3.Lerp(
                    heldCube.transform.position, carryPoint.position, Time.deltaTime * carrySmoothing);
                heldCube.transform.rotation = Quaternion.Lerp(
                    heldCube.transform.rotation, carryPoint.rotation, Time.deltaTime * carrySmoothing);
            }

            UpdateZoneFeedback();
        }
        else
        {
            UpdateHoverFeedback();
        }
    }

    // Resalta el cubo que el crosshair está apuntando, mientras no se cargue ninguno
    private void UpdateHoverFeedback()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        CubeItem newHover = null;

        if (Physics.Raycast(ray, out RaycastHit hit, maxGrabDistance, cubeLayer))
            newHover = hit.collider.GetComponent<CubeItem>();

        if (newHover == hoveredCube) return;

        if (hoveredCube != null) hoveredCube.SetHighlighted(false);
        hoveredCube = newHover;
        if (hoveredCube != null) hoveredCube.SetHighlighted(true);
    }

    // Pinta la StackZone en verde/rojo mientras el jugador carga un cubo y apunta hacia ella
    private void UpdateZoneFeedback()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        StackZone newZone = null;

        if (Physics.Raycast(ray, out RaycastHit hit, maxGrabDistance, stackZoneLayer))
            newZone = hit.collider.GetComponent<StackZone>();

        if (newZone != hoveredZone && hoveredZone != null)
            hoveredZone.SetZoneState(null);

        hoveredZone = newZone;

        if (hoveredZone != null)
        {
            Vector3 checkPos = carryPoint != null ? carryPoint.position : playerCamera.transform.position;
            bool valid = !hoveredZone.IsFull && hoveredZone.IsWithinPlacementRange(checkPos);
            hoveredZone.SetZoneState(valid);
        }
    }

    private void TryGrab()
    {
        if (playerCamera == null)
        {
            if (showDebugLogs) Debug.LogWarning("[PlayerCubeInteractor] No hay 'playerCamera' asignada.");
            return;
        }

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out RaycastHit hit, maxGrabDistance, cubeLayer))
        {
            if (showDebugLogs) Debug.Log("[PlayerCubeInteractor] TryGrab: El Raycast NO impactó ningún objeto en la capa configurada.");
            return;
        }

        CubeItem cube = hit.collider.GetComponent<CubeItem>();
        if (cube == null)
        {
            if (showDebugLogs) Debug.Log($"[PlayerCubeInteractor] TryGrab: Se impactó '{hit.collider.name}', pero no tiene el script CubeItem.");
            return;
        }

        if (cube.IsHeld || cube.IsPlaced)
        {
            if (showDebugLogs) Debug.Log($"[PlayerCubeInteractor] TryGrab: El cubo '{cube.name}' ya está " + (cube.IsHeld ? "siendo sostenido." : "colocado."));
            return;
        }

        // El cubo pasa de "resaltado" a "sostenido"; limpiamos la referencia de hover
        if (hoveredCube == cube) hoveredCube = null;

        heldCube = cube;
        heldCube.Grab();

        if (showDebugLogs) Debug.Log($"[PlayerCubeInteractor] ¡ÉXITO! Cubo agarrado: {cube.name}");
    }

    private void TryPlaceOrDrop()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        StackZone zone = null;

        if (Physics.Raycast(ray, out RaycastHit hit, maxGrabDistance, stackZoneLayer))
            zone = hit.collider.GetComponent<StackZone>();

        Vector3 checkPos = carryPoint != null ? carryPoint.position : playerCamera.transform.position;

        // Solo encaja si el jugador está mirando la zona Y el cubo cargado
        // está dentro del radio de tolerancia del siguiente slot del stack.
        if (zone != null && !zone.IsFull && zone.IsWithinPlacementRange(checkPos))
        {
            Vector3 slot = zone.GetNextSlotPosition();
            heldCube.SnapPlace(slot, Quaternion.identity);
            zone.RegisterCube(heldCube);

            if (showDebugLogs) Debug.Log($"[PlayerCubeInteractor] Cubo apilado con éxito en {zone.name}.");
        }
        else
        {
            // No hubo colocación válida: se suelta con física normal
            // para que el usuario pueda corregir e intentarlo de nuevo.
            heldCube.Release();

            if (showDebugLogs) Debug.Log("[PlayerCubeInteractor] Cubo soltado.");
        }

        // La zona vuelve a su color neutro hasta que se cargue otro cubo hacia ella
        if (hoveredZone != null)
        {
            hoveredZone.SetZoneState(null);
            hoveredZone = null;
        }

        heldCube = null;
    }
}





