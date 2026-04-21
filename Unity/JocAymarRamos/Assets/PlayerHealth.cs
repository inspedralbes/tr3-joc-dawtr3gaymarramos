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
    
    public MonoBehaviour scriptMovimiento; 
    private Rigidbody2D rb;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (textoVidas == null)
        {

            GameObject hud = GameObject.Find("TextVides") ?? GameObject.Find("Vides") ?? GameObject.Find("TextVidas") ?? GameObject.Find("Vidas");
            if (hud != null)
            {
                textoVidas = hud.GetComponent<TextMeshProUGUI>();
            }
        }

        ActualizarTexto();

        // --- NUEVA LÓGICA DE RED ---
        if (GameManager.Instance != null && GameManager.Instance.esMultijugador)
        {
            PlayerMovement mov = GetComponent<PlayerMovement>();
            if (mov != null && !mov.esLocal)
            {
                // Si este objeto representa al otro jugador, escuchamos sus cambios de estado
                SocketHandler.socket.OnUnityThread("onPlayerDowned", (response) => {
                    var data = response.GetValue<PlayerStateData>(0);
                    if (data.isHost == mov.isHostCharacter) SyncCaerAbatido();
                });

                SocketHandler.socket.OnUnityThread("onPlayerRevived", (response) => {
                    var data = response.GetValue<PlayerStateData>(0);
                    if (data.isHost == mov.isHostCharacter) SyncRevivir();
                });
            }

            // --- NUEVO: Sincronización de Daño (Todos escuchan por si les pegan) ---
            SocketHandler.socket.OnUnityThread("onPlayerDamaged", (response) => {
                try {
                    var data = response.GetValue<PlayerDamageData>(0);
                    if (data != null && data.isHost == mov.isHostCharacter && mov.esLocal)
                    {
                        // Solo aplico el daño si el mensaje es para MÍ y yo soy el jugador LOCAL en este PC
                        ApplyLocalDamage(data.damage);
                    }
                } catch { }
            });
        }
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
        // En multijugador, el daño lo suele gestionar el Host al detectar colisión con el enemigo.
        // Si jugamos en solitario, se aplica directamente.
        if (GameManager.Instance != null && GameManager.Instance.esMultijugador)
        {
            // En multijugador, esta función solo se llama desde OnCollision del Enemigo (en el Host)
            // o desde el evento de red.
            ApplyLocalDamage(damage);
        }
        else
        {
            ApplyLocalDamage(damage);
        }
    }

    private void ApplyLocalDamage(int damage)
    {
        if (estaAbatido) return;

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
            textoVidas.text = "Vides: " + currentHealth;
        }
    }

    void CaerAbatido()
    {
        if (estaAbatido) return;
        estaAbatido = true;
        
        if (animator != null) animator.SetBool("EstaMuerto", true);
        
        PlayerMovement mov = GetComponent<PlayerMovement>();
        if (mov != null) mov.enabled = false;

        if (rb != null) {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }

        // Emitir a la red si somos el jugador local
        if (mov != null && mov.esLocal && SocketHandler.socket != null)
        {
            string miSala = string.IsNullOrEmpty(NetworkManager.CodiSalaActual) ? "SENSE_SALA" : NetworkManager.CodiSalaActual;
            SocketHandler.socket.Emit("playerDowned", new PlayerStateData { room = miSala, isHost = mov.isHostCharacter });
        }

        if (GameManager.Instance != null) 
        {
            GameManager.Instance.ComprobarEstadoPartida();
        }
    }

    // Función para sincronizar sin re-emitir
    private void SyncCaerAbatido()
    {
        estaAbatido = true;
        if (animator != null) animator.SetBool("EstaMuerto", true);
        PlayerMovement mov = GetComponent<PlayerMovement>();
        if (mov != null) mov.enabled = false;
        
        if (rb != null) {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }

        if (GameManager.Instance != null) GameManager.Instance.ComprobarEstadoPartida();
    }
    
    void Revivir()
    {
        if (!estaAbatido) return;
        estaAbatido = false;
        siendoRevivido = false;
        temporizadorRevivir = 0f;
        currentHealth = 1; 
        ActualizarTexto();

        if (animator != null) animator.SetBool("EstaMuerto", false);

        PlayerMovement mov = GetComponent<PlayerMovement>();
        if (mov != null) mov.enabled = true;

        if (rb != null) {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        // Emitir a la red si somos el jugador local
        if (mov != null && mov.esLocal && SocketHandler.socket != null)
        {
            string miSala = string.IsNullOrEmpty(NetworkManager.CodiSalaActual) ? "SENSE_SALA" : NetworkManager.CodiSalaActual;
            SocketHandler.socket.Emit("playerRevived", new PlayerStateData { room = miSala, isHost = mov.isHostCharacter });
        }

        Debug.Log("¡JUGADOR REVIVIDO!");
    }

    private void SyncRevivir()
    {
        estaAbatido = false;
        siendoRevivido = false;
        temporizadorRevivir = 0f;
        currentHealth = 1;
        if (animator != null) animator.SetBool("EstaMuerto", false);
        PlayerMovement mov = GetComponent<PlayerMovement>();
        if (mov != null) mov.enabled = true;

        if (rb != null) {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    // --- NOTA: La detección por Triggers/Colisiones ha sido sustituida por el chequeo de distancia en Update() ---
}