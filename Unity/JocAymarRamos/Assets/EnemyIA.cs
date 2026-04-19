using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class EnemyAI : Agent
{
    [Header("Objetivos y Movimiento")]
    [SerializeField] private Transform playerTarget;
    [SerializeField] private float moveSpeed = 5f;

    // ¡NUEVO! El objetivo real que va a perseguir en cada momento
    private Transform objetivoActual;

    private Rigidbody2D rb;
    private Animator anim;

    // 1. Inicialización
    public override void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        objetivoActual = playerTarget; // Por defecto, va a por ti
    }

    // 2. Comienza un nuevo "Intento" (Episodio)
    public override void OnEpisodeBegin()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    // 3. ¿Qué "ve" el enemigo? (Observaciones)
    public override void CollectObservations(VectorSensor sensor)
    {
        // Por seguridad, si el objetivoActual se queda vacío, miramos al player
        if (objetivoActual == null) objetivoActual = playerTarget;
        if (objetivoActual == null) return;

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
        AddReward(-0.001f);
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
        // IMPORTANTE: Asegúrate de que en Unity tus muros y mesas tienen el Tag "Pared" (o cámbialo aquí)
        if (collision.transform.CompareTag("Pared")) 
        {
            // Les quitamos una pizca de puntos repetidamente para que les "duela" quedarse ahí
            AddReward(-0.005f); 
        }
    }

    // 7. LA MAGIA DE LA ANIMACIÓN Y EL CEREBRO
    private void Update()
    {
        // --- ¡NUEVO! CEREBRO DE PERSECUCIÓN ---
        if (playerTarget != null)
        {
            // ¿A cuánta distancia estás?
            float distancia = Vector3.Distance(transform.position, playerTarget.position);

            // Si estás a más de 15 metros, asume que te has teletransportado
            if (distancia > 15f && DoorManager.ultimaPuertaCruzada != null)
            {
                objetivoActual = DoorManager.ultimaPuertaCruzada; // Va hacia la puerta
            }
            else
            {
                objetivoActual = playerTarget; // Va directo a por ti
            }
        }

        // --- TU CÓDIGO DE ANIMACIÓN INTACTO ---
        if (anim != null && rb != null)
        {
            if (rb.linearVelocity.magnitude > 0.1f)
            {
                // .normalized convierte velocidades como (5, 0) o (-4, 3) 
                // en valores limpios entre -1 y 1 exactos para el Blend Tree
                Vector2 direccion = rb.linearVelocity.normalized;

                anim.SetFloat("Horizontal", direccion.x);
                anim.SetFloat("Vertical", direccion.y);
            }
        }
    }
}