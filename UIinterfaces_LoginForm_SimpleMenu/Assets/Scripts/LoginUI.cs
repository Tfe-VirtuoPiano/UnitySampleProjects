using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{

    [Header("Champs de saisie")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    [Header("UI")]
    public Button loginButton;
    public TextMeshProUGUI errorText;

    private AuthManager authManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        authManager = FindFirstObjectByType<AuthManager>();
        loginButton.onClick.AddListener(OnLoginClicked);
    }

    private void OnLoginClicked()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ShowError("Veuillez remplir tous les champs.");
            return;
        }

        authManager.LoginFromUI(email, password, OnLoginSuccess, ShowError);
    }

    private void OnLoginSuccess()
    {
        Debug.Log("Connexion réussie !");
        errorText.text = "";
    }

    private void ShowError(string message)
    {
        errorText.text = message;
    }
    // Update is called once per frame

}
