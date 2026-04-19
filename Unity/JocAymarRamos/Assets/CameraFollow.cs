using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform target; 
    public float smoothSpeed = 0.125f; 
    public Vector3 offset = new Vector3(0, 0, -10); 

    [Header("Límites del Mapa")]
    public Vector2 minLimits; // Límite inferior e izquierdo (X, Y)
    public Vector2 maxLimits; // Límite superior y derecho (X, Y)

    void LateUpdate()
    {
        if (target != null)
        {
            // 1. Calculamos la posición deseada original
            Vector3 desiredPosition = target.position + offset;

            // 2. Restringimos (Clamp) los valores X e Y para que no pasen de los límites
            float clampedX = Mathf.Clamp(desiredPosition.x, minLimits.x, maxLimits.x);
            float clampedY = Mathf.Clamp(desiredPosition.y, minLimits.y, maxLimits.y);

            // 3. Creamos la nueva posición limitada (respetando la profundidad Z)
            Vector3 limitedPosition = new Vector3(clampedX, clampedY, desiredPosition.z);

            // 4. Aplicamos el suavizado hacia la posición limitada
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, limitedPosition, smoothSpeed);
            
            // 5. Movemos la cámara
            transform.position = smoothedPosition;
        }
    }

    // FUNCIÓN FINAL: Cambia los límites y teletransporta la cámara al instante
    public void CambiarZona(Vector2 nuevoMin, Vector2 nuevoMax, Vector3 nuevaPosicion)
    {
        minLimits = nuevoMin;
        maxLimits = nuevoMax;
        
        // Esto hace que la cámara dé el "salto" instantáneo
        transform.position = new Vector3(nuevaPosicion.x, nuevaPosicion.y, transform.position.z);
    }
}