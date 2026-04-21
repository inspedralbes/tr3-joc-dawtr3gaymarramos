using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using SocketIOClient;

public class LobbyScreen : MonoBehaviour
{
    private Label lblP1, lblP2, lblCodi, lblStatus;
    private Button btnSortir, btnStart, btnReady;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Buscamos usando el nombre exacto que sale en tu jerarquía (con o sin #)
        // Si en el inspector el Name no tiene #, quítalo de aquí abajo
        lblP1 = root.Q<Label>("Player1");
        lblP2 = root.Q<Label>("Player2");
        lblCodi = root.Q<Label>("LblRoomCode"); 
        lblStatus = root.Q<Label>("LblStatus");
        btnSortir = root.Q<Button>("BtnBack");
        btnStart = root.Q<Button>("BtnStartGame");
        btnReady = root.Q<Button>("BtnReady");

        // 1. Mostrar código de sala
        string code = string.IsNullOrEmpty(NetworkManager.CodiSalaActual) ? "ERROR" : NetworkManager.CodiSalaActual;
        if (lblCodi != null) lblCodi.text = "CODI: " + code;

        // 2. El nombre del Host se actualizará con el evento de Sockets

        // 3. Configurar botón SORTIR
        if (btnSortir != null) {
            btnSortir.clicked += () => {
                Debug.Log("Botó sortir clicat");
                SceneManager.LoadScene("Menu");
            };
        }

        // 4. Configurar botones COMENÇAR y ESTIC LLEST
        bool soyHost = NetworkManager.esHost;
        
        if (soyHost) {
            // El Host ve Empezar (deshabilitado al inicio) y no ve Ready
            if (btnReady != null) btnReady.style.display = DisplayStyle.None;
            if (btnStart != null) {
                btnStart.SetEnabled(false); // Esperando al compañero
                btnStart.clicked += () => {
                    if (SocketHandler.socket != null) SocketHandler.socket.Emit("startGame", code);
                    NetworkManager.esMultijugador = true;
                    SceneManager.LoadScene("Joc"); 
                };
            }
        }
        else {
            // El Cliente ve Ready y no ve Empezar
            if (btnStart != null) btnStart.style.display = DisplayStyle.None;
            if (btnReady != null) {
                btnReady.style.display = DisplayStyle.Flex;
                btnReady.clicked += () => {
                    btnReady.SetEnabled(false);
                    if (lblStatus != null) lblStatus.text = "Esperant al Host...";
                    if (SocketHandler.socket != null) SocketHandler.socket.Emit("playerReady", code);
                };
            }
        }

        // 5. Lógica de Sockets (si el socket existe)
        if (SocketHandler.socket != null) {
            
            // --- NUEVO: Escuchar el inicio de la partida para el Cliente ---
            SocketHandler.socket.Off("onGameStarted");
            SocketHandler.socket.OnUnityThread("onGameStarted", (response) => {
                Debug.Log("¡El Host ha iniciado la partida!");
                NetworkManager.esMultijugador = true;
                SceneManager.LoadScene("Joc");
            });

            // --- NUEVO: Escuchar el 'Ready' del oponente (Host recibe esto) ---
            SocketHandler.socket.OnUnityThread("opponentReady", (response) => {
                Debug.Log("¡El compañero está listo!");
                if (soyHost) {
                    if (btnStart != null) btnStart.SetEnabled(true);
                    if (lblStatus != null) lblStatus.text = "Company preparat!";
                }
            });
            SocketHandler.socket.OnUnityThread("roomUpdated", (response) => {
                Debug.Log("roomUpdated event received: " + response.ToString());
                try {
                    var roomData = response.GetValue<RoomUpdatedData>();
                    Debug.Log($"Parsed data: count={roomData.playersCount}, players={roomData.players?.Length}");
                    
                    if (roomData.playersCount > 0 && roomData.players != null && roomData.players.Length > 0) {
                        if (lblP1 != null) lblP1.text = "HOST: " + roomData.players[0];
                    }
                    if (roomData.playersCount > 1 && roomData.players != null && roomData.players.Length > 1) {
                        if (lblP2 != null) lblP2.text = "COMPANY: " + roomData.players[1];
                        if (btnStart != null) btnStart.SetEnabled(true);
                    }
                } catch (System.Exception e) {
                    Debug.LogError("Error parsing roomUpdated data: " + e.Message);
                }
            });

            string username = string.IsNullOrEmpty(NetworkManager.PlayerUsername) ? "Invitado" : NetworkManager.PlayerUsername;
            var data = new { roomCode = code, username = username };
            SocketHandler.socket.Emit("joinRoom", data);
        }
    }

    void OnDisable()
    {
        if (SocketHandler.socket != null)
        {
            SocketHandler.socket.Off("onGameStarted");
            SocketHandler.socket.Off("opponentReady");
            SocketHandler.socket.Off("roomUpdated");
        }
    }

    [System.Serializable]
    class RoomUpdatedData
    {
        public int playersCount { get; set; }
        public string lastJoined { get; set; }
        public string[] players { get; set; }
    }
}