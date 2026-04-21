using UnityEngine;

public class DoorManager : MonoBehaviour
{
    [Header("Destino")]
    public Transform puntoDeDestino; 

    [Header("Configuración de Cámara (Solo para el Player)")]
    public Vector2 minEnDestino; 
    public Vector2 maxEnDestino;

    // La variable estática que sirve de "chivato" para la IA
    public static Transform ultimaPuertaCruzada;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // CASO 1: Si entra el NIÑO
        if (other.CompareTag("Player"))
        {
            // Solo teletransportamos y cambiamos cámara si es el jugador LOCAL
            PlayerMovement mov = other.GetComponent<PlayerMovement>();
            bool esLocal = (mov != null) ? mov.esLocal : true; // Si no hay script, asumimos local (modo solitario)

            if (esLocal)
            {
                // ¡NUEVO! Le decimos a la IA que esta es la puerta que debe seguir
                ultimaPuertaCruzada = this.transform;

                // 1. Teletransportar al niño
                other.transform.position = puntoDeDestino.position;

                // 2. Actualizar cámara al instante
                CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
                if (cam != null)
                {
                    cam.CambiarZona(minEnDestino, maxEnDestino, puntoDeDestino.position);
                }
                
                Debug.Log("El jugador local ha cruzado la puerta.");
            }
            else
            {
                // Si es un jugador remoto, no tocamos la cámara. 
                // El teletransporte físico lo hará su propio cliente y se sincronizará por posición,
                // pero lo movemos también aquí para evitar "ghosting" a través de paredes.
                other.transform.position = puntoDeDestino.position;
                Debug.Log("Un jugador remoto ha cruzado la puerta.");
            }
        }

        // CASO 2: Si entra un ENEMIGO (Pol, Toni o Álvaro)
        if (other.CompareTag("Enemy"))
        {
            // 1. Teletransportar al enemigo
            // Si usas NavMesh, necesitamos usar Warp para que no se pierda la IA
            var agent = other.GetComponent<UnityEngine.AI.NavMeshAgent>();
            
            if (agent != null)
            {
                agent.Warp(puntoDeDestino.position);
            }
            else
            {
                other.transform.position = puntoDeDestino.position;
            }

            Debug.Log(other.name + " ha cruzado la puerta por su cuenta.");
        }
    }
}