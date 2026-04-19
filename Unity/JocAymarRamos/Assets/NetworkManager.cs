using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using UnityEngine.SceneManagement;

// --- CLASES PARA CONVERTIR EL JSON ---
[System.Serializable]
public class UserAuthData {
    public string username;
    public string password;
}

[System.Serializable]
public class UserData {
    public string id;
    public string username;
}

[System.Serializable]
public class AuthResponse {
    public string status;
    public string message;
    public UserData data;
}

[System.Serializable]
public class GameStatsData {
    public string userId;
    public int nightsReached;
    public bool victory;
}

// --- CLASE PRINCIPAL ---
public class NetworkManager : MonoBehaviour
{
    private string baseUrl = "http://localhost:3000/api/users";

    // Guardaremos los datos del jugador actual aquí para usarlos en todo el juego
    public static string PlayerID;
    public static string PlayerUsername;

    void Awake()
    {
        // Para que el objeto no se destruya al cambiar de escena
        DontDestroyOnLoad(this.gameObject);
    }

    // --- MÉTODOS DE AUTENTICACIÓN ---
    public void Register(string user, string pass)
    {
        StartCoroutine(PostAuth(user, pass, "/register"));
    }

    public void Login(string user, string pass)
    {
        StartCoroutine(PostAuth(user, pass, "/login"));
    }

    IEnumerator PostAuth(string user, string pass, string endpoint)
    {
        UserAuthData data = new UserAuthData { username = user, password = pass };
        string json = JsonUtility.ToJson(data);

        using (UnityWebRequest request = new UnityWebRequest(baseUrl + endpoint, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log($"Connectant a {endpoint}...");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Resposta: " + request.downloadHandler.text);

                // Si es Login, guardamos los datos del usuario que devuelve Node
                if (endpoint == "/login") 
                {
                    AuthResponse response = JsonUtility.FromJson<AuthResponse>(request.downloadHandler.text);
                    if (response != null && response.data != null)
                    {
                        PlayerID = response.data.id;
                        PlayerUsername = response.data.username;
                        Debug.Log("ID guardat: " + PlayerID);
                    }
                }

                // Cambiamos a la escena del Menú
                SceneManager.LoadScene(1);
            }
            else
            {
                Debug.LogError("Error: " + request.downloadHandler.text);
            }
        }
    }

    // --- NUEVO: MÉTODOS PARA GUARDAR ESTADÍSTICAS ---
    public void EnviarEstadisticas(int noche, bool gano)
    {
        StartCoroutine(PostStats(noche, gano));
    }

    IEnumerator PostStats(int noche, bool gano)
    {
        // Verificamos que el jugador se haya logueado previamente
        if (string.IsNullOrEmpty(PlayerID))
        {
            Debug.LogWarning("No hi ha PlayerID. No es poden enviar les estadístiques.");
            yield break;
        }

        GameStatsData statsData = new GameStatsData { 
            userId = PlayerID, 
            nightsReached = noche, 
            victory = gano 
        };
        string json = JsonUtility.ToJson(statsData);

        using (UnityWebRequest request = new UnityWebRequest(baseUrl + "/save-stats", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log("Enviant estadístiques al servidor...");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Estadístiques guardades correctament a la base de dades!");
            }
            else
            {
                Debug.LogError("Error al guardar estadístiques: " + request.error);
            }
        }
    }
}