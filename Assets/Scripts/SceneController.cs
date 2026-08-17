using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Controlador y cambiador de escenas para Unity.
/// Permite cambiar de escena por código, eventos UI, o por colisión/trigger con el Player u objetos específicos.
/// </summary>
public class SceneChanger : MonoBehaviour
{
    public enum TipoDeteccion
    {
        PorTag,             // Detecta objetos con un Tag específico (ej: "Player", "Finish", etc.)
        ObjetoEspecifico,   // Detecta solo un GameObject asignado explícitamente
        CualquierObjeto     // Cambia de escena con cualquier colisión
    }

    [Header("Escena Destino")]
    [Tooltip("Nombre de la escena a cargar")]
    [SerializeField] private string nombreEscena;

#if UNITY_EDITOR
    [Tooltip("Arrastra aquí la escena desde el Project view (solo en Editor)")]
    [SerializeField] private SceneAsset escena;

    private void OnValidate()
    {
        if (escena != null)
        {
            nombreEscena = escena.name;
        }
    }
#endif

    [Header("Configuración de Colisión / Trigger")]
    [Tooltip("Activa la detección automática de colisión para cambiar de escena")]
    [SerializeField] private bool cambiarPorColision = true;

    [Tooltip("Modo para filtrar con qué objeto se debe chocar")]
    [SerializeField] private TipoDeteccion modoDeteccion = TipoDeteccion.PorTag;

    [Tooltip("Tag del objeto que debe chocar para activar el cambio (ej: 'Player' o 'Finish')")]
    [SerializeField] private string tagObjetivo = "Player";

    [Tooltip("Objeto específico con el que se debe chocar si el modo es ObjetoEspecifico")]
    [SerializeField] private GameObject objetoEspecifico;

    [Tooltip("Tiempo de espera opcional (segundos) antes de cargar la escena")]
    [SerializeField] private float delaySegundos = 0f;

    private bool yaCambiando = false;

    /// <summary>
    /// Carga la escena configurada en el inspector.
    /// </summary>
    public void CambiarEscena()
    {
        if (yaCambiando) return;

        if (string.IsNullOrEmpty(nombreEscena))
        {
            Debug.LogWarning("[SceneChanger] No se ha especificado un nombre de escena destino.");
            return;
        }

        yaCambiando = true;
        if (delaySegundos > 0f)
        {
            Invoke(nameof(EjecutarCambioEscena), delaySegundos);
        }
        else
        {
            EjecutarCambioEscena();
        }
    }

    /// <summary>
    /// Carga una escena específica pasando su nombre por parámetro.
    /// </summary>
    public void CambiarEscena(string nombreDeEscena)
    {
        if (yaCambiando) return;
        if (string.IsNullOrEmpty(nombreDeEscena)) return;

        yaCambiando = true;
        SceneManager.LoadScene(nombreDeEscena);
    }

    private void EjecutarCambioEscena()
    {
        SceneManager.LoadScene(nombreEscena);
    }

    // Detectar colisiones físicas (3D)
    private void OnCollisionEnter(Collision collision)
    {
        if (!cambiarPorColision || yaCambiando) return;

        if (EsObjetoCorrecto(collision.gameObject))
        {
            CambiarEscena();
        }
    }

    // Detectar triggers (3D)
    private void OnTriggerEnter(Collider other)
    {
        if (!cambiarPorColision || yaCambiando) return;

        if (EsObjetoCorrecto(other.gameObject))
        {
            CambiarEscena();
        }
    }

    // Detectar colisiones físicas (2D) por compatibilidad
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!cambiarPorColision || yaCambiando) return;

        if (EsObjetoCorrecto(collision.gameObject))
        {
            CambiarEscena();
        }
    }

    // Detectar triggers (2D) por compatibilidad
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!cambiarPorColision || yaCambiando) return;

        if (EsObjetoCorrecto(other.gameObject))
        {
            CambiarEscena();
        }
    }

    /// <summary>
    /// Evalúa si el objeto con el que se chocó cumple con las condiciones de detección.
    /// </summary>
    private bool EsObjetoCorrecto(GameObject obj)
    {
        if (obj == null) return false;

        switch (modoDeteccion)
        {
            case TipoDeteccion.PorTag:
                return !string.IsNullOrEmpty(tagObjetivo) && obj.CompareTag(tagObjetivo);

            case TipoDeteccion.ObjetoEspecifico:
                return objetoEspecifico != null && obj == objetoEspecifico;

            case TipoDeteccion.CualquierObjeto:
                return true;

            default:
                return false;
        }
    }
}

/// <summary>
/// Alias para permitir usar 'SceneController' o 'SceneChanger' indistintamente.
/// </summary>
public class SceneController : SceneChanger
{
}
