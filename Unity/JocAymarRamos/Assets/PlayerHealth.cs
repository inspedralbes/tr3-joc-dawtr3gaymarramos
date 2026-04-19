using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración de Vida")]
    // Como acordamos que Álvaro te tumba de un golpe al estilo FNaF, puedes dejarlo en 1 o 3, 
    // pero te tumbará igual si usamos la función CaerAbatido()
    public int maxHealth = 3; 
    private int currentHealth;
    public TextMeshProUGUI textoVidas;

    [Header("Modo de Juego")]
    public bool estaAbatido = false;   // Para saber si está tirado llorando

    [Header("Mecánica cooperativa de Revivir")]
    public float tiempoParaRevivir = 3f; // 3 segundos para revivir
    private float temporizadorRevivir = 0f;
    private bool siendoRevivido = false; 

    [Header("Componentes")]
    private Animator animator;
    
    // IMPORTANTE: Aquí pon el nombre de tu script de movimiento real.
    // He puesto MonoBehaviour por defecto, pero cámbialo si tu script se llama PlayerController o PlayerMovement
    public MonoBehaviour scriptMovimiento; 

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        
        // Buscar el HUD automáticamente si no está asignado
        if (textoVidas == null)
        {
            // Busca un objeto en la escena que contenga el texto de vidas (por defecto "Vidas" o "TextVidas")
            // Reemplaza "VidasText" por el nombre exacto de tu objeto en la jerarquía si es distinto.
            GameObject hud = GameObject.Find("TextVidas") ?? GameObject.Find("Vidas") ?? GameObject.Find("VidasText");
            if (hud != null)
            {
                textoVidas = hud.GetComponent<TextMeshProUGUI>();
            }
        }

        ActualizarTexto();
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.esMultijugador && estaAbatido)
        {
            // Comprobar por distancia si hay un compañero vivo cerca (así evitamos problemas de físicas)
            bool compañeroCerca = false;
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (var p in players)
            {
                if (p != this.gameObject) // No contarnos a nosotros mismos
                {
                    PlayerHealth hp = p.GetComponent<PlayerHealth>();
                    if (hp != null && !hp.estaAbatido)
                    {
                        if (Vector3.Distance(transform.position, p.transform.position) < 1.5f)
                        {
                            compañeroCerca = true;
                            break;
                        }
                    }
                }
            }

            siendoRevivido = compañeroCerca;

            if (siendoRevivido)
            {
                temporizadorRevivir += Time.deltaTime;
                if (temporizadorRevivir >= tiempoParaRevivir)
                {
                    Revivir();
                }
            }
            else
            {
                temporizadorRevivir = 0f; // Si se separa, el contador vuelve a cero
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (estaAbatido) return; // Si ya está llorando en el suelo, no le hacen más daño

        currentHealth -= damage;
        ActualizarTexto();

        if (currentHealth <= 0)
        {
            CaerAbatido();
        }
    }

    void ActualizarTexto()
    {
        // Bloqueo importante: Solo el jugador local puede escribir en el HUD de su propia pantalla
        PlayerMovement mov = GetComponent<PlayerMovement>();
        if (mov != null && !mov.esLocal) return; 

        if (textoVidas != null)
        {
            textoVidas.text = "Vidas: " + currentHealth;
        }
    }

    void CaerAbatido()
    {
        estaAbatido = true;
        
        if (animator != null) animator.SetBool("EstaMuerto", true);
        
        // Bloquear movimiento automáticamente buscando el script
        PlayerMovement mov = GetComponent<PlayerMovement>();
        if (mov != null) mov.enabled = false;

        // ESTO ES LO IMPORTANTE: Avisamos al GameManager de que hemos caído
        if (GameManager.Instance != null) 
        {
            GameManager.Instance.ComprobarEstadoPartida();
        }
    }
    
    void Revivir()
    {
        estaAbatido = false;
        siendoRevivido = false;
        temporizadorRevivir = 0f;
        currentHealth = 1; // Se levanta con 1 sola vida
        ActualizarTexto();

        // ¡ESTO QUITA LA ANIMACIÓN DE LLORAR Y TE PONE DE PIE!
        if (animator != null) animator.SetBool("EstaMuerto", false);

        // Desbloquear movimiento
        PlayerMovement mov = GetComponent<PlayerMovement>();
        if (mov != null) mov.enabled = true;

        Debug.Log("¡JUGADOR REVIVIDO!");
    }

    // --- NOTA: La detección por Triggers/Colisiones ha sido sustituida por el chequeo de distancia en Update() ---
}