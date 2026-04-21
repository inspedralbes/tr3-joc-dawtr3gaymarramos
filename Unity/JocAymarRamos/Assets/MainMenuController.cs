using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    void OnEnable()
    {
        Debug.Log("Script del Menú activat!");
        // 1. Agafem l'arrel de la teva interfície
        var root = GetComponent<UIDocument>().rootVisualElement;

        // 2. Busquem els botons exactament pels noms que tens al UXML
        Button btnNouJoc = root.Q<Button>("BtnNewGame");
        Button btnCrearSala = root.Q<Button>("BtnCreateRoom");
        Button btnUnirSala = root.Q<Button>("BtnJoinRoom");
        Button btnSortir = root.Q<Button>("BtnQuit");

        // 3. Programem les funcions per a cada botó
        
        // Entrar a l'escena "Joc"
        if (btnNouJoc != null) {
            btnNouJoc.clicked += () => {
                Debug.Log("Intentant carregar escena individual...");
                NetworkManager.esMultijugador = false;
                SceneManager.LoadScene(2); 
            };
        }

        // Multiplayer (de moment només log per provar)
        if (btnCrearSala != null) {
            btnCrearSala.clicked += () => Debug.Log("Has polsat: Crear Sala");
        }

        if (btnUnirSala != null) {
            btnUnirSala.clicked += () => Debug.Log("Has polsat: Unir-se a Sala");
        }



        // Sortir del joc
        if (btnSortir != null) {
            btnSortir.clicked += () => Application.Quit();
        }
        
    }
}