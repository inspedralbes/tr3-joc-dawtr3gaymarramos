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
            
            Debug.Log("El jugador ha cruzado la puerta.");
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