using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Ajustes de Partida")]
    public bool esMultijugador = false; // Ponlo a true si pruebas con dos jugadores
    public float tiempoParaGanar = 360f; // 6 minutos reales

    [Header("UI Reloj")]
    public TextMeshProUGUI relojText;

    private float tiempoSobrevivido = 0f;
    private bool partidaActiva = true;

    void Awake() { if (Instance == null) Instance = this; }

    void Update()
    {
        if (!partidaActiva) return;

        tiempoSobrevivido += Time.deltaTime;
        ActualizarReloj();

        if (tiempoSobrevivido >= tiempoParaGanar) GanarPartida();
    }

    private void ActualizarReloj()
    {
        int horasPasadas = Mathf.FloorToInt(tiempoSobrevivido / 60f);
        int horaActual = 12 + horasPasadas;
        if (horaActual > 12) horaActual -= 12;
        relojText.text = horaActual.ToString("00") + ":00 AM";
    }

    public void ComprobarEstadoPartida()
    {
        if (!partidaActiva) return;

        if (!esMultijugador)
        {
            // Individual: Si caes, pierdes.
            PerderPartida("Abatut en Solitari");
        }
        else
        {
            // Multijugador: Solo pierdes si TODOS caen
            PlayerHealth[] jugadores = FindObjectsOfType<PlayerHealth>();
            int abatidos = 0;
            foreach (var p in jugadores) { if (p.estaAbatido) abatidos++; }

            if (abatidos >= jugadores.Length && jugadores.Length > 0)
            {
                PerderPartida("Equip Completament Abatut");
            }
        }
    }

    public void GanarPartida()
    {
        partidaActiva = false;
        relojText.text = "06:00 AM";
        
        // Llamamos al UIManager para que muestre la victoria
        if (UIManager.Instance != null) UIManager.Instance.MostrarVictoria(); 
        
        // Enviamos datos a la BD
        NetworkManager net = FindObjectOfType<NetworkManager>();
        if (net != null) net.EnviarEstadisticas(1, true);
        
        StartCoroutine(RegresarAlMenu(7f));
    }

    public void PerderPartida(string razon)
    {
        partidaActiva = false;
        
        // Llamamos al UIManager para que muestre la derrota con nuestros 3 textos
        if (UIManager.Instance != null) UIManager.Instance.MostrarDerrota(relojText.text, razon); 
        
        // Enviamos datos a la BD
        NetworkManager net = FindObjectOfType<NetworkManager>();
        if (net != null) net.EnviarEstadisticas(1, false);
        
        StartCoroutine(RegresarAlMenu(5f));
    }

    IEnumerator RegresarAlMenu(float segundos)
    {
        yield return new WaitForSeconds(segundos);
        SceneManager.LoadScene("Menu"); 
    }
}