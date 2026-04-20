using UnityEngine;
using SocketIOClient;
using System;

public class SocketHandler : MonoBehaviour
{
    public static SocketIOUnity socket;
    // PROD: IP del servidor VPS (Port 80 via Nginx)
    public string serverUrl = "http://204.168.205.93";
    // DEV: public string serverUrl = "http://localhost:3000";

    void Awake()
    {
        // Evitamos que se destruya al cambiar de escena
        if (socket == null)
        {
            Application.runInBackground = true; // <-- Evita que el juego se congele si cambias de ventana
            DontDestroyOnLoad(this.gameObject);
            var uri = new Uri(serverUrl);
            socket = new SocketIOUnity(uri);

            // --- DEBUG GLOBAL (Cambiado a OnAny para compatibilidad) ---
            socket.OnAny((eventName, response) => {
                Debug.Log($"[SOCKET_RAWDAT] Evento: {eventName} | Data: {response.ToString()}");
            });

            socket.Connect();
            Debug.Log("🔌 Intentant connectar al Socket...");
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}