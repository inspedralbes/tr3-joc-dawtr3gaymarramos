using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

[System.Serializable]
public class EnemyMoveData {
    public string room;
    public string enemyName;
    public Vector2 pos;
}

public class EnemyAI : Agent
{
    [Header("Objetivos y Movimiento")]
    [SerializeField] private Transform playerTarget;
    [SerializeField] private float moveSpeed = 5f;

    // ¡NUEVO! El objetivo real que va a perseguir en cada momento
    private Transform objetivoActual;

    private Rigidbody2D rb;
    private Animator anim;
    private Vector3 initialPosition; // Para resetear al inicio del episodio

    // --- NUEVAS VARIABLES DE RED ---
    private Vector2 targetPos;
    private float syncTimer = 0f;

    // 1. Inicialización
    public override void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        initialPosition = transform.position; 
        objetivoActual = GetClosestPlayer(); 
        targetPos = transform.position;

        // Si somos el cliente, nos suscribimos a las actualizaciones del Host
        if (NetworkManager.esMultijugador && !NetworkManager.esHost)
        {
            SocketHandler.socket.OnUnityThread("enemyUpdated", (response) => {
                var data = response.GetValue<EnemyMoveData>(0);
                if (data.enemyName == gameObject.name)
                {
                    targetPos = data.pos;
                }
            });
        }
    }

    private Transform GetClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Transform closest = null;
        float minDistance = Mathf.Infinity;
        foreach(GameObject p in players)
        {
            float dist = Vector3.Distance(transform.position, p.transform.position);
            if(dist < minDistance)
            {
                minDistance = dist;
                closest = p.transform;
            }
        }
        return closest;
    }

    // 2. Comienza un nuevo "Intento" (Episodio)
    public override void OnEpisodeBegin()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        // Resetear posición y frenar
        transform.position = initialPosition;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    // 3. ¿Qué "ve" el enemigo? (Observaciones)
    public override void CollectObservations(VectorSensor sensor)
    {
        // Por seguridad, si el objetivoActual se queda vacío, miramos al player más cercano
        if (objetivoActual == null) objetivoActual = GetClosestPlayer();
        
        if (objetivoActual == null) 
        {
            // Rellenar con 0s para que la IA no se rompa si el jugador tarda 1 frame en cargar
            sensor.AddObservation(transform.localPosition.x);
            sensor.AddObservation(transform.localPosition.y);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            return;
        }

        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.y);
        
        // ¡ACTUALIZADO! Ahora observa al 'objetivoActual' (Tú o la puerta)
        sensor.AddObservation(objetivoActual.localPosition.x);
        sensor.AddObservation(objetivoActual.localPosition.y);

        Vector3 relative = objetivoActual.localPosition - transform.localPosition;
        sensor.AddObservation(relative.x);
        sensor.AddObservation(relative.y);
    }

    // 4. Moverse y Animarse
    public override void OnActionReceived(ActionBuffers actions)
    {
        // SI ES MULTIJUGADOR Y NO SOMOS EL HOST, NO HACEMOS NADA (el Host nos mueve)
        if (NetworkManager.esMultijugador && !NetworkManager.esHost) return;

        if (objetivoActual == null) return;

        int action = actions.DiscreteActions[0];
        Vector2 dir = Vector2.zero;

        switch (action)
        {
            case 1: dir = Vector2.up; break;
            case 2: dir = Vector2.down; break;
            case 3: dir = Vector2.left; break;
            case 4: dir = Vector2.right; break;
        }

        rb.linearVelocity = dir * moveSpeed;

        // --- NUEVAS RECOMPENSAS PARA APRENDER RÁPIDO ---
        float distanciaActual = Vector3.Distance(transform.position, objetivoActual.position);
        
        // Recompensa por estar cerca (cuanto más cerca, más puntos)
        if (distanciaActual < 5f) AddReward(0.001f); 
        
        // Pequeño castigo por cada segundo que pasa (para que se den prisa)
        AddReward(-0.0005f);
    }

    // 5. Control manual
    public override void Heuristic(in ActionBuffers actionsOut) { }

    // 6. Choques y daño
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
            }

            AddReward(1f);
            EndEpisode();
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Pared")) 
        {
            // Castigo más fuerte por chocar (para que aprendan a girar)
            AddReward(-0.01f); 
        }
    }

    // 7. LA MAGIA DE LA ANIMACIÓN Y EL CEREBRO
    private void Update()
    {
        if (NetworkManager.esMultijugador && !NetworkManager.esHost)
        {
            // Lógica de CLIENTE: Seguir la posición del Host suavemente
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 10f);
            
            // Animación basada en el movimiento de red
            Vector2 velocity = (Vector2)transform.position - (Vector2)targetPos; // Aproximación
            if (velocity.magnitude > 0.01f)
            {
                anim.SetFloat("Horizontal", -velocity.normalized.x);
                anim.SetFloat("Vertical", -velocity.normalized.y);
            }
            return;
        }

        // --- Lógica de HOST o SOLITARIO ---
        
        // --- ¡NUEVO! CEREBRO DE PERSECUCIÓN DINÁMICO ---
        Transform targetMasCercano = GetClosestPlayer();

        if (targetMasCercano != null)
        {
            // ¿A cuánta distancia estás?
            float distancia = Vector3.Distance(transform.position, targetMasCercano.position);

            // Si estás a más de 15 metros, asume que te has teletransportado
            if (distancia > 15f && DoorManager.ultimaPuertaCruzada != null)
            {
                objetivoActual = DoorManager.ultimaPuertaCruzada; // Va hacia la puerta
            }
            else
            {
                objetivoActual = targetMasCercano; // Va directo a por ti
            }
        }

        // --- TU CÓDIGO DE ANIMACIÓN INTACTO ---
        if (anim != null && rb != null)
        {
            if (rb.linearVelocity.magnitude > 0.1f)
            {
                Vector2 direccion = rb.linearVelocity.normalized;
                anim.SetFloat("Horizontal", direccion.x);
                anim.SetFloat("Vertical", direccion.y);
            }
        }

        // --- ENVIAR POSICIÓN POR RED (Solo si somos el Host) ---
        if (NetworkManager.esMultijugador && NetworkManager.esHost)
        {
            syncTimer += Time.deltaTime;
            if (syncTimer >= 0.1f) // 10 veces por segundo
            {
                syncTimer = 0f;
                string miSala = string.IsNullOrEmpty(NetworkManager.CodiSalaActual) ? "SENSE_SALA" : NetworkManager.CodiSalaActual;
                SocketHandler.socket.Emit("enemyMove", new EnemyMoveData { 
                    room = miSala, 
                    enemyName = gameObject.name, 
                    pos = transform.position 
                });
            }
        }
    }
}