using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// Componente de Temporizador para Unity.
/// Se inicia automáticamente al comenzar la escena si autoStart está activado.
/// </summary>
public class SceneTimer : MonoBehaviour
{
    public enum TimerType
    {
        CountDown,  // Cuenta regresiva (ej: de 60 a 0)
        CountUp     // Cuenta progresiva (ej: de 0 en adelante)
    }

    public enum DisplayFormat
    {
        MinutesSeconds,      // 01:25
        MinutesSecondsMs,    // 01:25.40
        SecondsOnly,         // 85
        SecondsWithDecimal   // 85.4
    }

    [Header("Configuración del Temporizador")]
    [Tooltip("Modo de temporizador: Cuenta Regresiva (CountDown) o Cuenta Progresiva (CountUp)")]
    [SerializeField] private TimerType timerMode = TimerType.CountDown;

    [Tooltip("Tiempo inicial en segundos (ej: 60 para 1 minuto en regresivo, o 0 para progresivo)")]
    [SerializeField] private float initialTime = 60f;

    [Tooltip("Tiempo objetivo en segundos (solo para CountUp si se desea un límite; 0 = sin límite)")]
    [SerializeField] private float targetTime = 0f;

    [Tooltip("Iniciar automáticamente el temporizador al cargar/iniciar la escena")]
    [SerializeField] private bool autoStart = true;

    [Tooltip("Usar tiempo sin escala (ignora Time.timeScale al pausar el juego)")]
    [SerializeField] private bool useUnscaledTime = false;

    [Header("Referencias de UI (Opcional)")]
    [Tooltip("Componente TextMeshPro (TMP_Text) para mostrar el temporizador")]
    [SerializeField] private TMP_Text tmpTextDisplay;

    [Tooltip("Componente UI Text tradicional para mostrar el temporizador")]
    [SerializeField] private Text legacyTextDisplay;

    [Header("Formato de Pantalla")]
    [SerializeField] private DisplayFormat displayFormat = DisplayFormat.MinutesSeconds;

    [Tooltip("Texto previo al tiempo (ej: 'Tiempo: ')")]
    [SerializeField] private string prefix = "";

    [Tooltip("Texto posterior al tiempo (ej: ' s')")]
    [SerializeField] private string suffix = "";

    [Header("Guardado de Tiempo Final (Entre Escenas)")]
    [Tooltip("Guarda automáticamente el tiempo final en PlayerPrefs y variables estáticas al detenerse o finalizar")]
    [SerializeField] private bool saveFinalTimeOnStop = true;

    [Tooltip("Clave para guardar en PlayerPrefs")]
    [SerializeField] private string playerPrefsKey = "FinalTime";

    [Header("Eventos")]
    [Tooltip("Evento ejecutado al iniciar o reiniciar el temporizador")]
    [SerializeField] private UnityEvent onTimerStart;

    [Tooltip("Evento ejecutado cuando el tiempo llega a 0 (regresivo) o al objetivo (progresivo)")]
    [SerializeField] private UnityEvent onTimerEnd;

    [Tooltip("Evento ejecutado en cada frame enviando el valor flotante actual del tiempo")]
    [SerializeField] private UnityEvent<float> onTimeChanged;

    private float currentTime;
    private bool isRunning = false;
    private bool hasFinished = false;

    /// <summary>
    /// Instancia estática accesible globalmente (Singleton).
    /// </summary>
    public static SceneTimer Instance { get; private set; }

    /// <summary>
    /// Último tiempo flotante registrado al detenerse el temporizador.
    /// </summary>
    public static float LastFinalTime { get; private set; }

    /// <summary>
    /// Último tiempo formateado (string) registrado al detenerse el temporizador.
    /// </summary>
    public static string LastFormattedTime { get; private set; }

    // Propiedades públicas de consulta
    public float CurrentTime => currentTime;
    public bool IsRunning => isRunning;
    public bool HasFinished => hasFinished;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        currentTime = initialTime;
        UpdateUI();

