using UnityEngine;
using SocketIOClient; // La librería que acabas de bajar
using System;
using Newtonsoft.Json; // Unity suele traerlo, si no, usa JsonUtility

public class SocketHandler : MonoBehaviour
{
    public static SocketIOUnity socket;
    public string serverUrl = "http://localhost:3000";

    void Awake()
    {
        // Evitamos que se destruya al cambiar de escena
        if (socket == null)
        {
            var uri = new Uri(serverUrl);
            socket = new SocketIOUnity(uri);
            socket.Connect();
            Debug.Log("🔌 Intentant connectar al Socket...");
        }
    }
}