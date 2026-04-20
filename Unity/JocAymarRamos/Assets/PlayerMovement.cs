using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Vector2Data 
{ 
    public float x { get; set; } 
    public float y { get; set; } 
}

[System.Serializable]
public class PlayerMovedResponse 
{ 
    public string id { get; set; } 
    public Vector2Data pos { get; set; } 
    public string username { get; set; } 
    public bool isHost { get; set; } 
}

public class PlayerMovement : MonoBehaviour
{
    public bool esLocal = true;
    public bool isHostCharacter = true;
    public float moveSpeed = 4f;
    public Rigidbody2D rb;
    public Animator animator;
    public GameObject linterna; 

    Vector2 movement;
    private Vector2 targetPos;
    private float syncTimer = 0f;

    public void Initialize(bool local, bool isHost)
    {
        this.esLocal = local;
        this.isHostCharacter = isHost;

        if (!esLocal && SocketHandler.socket != null)
        {
            targetPos = transform.position;
            
            // Usamos GetValue<T>(0) porque los datos vienen dentro de un array [ { ... } ]
            SocketHandler.socket.OnUnityThread("playerMoved", (response) => {
                try {
                    var data = response.GetValue<PlayerMovedResponse>(0);
                    if (data == null || data.pos == null) return;

                    // Filtramos para no movernos a nosotros mismos
                    // Si el mensaje dice isHost=true y yo soy el Host Remoto (represento al host), actualizo.
                    // Si el mensaje dice isHost=false y yo soy el Invitado Remoto, actualizo.
                    if (data.isHost == this.isHostCharacter) 
                    {
                        targetPos = new Vector2(data.pos.x, data.pos.y);
                    }

                } catch (System.Exception e) {
                    Debug.LogError("Error en playerMoved: " + e.Message);
                }
            });
            
            Debug.Log($"[RED] Jugador {(isHost ? "Host" : "Invitado")} remoto escuchando.");
        }
    }

    void Start() { }

    void Update()
    {
        if (!esLocal)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 10f);
            
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

            float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;
            linterna.transform.rotation = Quaternion.Euler(0, 0, angle - 90f); 
        }
        else
        {
            animator.SetFloat("Speed", 0f);
        }
    }

    void FixedUpdate()
    {
        if (!esLocal) return;
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);

        syncTimer += Time.fixedDeltaTime;
        if (syncTimer >= 0.1f)
        {
            syncTimer = 0f;
            if (SocketHandler.socket != null)
            {
                string miSala = string.IsNullOrEmpty(NetworkManager.CodiSalaActual) ? "SENSE_SALA" : NetworkManager.CodiSalaActual;
                
                var data = new { 
                    room = miSala, 
                    pos = new { x = rb.position.x, y = rb.position.y },
                    isHost = this.isHostCharacter
                };

                SocketHandler.socket.Emit("move", data);
            }
        }
    }
}