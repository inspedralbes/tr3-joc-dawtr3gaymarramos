using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
    private Label lblCodigo;
    private VisualElement listaJugadores;
    private Button btnComencar;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        lblCodigo = root.Q<Label>("LblCodigoSala");
        listaJugadores = root.Q<VisualElement>("ListaJugadores");
        btnComencar = root.Q<Button>("BtnStart");

        // Recuperamos el código que guardamos en el LobbyManager
        string codi = PlayerPrefs.GetString("CurrentRoom", "0000");
        lblCodigo.text = "CODI SALA: " + codi;

        // Aquí es donde conectarías el Socket.io para escuchar quién entra
    }

    public void ActualizarLista(List<string> nombres)
    {
        listaJugadores.Clear();
        foreach (string nom in nombres)
        {
            var nuevoLabel = new Label(nom);
            nuevoLabel.style.color = Color.white;
            listaJugadores.Add(nuevoLabel);
        }
        
        // Si hay 2 o más, habilitamos el botón de empezar (solo para el Host)
        if (nombres.Count >= 2) btnComencar.SetEnabled(true);
    }
}