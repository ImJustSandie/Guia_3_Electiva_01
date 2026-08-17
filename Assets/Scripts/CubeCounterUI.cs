using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Colocar en un objeto con un TMP_Text (o Text clásico) para mostrar el contador de cubos apilados.
// Asignar en el inspector la "zona" (StackZone) y el texto donde se mostrará "Cubos: X/4".
public class CubeCounterUI : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Zona de apilado que se está contando.")]
    [SerializeField] private StackZone zona;

    [Tooltip("Texto TextMeshPro (TMP_Text) donde se mostrará el contador.")]
    [SerializeField] private TMP_Text contadorText;

    [Tooltip("Texto UI tradicional (opcional) donde se mostrará el contador.")]
    [SerializeField] private Text legacyText;

    [Tooltip("Texto previo al contador (ej: 'Cubos: ').")]
    [SerializeField] private string prefix = "Cubos: ";

    [Tooltip("Formato de cada elemento del contador (ej: '0' para números enteros).")]
    [SerializeField] private string formatoNumero = "0";

    public bool EstaCompleto => zona != null && zona.IsFull;

    private void Update()
    {
        if (zona == null) return;

        string texto = prefix
            + zona.PlacedCount.ToString(formatoNumero)
            + "/"
            + zona.MaxCount.ToString(formatoNumero);

        if (contadorText != null)
            contadorText.text = texto;

        if (legacyText != null)
            legacyText.text = texto;
    }
}