using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using SocketIOClient;

public class LobbyScreen : MonoBehaviour
{
    private Label lblP1, lblP2, lblCodi;
    private Button btnSortir, btnStart;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Buscamos usando el nombre exacto que sale en tu jerarquía (con o sin #)
        // Si en el inspector el Name no tiene #, quítalo de aquí abajo
        lblP1 = root.Q<Label>("Player1");
        lblP2 = root.Q<Label>("Player2");
        lblCodi = root.Q<Label>("LblRoomCode"); 
        btnSortir = root.Q<Button>("BtnBack");
        btnStart = root.Q<Button>("BtnStartGame");

        // 1. Mostrar código de sala
        string code = PlayerPrefs.GetString("CodiSalaActual", "ERROR");
        if (lblCodi != null) lblCodi.text = "CODI: " + code;

        // 2. Mostrar nombre del Host
        if (lblP1 != null) lblP1.text = "HOST: " + PlayerPrefs.GetString("Username", "Jugador");

        // 3. Configurar botón SORTIR
        if (btnSortir != null) {
            btnSortir.clicked += () => {
                Debug.Log("Botó sortir clicat");
                SceneManager.LoadScene("Menu");
            };
        }

        // 4. Configurar botón COMENÇAR
        if (btnStart != null) {
            btnStart.clicked += () => {
                if (SocketHandler.socket != null)
                    SocketHandler.socket.Emit("startGame", code);
                SceneManager.LoadScene("Joc"); // Salto directo para pruebas
            };
        }

        // 5. Lógica de Sockets (si el socket existe)
        if (SocketHandler.socket != null) {
            var data = new { roomCode = code, username = PlayerPrefs.GetString("Username") };
            SocketHandler.socket.Emit("joinRoom", data);

            SocketHandler.socket.OnUnityThread("roomUpdated", (response) => {
                if (lblP2 != null) lblP2.text = "RIVAL: Conectat";
                if (btnStart != null) btnStart.SetEnabled(true);
            });
        }
    }
}