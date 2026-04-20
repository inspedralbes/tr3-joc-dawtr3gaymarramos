using UnityEngine;
using UnityEngine.UIElements;

public class AuthControllerUI : MonoBehaviour
{
    private TextField _userField;
    private TextField _passField;
    private Button _registerButton;
    private Button _loginButton; // Añadimos el botón de login

    public NetworkManager networkManager;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Buscamos los elementos del UXML
        _userField = root.Q<TextField>("UsernameField");
        _passField = root.Q<TextField>("PasswordField");
        _registerButton = root.Q<Button>("RegisterBtn");
        _loginButton = root.Q<Button>("LoginBtn"); // Coincide con el nombre en el UXML

        // Evento para REGISTRAR
        _registerButton.clicked += () => {
            if (ValidarInputs()) {
                networkManager.Register(_userField.value, _passField.value);
            }
        };

        // Evento para ENTRAR (Login)
        _loginButton.clicked += () => {
            if (ValidarInputs()) {
                Debug.Log("Intentant entrar amb: " + _userField.value);
                
                // === AQUÍ GUARDAMOS EL NOMBRE ===
                NetworkManager.PlayerUsername = _userField.value;
                
                // Llamamos a la red
                networkManager.Login(_userField.value, _passField.value); 
            }
        };
    }

    // Una validación rápida para no perder tiempo con peticiones vacías
    bool ValidarInputs() {
        if (string.IsNullOrEmpty(_userField.value) || string.IsNullOrEmpty(_passField.value)) {
            Debug.LogWarning("Has d'omplir tots els camps");
            return false;
        }
        return true;
    }
}