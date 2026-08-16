using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class CubeItem : MonoBehaviour
{
    [Header("Feedback visual")]
    [SerializeField] private Renderer cubeRenderer;
    [SerializeField] private string colorProperty = "_BaseColor"; // URP. En Built-in RP usa "_Color"
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.2f); // apuntado, sin tomar
    [SerializeField] private Color heldColor = new Color(0.25f, 0.6f, 1f);      // siendo cargado
    [SerializeField] private Color placedColor = new Color(0.3f, 0.9f, 0.4f);   // apilado correctamente

    public bool IsHeld { get; private set; }
    public bool IsPlaced { get; private set; }

    private Rigidbody rb;
    private Collider col;
    private MaterialPropertyBlock propBlock;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        propBlock = new MaterialPropertyBlock();
        ApplyColor(normalColor);
    }

    // Se llama cada frame mientras el crosshair apunta a este cubo (y nadie lo carga)
    public void SetHighlighted(bool isHighlighted)
    {
        if (IsHeld || IsPlaced) return; // no pisar un estado más importante
        ApplyColor(isHighlighted ? highlightColor : normalColor);
    }

    // Se llama cuando el jugador toma el cubo (tap sobre él)
    public void Grab()
    {
        IsHeld = true;
        IsPlaced = false;
        rb.isKinematic = true;   // deja de reaccionar a la física mientras se carga
        col.enabled = false;     // evita que choque con el jugador durante el traslado
        ApplyColor(heldColor);
        SendMessage("OnCubeGrabbed", SendMessageOptions.DontRequireReceiver);
    }

    // Se llama cuando el jugador suelta el cubo fuera de una zona válida
    public void Release()
    {
        IsHeld = false;
        rb.isKinematic = false;  // vuelve a caer con física normal
        col.enabled = true;
        ApplyColor(normalColor);
        SendMessage("OnCubeDropped", SendMessageOptions.DontRequireReceiver);
    }

    // Se llama cuando el cubo encaja correctamente en la StackZone
    public void SnapPlace(Vector3 position, Quaternion rotation)
    {
        IsHeld = false;
        IsPlaced = true;
        rb.isKinematic = true;   // se queda fijo una vez apilado
        col.enabled = true;
        transform.SetPositionAndRotation(position, rotation);
        ApplyColor(placedColor);
        SendMessage("OnCubePlaced", SendMessageOptions.DontRequireReceiver);
    }

    private void ApplyColor(Color color)
    {
        if (cubeRenderer == null) return;
        propBlock.SetColor(colorProperty, color);
        cubeRenderer.SetPropertyBlock(propBlock);
    }
}