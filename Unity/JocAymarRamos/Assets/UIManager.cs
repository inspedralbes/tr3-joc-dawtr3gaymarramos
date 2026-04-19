using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Paneles de Resultado")]
    public GameObject panelVictoria;    // Panel "06:00 AM"
    public GameObject panelDerrota;     // Panel "GAME OVER / STATS"

    [Header("Textos de Estadísticas (Dentro de Derrota)")]
    public TextMeshProUGUI textoNombreUsuario;
    public TextMeshProUGUI textoHoraFinal;
    public TextMeshProUGUI textoEstadoFinal;

    void Awake()
    {
        if (Instance == null) Instance = this;
        
        if(panelVictoria) panelVictoria.SetActive(false);
        if(panelDerrota) panelDerrota.SetActive(false);
    }

    public void MostrarVictoria()
    {
        if(panelVictoria) panelVictoria.SetActive(true);
    }

    public void MostrarDerrota(string horaMuerte, string razon)
    {
        if(panelDerrota) panelDerrota.SetActive(true);
        
        if (textoNombreUsuario != null) textoNombreUsuario.text = "USUARI: " + NetworkManager.PlayerUsername;
        if (textoHoraFinal != null) textoHoraFinal.text = "HORA: " + horaMuerte;
        if (textoEstadoFinal != null) textoEstadoFinal.text = "ESTAT: " + razon;
    }
}