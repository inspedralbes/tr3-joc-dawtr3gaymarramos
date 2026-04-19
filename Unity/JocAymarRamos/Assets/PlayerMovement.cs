using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public bool esLocal = true; // Si es true, obedece al teclado. Si es false, es el compañero.
    public bool isHostCharacter = true; // Para diferenciar los IDs de red (Host vs Join)
    public float moveSpeed = 4f; // Velocidad ajustada
    public Rigidbody2D rb;
    public Animator animator;
    
    // Aquí le decimos al código que existe una linterna
    public GameObject linterna; 

    Vector2 movement;
    private Vector2 targetPos;
    private float syncTimer = 0f;

    [System.Serializable]
    class Vector2Data { public float x; public float y; }

    [System.Serializable]
    class MoveEmitData { public string room; public Vector2Data pos; public bool isHost; }

    [System.Serializable]
    class PlayerMovedResponse { public string id; public Vector2Data pos; public string username; public bool isHost; }

    void Start()
    {
        if (!esLocal && SocketHandler.socket != null)
        {
            targetPos = transform.position;
            SocketHandler.socket.OnUnityThread("playerMoved", (response) => {
                Debug.Log("<<< RECIBIDO evento 'playerMoved' del servidor");
                try {
                    string json = response.ToString();
                    Debug.Log("JSON recibido: " + json);
                    
                    var data = response.GetValue<PlayerMovedResponse>();
                    if (data.isHost == this.isHostCharacter) 
                    {
                        targetPos = new Vector2(data.pos.x, data.pos.y);
                    }
                } catch (System.Exception e) {
                    Debug.LogError("Error al procesar playerMoved: " + e.Message);
                }
            });
        }
    }

    void Update()
    {
        if (!esLocal)
        {
            // Interpolar la posición del compañero para que se mueva suave
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 10f);
            
            // Animar compañero
            Vector2 dir = targetPos - (Vector2)transform.position;
            if (dir.magnitude > 0.05f)
            {
                animator.SetFloat("Horizontal", dir.normalized.x);
                animator.SetFloat("Vertical", dir.normalized.y);
                animator.SetFloat("Speed", dir.sqrMagnitude);

                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                linterna.transform.rotation = Quaternion.Euler(0, 0, angle - 90f); 
            }
            else
            {
                animator.SetFloat("Speed", 0f);
            }
            return; 
        }

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (movement != Vector2.zero)
        {
            animator.SetFloat("Horizontal", movement.x);
            animator.SetFloat("Vertical", movement.y);
            animator.SetFloat("Speed", movement.sqrMagnitude);

            // ¡La magia de la rotación!
            float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;
            // Le restamos 90 grados para que el cono apunte hacia adelante y no de lado
            linterna.transform.rotation = Quaternion.Euler(0, 0, angle - 90f); 
        }
        else
        {
            animator.SetFloat("Speed", 0f);
        }
    }

    void FixedUpdate()
    {
        if (!esLocal) return; // El movimiento físico solo lo hace el local
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);

        // Emitir nuestra posición al servidor 10 veces por segundo
        syncTimer += Time.fixedDeltaTime;
        if (syncTimer >= 0.1f) // Cada 0.1 segundos (10 Hz)
        {
            syncTimer = 0f;
            if (SocketHandler.socket != null)
            {
                string miSala = PlayerPrefs.GetString("CodiSalaActual", "SENSE_SALA");
                var data = new MoveEmitData { 
                    room = miSala, 
                    pos = new Vector2Data { x = rb.position.x, y = rb.position.y },
                    isHost = this.isHostCharacter
                };
                Debug.Log(">>> ENVIANDO move a la sala: " + miSala);
                SocketHandler.socket.Emit("move", data);
            }
        }
    }
}