        if (autoStart)
        {
            StartTimer();
        }
    }

    private void Update()
    {
        if (!isRunning || hasFinished) return;

        float delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        if (timerMode == TimerType.CountDown)
        {
            currentTime -= delta;
            if (currentTime <= 0f)
            {
                currentTime = 0f;
                hasFinished = true;
                isRunning = false;
                UpdateUI();
                SaveFinalTimeData();
                onTimeChanged?.Invoke(currentTime);
                onTimerEnd?.Invoke();
                return;
            }
        }
        else // CountUp
        {
            currentTime += delta;
            if (targetTime > 0f && currentTime >= targetTime)
            {
                currentTime = targetTime;
                hasFinished = true;
                isRunning = false;
                UpdateUI();
                SaveFinalTimeData();
                onTimeChanged?.Invoke(currentTime);
                onTimerEnd?.Invoke();
                return;
            }
        }

        UpdateUI();
        onTimeChanged?.Invoke(currentTime);
    }

    /// <summary>
    /// Inicia el temporizador.
    /// </summary>
    public void StartTimer()
    {
        isRunning = true;
        hasFinished = false;
        onTimerStart?.Invoke();
    }

    /// <summary>
    /// Pausa el temporizador.
    /// </summary>
    public void PauseTimer()
    {
        isRunning = false;
    }

    /// <summary>
    /// Reanuda el temporizador si no ha finalizado.
    /// </summary>
    public void ResumeTimer()
    {
        if (!hasFinished)
        {
            isRunning = true;
        }
    }

    /// <summary>
    /// Detiene el temporizador y guarda el tiempo final.
    /// </summary>
    public void StopTimer()
    {
        isRunning = false;
        SaveFinalTimeData();
    }

    /// <summary>
    /// Guarda los datos del tiempo actual en la memoria estática y PlayerPrefs.
    /// </summary>
    public void SaveFinalTimeData()
    {
        LastFinalTime = currentTime;
        LastFormattedTime = GetFormattedTimeString();

        if (saveFinalTimeOnStop && !string.IsNullOrEmpty(playerPrefsKey))
        {
            PlayerPrefs.SetFloat(playerPrefsKey, currentTime);
            PlayerPrefs.SetString(playerPrefsKey + "_Formatted", LastFormattedTime);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Restablece el temporizador a su valor inicial.
    /// </summary>
    public void ResetTimer()
    {
        currentTime = initialTime;
        hasFinished = false;
        UpdateUI();
        onTimeChanged?.Invoke(currentTime);
    }

    /// <summary>
    /// Restablece e inicia inmediatamente el temporizador.
    /// </summary>
    public void RestartTimer()
    {
        ResetTimer();
        StartTimer();
    }

    /// <summary>
    /// Permite añadir o restar tiempo dinámicamente en segundos.
    /// </summary>
    public void AddTime(float seconds)
    {
        currentTime += seconds;
        if (timerMode == TimerType.CountDown && currentTime < 0f)
        {
            currentTime = 0f;
        }
        UpdateUI();
        onTimeChanged?.Invoke(currentTime);
    }

    /// <summary>
    /// Actualiza los componentes de UI asignados.
    /// </summary>
    public void UpdateUI()
    {
        string formattedString = GetFormattedTimeString();

        if (tmpTextDisplay != null)
        {
            tmpTextDisplay.text = formattedString;
        }

        if (legacyTextDisplay != null)
        {
            legacyTextDisplay.text = formattedString;
        }
    }

    /// <summary>
    /// Retorna la cadena de texto formateada del tiempo actual.
    /// </summary>
    public string GetFormattedTimeString()
    {
        string formatted = "";

        switch (displayFormat)
        {
            case DisplayFormat.MinutesSeconds:
                int minutes = Mathf.FloorToInt(currentTime / 60f);
                int seconds = Mathf.FloorToInt(currentTime % 60f);
                formatted = string.Format("{0:00}:{1:00}", minutes, seconds);
                break;

            case DisplayFormat.MinutesSecondsMs:
                int minMs = Mathf.FloorToInt(currentTime / 60f);
                int secMs = Mathf.FloorToInt(currentTime % 60f);
                int fraction = Mathf.FloorToInt((currentTime * 100f) % 100f);
                formatted = string.Format("{0:00}:{1:00}.{2:00}", minMs, secMs, fraction);
                break;

            case DisplayFormat.SecondsOnly:
                formatted = Mathf.FloorToInt(currentTime).ToString();
                break;

            case DisplayFormat.SecondsWithDecimal:
                formatted = currentTime.ToString("F1");
                break;
        }

        return prefix + formatted + suffix;
    }
}
