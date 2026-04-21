using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class EnemyAI : Agent
{
    [Header("Objetivos y Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    private Transform objetivoActual;
    private Rigidbody2D rb;
    private Animator anim;
    private Vector3 initialPosition;

    private Vector2 targetPos;
    private float syncTimer = 0f;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        initialPosition = transform.position;
        targetPos = transform.position;

        Debug.Log($"[EnemyAI] Inicialitzat: {gameObject.name}. Multijugador: {NetworkManager.esMultijugador}, Host: {NetworkManager.esHost}");

        if (NetworkManager.esMultijugador && !NetworkManager.esHost)
        {
            // En el cliente, no queremos que ML-Agents pida decisiones ni use físicas
            var dr = GetComponent<DecisionRequester>();
            if (dr != null) dr.enabled = false;
            if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;

            if (SocketHandler.socket != null)
            {
                SocketHandler.socket.OnUnityThread("enemyUpdated", (response) => {
                    try {
                        var data = response.GetValue<SincroEnemic>(0);
                        if (data != null && data.enemyName == gameObject.name)
                        {
                            targetPos = new Vector2(data.x, data.y);
                            // Log opcional para confirmar recepción (desactivar si hay mucho spam)
                            // Debug.Log($"[EnemyAI] {gameObject.name} recibió posición: {targetPos}");
                        }
                    } catch { }
                });
            }
        }
    }

    private Transform GetClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Transform closest = null;
        float minDistance = Mathf.Infinity;
        foreach (GameObject p in players)
        {
            float dist = Vector3.Distance(transform.position, p.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = p.transform;
            }
        }
        return closest;
    }

    public override void OnEpisodeBegin()
    {
        transform.position = initialPosition;
        if (rb != null) { rb.linearVelocity = Vector2.zero; rb.angularVelocity = 0f; }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Siempre añadir exactamente 6 observaciones para evitar errores de ML-Agents
        if (objetivoActual == null) objetivoActual = GetClosestPlayer();

        Vector2 myPos = transform.position;
        Vector2 targetPosObs = (objetivoActual != null) ? (Vector2)objetivoActual.position : myPos;
        Vector2 relative = targetPosObs - myPos;

        sensor.AddObservation(myPos.x);
        sensor.AddObservation(myPos.y);
        sensor.AddObservation(targetPosObs.x);
        sensor.AddObservation(targetPosObs.y);
        sensor.AddObservation(relative.x);
        sensor.AddObservation(relative.y);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Solo el Host o en modo Solitario se mueve por físicas
        if (NetworkManager.esMultijugador && !NetworkManager.esHost) return;

        if (objetivoActual == null) {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        int action = actions.DiscreteActions[0];
        Vector2 dir = Vector2.zero;
        switch (action)
        {
            case 1: dir = Vector2.up; break;
            case 2: dir = Vector2.down; break;
            case 3: dir = Vector2.left; break;
            case 4: dir = Vector2.right; break;
        }

        if (rb != null) rb.linearVelocity = dir * moveSpeed;
    }

    // 6. Choques y daño
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            PlayerMovement playerMov = collision.gameObject.GetComponent<PlayerMovement>();

            if (playerHealth != null && playerMov != null)
            {
                // Solo el Host tiene la autoridad para decir "He golpeado a X"
                if (!NetworkManager.esMultijugador || NetworkManager.esHost)
                {
                    playerHealth.TakeDamage(1);

                    // Si es multijugador, avisamos a la red para que el otro jugador pierda vida
                    if (NetworkManager.esMultijugador && SocketHandler.socket != null)
                    {
                        string sala = string.IsNullOrEmpty(NetworkManager.CodiSalaActual) ? "SALA" : NetworkManager.CodiSalaActual;
                        SocketHandler.socket.Emit("playerDamage", new PlayerDamageData {
                            room = sala,
                            isHost = playerMov.isHostCharacter,
                            damage = 1
                        });
                        Debug.Log($"[Host] Golpeado {playerMov.gameObject.name}. Enviando daño a la red.");
                    }
                }
            }

            AddReward(1f);
            EndEpisode();
        }
    }

    private void Update()
    {
        if (NetworkManager.esMultijugador && !NetworkManager.esHost)
        {
            // Lógica de CLIENTE: Seguir al Host
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 10f);
            
            // Animación aproximada para el cliente
            Vector2 velocity = (Vector2)transform.position - targetPos;
            if (velocity.magnitude > 0.01f && anim != null)
            {
                anim.SetFloat("Horizontal", -velocity.normalized.x);
                anim.SetFloat("Vertical", -velocity.normalized.y);
            }
            return;
        }

        // Lógica de HOST o SOLITARIO
        Transform targetMasCercano = GetClosestPlayer();
        if (targetMasCercano != null)
        {
            float dist = Vector3.Distance(transform.position, targetMasCercano.position);
            if (dist > 15f && DoorManager.ultimaPuertaCruzada != null)
                objetivoActual = DoorManager.ultimaPuertaCruzada;
            else
                objetivoActual = targetMasCercano;
        }

        // Animación de Host
        if (anim != null && rb != null && rb.linearVelocity.magnitude > 0.1f)
        {
            Vector2 v = rb.linearVelocity.normalized;
            anim.SetFloat("Horizontal", v.x);
            anim.SetFloat("Vertical", v.y);
        }

        // Emitir posición si somos el Host
        if (NetworkManager.esMultijugador && NetworkManager.esHost)
        {
            syncTimer += Time.deltaTime;
            if (syncTimer >= 0.1f && SocketHandler.socket != null)
            {
                syncTimer = 0f;
                string sala = string.IsNullOrEmpty(NetworkManager.CodiSalaActual) ? "SALA" : NetworkManager.CodiSalaActual;
                SocketHandler.socket.Emit("enemyMove", new SincroEnemic {
                    room = sala, 
                    enemyName = gameObject.name, 
                    x = transform.position.x, 
                    y = transform.position.y
                });
            }
        }
    }
}