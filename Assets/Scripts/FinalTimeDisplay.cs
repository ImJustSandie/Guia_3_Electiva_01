using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Muestra en UI (TMP o Text) el tiempo final logrado en la escena anterior.
/// Coloca este componente en la escena final (ej: Pantalla de Victoria / Resultados).
/// </summary>
public class FinalTimeDisplay : MonoBehaviour
{
    [Header("Referencias de UI (Opcional)")]
    [Tooltip("Componente TextMeshPro para mostrar el tiempo. Si se deja nulo, intentará buscar uno en este mismo GameObject.")]
    [SerializeField] private TMP_Text tmpTextDisplay;

    [Tooltip("Componente UI Text tradicional para mostrar el tiempo.")]
    [SerializeField] private Text legacyTextDisplay;

    [Header("Configuración")]
    [Tooltip("Clave de PlayerPrefs donde se guardó el tiempo (debe coincidir con la de SceneTimer)")]
    [SerializeField] private string playerPrefsKey = "FinalTime";

    [Tooltip("Texto previo al tiempo (ej: 'Tu tiempo final fue: ')")]
    [SerializeField] private string prefix = "Tiempo Final: ";

    [Tooltip("Texto posterior al tiempo (ej: ' s')")]
    [SerializeField] private string suffix = "";

    [Tooltip("Texto por defecto si no hay tiempo registrado previo")]
    [SerializeField] private string defaultIfEmpty = "--:--";

    private void Start()
    {
        DisplayFinalTime();
    }

    /// <summary>
    /// Lee el tiempo registrado y actualiza el texto en pantalla.
    /// </summary>
    public void DisplayFinalTime()
    {
        string formattedTime = "";

        // 1. Intentar desde la variable en memoria de la sesión actual
        if (!string.IsNullOrEmpty(SceneTimer.LastFormattedTime))
        {
            formattedTime = SceneTimer.LastFormattedTime;
        }
        // 2. Si no, leer de PlayerPrefs guardado previamente
        else if (PlayerPrefs.HasKey(playerPrefsKey + "_Formatted"))
        {
            formattedTime = PlayerPrefs.GetString(playerPrefsKey + "_Formatted");
        }
        else if (PlayerPrefs.HasKey(playerPrefsKey))
        {
            float seconds = PlayerPrefs.GetFloat(playerPrefsKey);
            int min = Mathf.FloorToInt(seconds / 60f);
            int sec = Mathf.FloorToInt(seconds % 60f);
            formattedTime = string.Format("{0:00}:{1:00}", min, sec);
        }
        else
        {
            formattedTime = defaultIfEmpty;
        }

        string fullText = prefix + formattedTime + suffix;

        // Asignar a TMP_Text si está configurado
        if (tmpTextDisplay != null)
        {
            tmpTextDisplay.text = fullText;
        }

        // Asignar a UI Text si está configurado
        if (legacyTextDisplay != null)
        {
            legacyTextDisplay.text = fullText;
        }

        // Si no se arrastró ninguna referencia al Inspector, auto-detectar en el propio GameObject
        if (tmpTextDisplay == null && legacyTextDisplay == null)
        {
            var tmp = GetComponent<TMP_Text>();
            if (tmp != null)
            {
                tmp.text = fullText;
            }

            var legacy = GetComponent<Text>();
            if (legacy != null)
            {
                legacy.text = fullText;
            }
        }
    }
}
