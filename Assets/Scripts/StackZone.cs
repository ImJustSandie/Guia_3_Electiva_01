using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

// Colocar en el objeto del Punto B, con un Collider (puede ser trigger) que
// delimite visualmente la zona de apilado.
public class StackZone : MonoBehaviour
{
    [SerializeField] private float cubeHeight = 1f;       // alto de cada cubo (ajusta a tu escala)
    [SerializeField] private float placementRadius = 0.6f; // tolerancia horizontal para "encajar"
    [SerializeField] private int maxCubes = 4;

    [Header("Objetos / Eventos al Completar")]
    [Tooltip("Objeto que desaparecerá automáticamente al completar los cubos requeridos.")]
    [SerializeField] private GameObject objectToDisappear;

    [Tooltip("Temporizador opcional que se detendrá automáticamente al completar la pila de cubos.")]
    [SerializeField] private SceneTimer timerToStop;

    [Tooltip("Si es true y no se asignó timerToStop, buscará automáticamente el SceneTimer activo en la escena.")]
    [SerializeField] private bool autoStopSceneTimer = true;

    [Tooltip("Eventos de Unity opcionales al completar la pila de cubos.")]
    [SerializeField] private UnityEvent onStackCompleted;

    [Header("Feedback visual")]
    [SerializeField] private Renderer zoneRenderer;
    [SerializeField] private string colorProperty = "_BaseColor"; // URP. En Built-in RP usa "_Color"
    [SerializeField] private Color idleColor = new Color(1f, 1f, 1f, 0.35f);
    [SerializeField] private Color validColor = new Color(0.3f, 0.9f, 0.4f, 0.5f);
    [SerializeField] private Color invalidColor = new Color(0.9f, 0.3f, 0.3f, 0.5f);

    private MaterialPropertyBlock propBlock;
    private readonly List<CubeItem> placedCubes = new List<CubeItem>();

    public int PlacedCount => placedCubes.Count;
    public int MaxCount => maxCubes;
    public bool IsFull => placedCubes.Count >= maxCubes;

    private void Awake()
    {
        propBlock = new MaterialPropertyBlock();
        SetZoneState(null);
    }

    // valid = null -> color neutro (nadie está apuntando con un cubo cargado)
    // valid = true/false -> verde si encajaría, rojo si no
    public void SetZoneState(bool? valid)
    {
        if (zoneRenderer == null) return;
        Color c = valid == null ? idleColor : (valid.Value ? validColor : invalidColor);
        propBlock.SetColor(colorProperty, c);
        zoneRenderer.SetPropertyBlock(propBlock);
    }

    // Posición del próximo cubo en el stack (encima del último colocado)
    public Vector3 GetNextSlotPosition()
    {
        Vector3 basePos = transform.position;
        return basePos + Vector3.up * (cubeHeight * placedCubes.Count);
    }

    // Verifica si una posición (por ejemplo, el cubo que el jugador está cargando)
    // está lo bastante cerca del slot correcto para considerarse una colocación válida
    public bool IsWithinPlacementRange(Vector3 worldPosition)
    {
        Vector3 slot = GetNextSlotPosition();
        Vector3 flatDelta = worldPosition - slot;
        flatDelta.y = 0f; // solo importa la distancia horizontal; la altura la define el slot
        return flatDelta.magnitude <= placementRadius;
    }

    public void RegisterCube(CubeItem cube)
    {
        if (placedCubes.Contains(cube)) return;
        placedCubes.Add(cube);

        if (placedCubes.Count >= maxCubes)
        {
            if (objectToDisappear != null)
            {
                objectToDisappear.SetActive(false);
            }

            // Detener el temporizador de la escena
            StopTimerIfPresent();

            SendMessage("OnStackCompleted", SendMessageOptions.DontRequireReceiver);
            onStackCompleted?.Invoke();
        }
    }

    private void StopTimerIfPresent()
    {
        if (timerToStop != null)
        {
            timerToStop.StopTimer();
        }
        else if (autoStopSceneTimer)
        {
            if (SceneTimer.Instance != null)
            {
                SceneTimer.Instance.StopTimer();
            }
            else
            {
                SceneTimer timer = FindObjectOfType<SceneTimer>();
                if (timer != null)
                {
                    timer.StopTimer();
                }
            }
        }
    }

    // Útil para reiniciar la prueba entre usuarios
    public void ResetZone()
    {
        placedCubes.Clear();
        if (objectToDisappear != null)
        {
            objectToDisappear.SetActive(true);
        }
    }
}
