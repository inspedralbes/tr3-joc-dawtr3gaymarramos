using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    public string baseUrl = "http://localhost:3000/api/games";
    
    private VisualElement root;

    void OnEnable()
    {
        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null) {
            Debug.LogError("No s'ha trobat UIDocument en aquest objecte!");
            return;
        }

        root = uiDoc.rootVisualElement;

        // Buscamos los elementos con nombres seguros
        SetupButton("BtnCreateRoom", ClickCrear);
        SetupButton("BtnJoinRoom", ClickUnirse);
    }

    // Función auxiliar para evitar el NullReferenceException
    void SetupButton(string name, System.Action action)
    {
        var btn = root.Q<Button>(name);
        if (btn != null) {
            btn.clicked += action;
            Debug.Log("Botó vinculat: " + name);
        } else {
            Debug.LogWarning("ALERTA: No s'ha trobat el botó '" + name + "' al UI Builder. Revisa el nom!");
        }
    }

    public void ClickCrear()
    {
        string userId = PlayerPrefs.GetString("UserId", "65f1a2b3c4d5e6f7a8b9c0d1"); 
        Debug.Log("Botó CREAR polsat. ID: " + userId);
        StartCoroutine(EnviarPeticion("create", "{\"hostId\":\"" + userId + "\"}"));
    }

    public void ClickUnirse()
    {
        var txtField = root.Q<TextField>("TxtRoomCode");
        string codigo = (txtField != null) ? txtField.value.ToUpper() : "";
        string userId = PlayerPrefs.GetString("UserId", "65f1a2b3c4d5e6f7a8b9c0d1");
        
        Debug.Log("Botó UNIR-SE polsat. Codi: " + codigo);
        StartCoroutine(EnviarPeticion("join", "{\"roomCode\":\"" + codigo + "\", \"userId\":\"" + userId + "\"}"));
    }

    IEnumerator EnviarPeticion(string endpoint, string json)
{
    using (UnityWebRequest request = new UnityWebRequest(baseUrl + "/" + endpoint, "POST"))
    {
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // 1. Guardamos la respuesta del servidor (el JSON)
            string respuesta = request.downloadHandler.text;
            Debug.Log("Servidor diu: " + respuesta);

            // 2. Extraemos el código de sala del JSON
            // Buscamos donde pone "roomCode":" y pillamos los 6 caracteres siguientes
            if (respuesta.Contains("roomCode"))
            {
                int inicio = respuesta.IndexOf("roomCode") + 11; 
                string codi = respuesta.Substring(inicio, 6);
                
                // GUARDAMOS EL CÓDIGO EN LA MEMORIA
                PlayerPrefs.SetString("CodiSalaActual", codi);
                PlayerPrefs.Save();
                Debug.Log("Codi guardat per a la Lobby: " + codi);
            }

            // 3. Saltamos a la escena de la Lobby
            SceneManager.LoadScene("Lobby"); 
        }
        else
        {
            Debug.LogError("Error: " + request.error);
        }
    }
}
}