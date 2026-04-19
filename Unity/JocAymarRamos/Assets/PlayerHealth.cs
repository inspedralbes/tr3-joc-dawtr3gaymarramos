using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración de Vida")]
    // Como acordamos que Álvaro te tumba de un golpe al estilo FNaF, puedes dejarlo en 1 o 3, 
    // pero te tumbará igual si usamos la función CaerAbatido()
    public int maxHealth = 1; 
    private int currentHealth;
    public TextMeshProUGUI textoVidas;

    [Header("Modo de Juego")]
    public bool esMultijugador = true; // Ponlo a true para poder reanimar
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
        ActualizarTexto();
    }

    void Update()
    {
        if (esMultijugador && estaAbatido && siendoRevivido)
        {
            temporizadorRevivir += Time.deltaTime;
            
            if (temporizadorRevivir >= tiempoParaRevivir)
            {
                Revivir();
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
        if (textoVidas != null)
        {
            textoVidas.text = "Vidas: " + currentHealth;
        }
    }

    void CaerAbatido()
    {
        estaAbatido = true;
        
        if (animator != null) animator.SetBool("EstaMuerto", true);
        if (scriptMovimiento != null) scriptMovimiento.enabled = false;

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
        currentHealth = maxHealth; 
        ActualizarTexto();

        // ¡ESTO QUITA LA ANIMACIÓN DE LLORAR Y TE PONE DE PIE!
        if (animator != null) animator.SetBool("EstaMuerto", false);

        if (scriptMovimiento != null) scriptMovimiento.enabled = true;

        Debug.Log("¡JUGADOR REVIVIDO!");
    }

    // --- DETECCIÓN DE COMPAÑEROS PARA REVIVIR ---
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (estaAbatido && collision.CompareTag("Player"))
        {
            siendoRevivido = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (estaAbatido && collision.CompareTag("Player"))
        {
            siendoRevivido = false;
            temporizadorRevivir = 0f; 
        }
    }
}