using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Ajustes de Partida")]
    public bool esMultijugador; // Automático según desde dónde se inicia la escena
    public float tiempoParaGanar = 360f; // 6 minutos reales

    [Header("Spawners de Jugadores")]
    public GameObject playerPrefab;
    public Transform spawnP1;
    public Transform spawnP2;

    public TextMeshProUGUI relojText;
    public UnityEngine.Rendering.Universal.Light2D luzGlobal; // Referencia a la luz 2D de URP
    public TextMeshProUGUI victoriaExtraText; // Texto de "Has guanyat!"

    private float tiempoSobrevivido = 0f;
    private bool partidaActiva = true;

    void Awake() 
    { 
        if (Instance == null) Instance = this; 
        
        // Cargar automáticamente si es multijugador (1 = Sala, 0 = Solitario)
        esMultijugador = NetworkManager.esMultijugador;
    }

    void Start()
    {
        if (playerPrefab != null)
        {
            if (esMultijugador)
            {
                // Instanciar a P1 (Host) y a P2 (Cliente)
                GameObject p1 = Instantiate(playerPrefab, spawnP1 != null ? spawnP1.position : Vector3.zero, Quaternion.identity);
                GameObject p2 = Instantiate(playerPrefab, spawnP2 != null ? spawnP2.position : new Vector3(2, 0, 0), Quaternion.identity);
                
                // Ignorar colisiones físicas entre jugadores para evitar que se empujen al revivir
                Collider2D col1 = p1.GetComponent<Collider2D>();
                Collider2D col2 = p2.GetComponent<Collider2D>();
                if (col1 != null && col2 != null) Physics2D.IgnoreCollision(col1, col2);

                PlayerMovement mov1 = p1.GetComponent<PlayerMovement>();
                PlayerMovement mov2 = p2.GetComponent<PlayerMovement>();

                bool soyHost = NetworkManager.esHost;

                if (soyHost)
                {
                    if (mov1 != null) mov1.Initialize(true, true);  // P1 es local y es el Host
                    if (mov2 != null) mov2.Initialize(false, false); // P2 es remoto y es el Invitado
                    
                    CameraFollow cam = FindObjectOfType<CameraFollow>();
                    if (cam != null) cam.target = p1.transform;
                }
                else
                {
                    if (mov1 != null) mov1.Initialize(false, true); // P1 es remoto y es el Host
                    if (mov2 != null) mov2.Initialize(true, false); // P2 es local y es el Invitado
                    
                    CameraFollow cam = FindObjectOfType<CameraFollow>();
                    if (cam != null) cam.target = p2.transform;
                }
            }
            else
            {
                // Modo Individual: instanciar solo 1 jugador
                GameObject p1 = Instantiate(playerPrefab, spawnP1 != null ? spawnP1.position : Vector3.zero, Quaternion.identity);
                if (p1.GetComponent<PlayerMovement>() != null) p1.GetComponent<PlayerMovement>().esLocal = true;

                // Asignar cámara a P1
                CameraFollow cam = FindObjectOfType<CameraFollow>();
                if (cam != null) cam.target = p1.transform;
            }
        }
    }

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

        // 1. Encender la luz global
        if (luzGlobal != null) luzGlobal.intensity = 1f;

        // 2. Mostrar "Has guanyat!"
        if (victoriaExtraText != null) 
        {
            victoriaExtraText.text = "HAS GUANYAT!";
            victoriaExtraText.gameObject.SetActive(true);
        }

        // 3. Detener a todos los animatrónicos
        EnemyAI[] enemigos = FindObjectsOfType<EnemyAI>();
        foreach (var e in enemigos) 
        {
            e.enabled = false; // Desactivamos el script de IA
            Rigidbody2D rb = e.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero; // Frenazo en seco
        }
        
        // Llamamos al UIManager para que muestre la victoria (el panel de las 06:00 AM)
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