using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Si usas TextMeshPro para los inputs

public class ControladorLogin : MonoBehaviour
{
    public TMP_InputField campoUsuario;
    public TMP_InputField campoContrasena;

    public void IntentarLogin()
    {
        // Aquí iría tu lógica de validación
        Debug.Log("Validando usuario...");
        
        // Si el login es correcto:
        SceneManager.LoadScene("EscenaMenu"); 
    }
}