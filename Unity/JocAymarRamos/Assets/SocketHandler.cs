using UnityEngine;
using SocketIOClient; // La librería que acabas de bajar
using System;
using Newtonsoft.Json; // Unity suele traerlo, si no, usa JsonUtility

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
            socket.Connect();
            Debug.Log("🔌 Intentant connectar al Socket...");
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}