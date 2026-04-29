using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; // <--- ¡NUEVA LIBRERÍA OBLIGATORIA!
using System.Collections;
using System;

[Serializable]
public class DatosUsuario {
    public string nombre_usuario;
    public string email;
    public string password;
}

public class ConexionAPI : MonoBehaviour
{
    public TMP_InputField inputNombre;
    public TMP_InputField inputEmail;
    public TMP_InputField inputPassword;

    private string urlRegistro = "http://localhost:8080/usuarios/registro";
    private string urlLogin = "http://localhost:8080/usuarios/login";

    // --- REGISTRO ---
    public void ClickRegistro()
    {
        StartCoroutine(PostRegistro(inputNombre.text, inputEmail.text, inputPassword.text));
    }

    IEnumerator PostRegistro(string nombre, string correo, string pass)
    {
        DatosUsuario nuevo = new DatosUsuario { nombre_usuario = nombre, email = correo, password = pass };
        string json = JsonUtility.ToJson(nuevo);

        using (UnityWebRequest request = new UnityWebRequest(urlRegistro, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) 
            {
                Debug.Log("¡Registrado! Ahora puedes iniciar sesión.");
                // Opcional: Podrías borrar los campos aquí inputNombre.text = "";
            }
            else 
            {
                Debug.LogError("Error Registro: " + request.error);
            }
        }
    }

    // --- LOGIN ---
    public void ClickLogin()
    {
        StartCoroutine(PostLogin(inputNombre.text, inputPassword.text));
    }

    IEnumerator PostLogin(string nombre, string pass)
    {
        DatosUsuario loginData = new DatosUsuario { nombre_usuario = nombre, password = pass };
        string json = JsonUtility.ToJson(loginData);

        using (UnityWebRequest request = new UnityWebRequest(urlLogin, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

           if (request.result == UnityWebRequest.Result.Success)
            {
                if (!string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    Debug.Log("LOGIN CORRECTO. Cargando juego...");

                    // --- NUEVO: GUARDAR DATOS EN LA MOCHILA ---
                    // Convertimos el texto JSON que devuelve el servidor en un objeto y lo guardamos
                    DatosGlobales.usuarioLogueado = JsonUtility.FromJson<DatosUsuario>(request.downloadHandler.text);
                    
                    // Solo por seguridad, imprimimos el nombre en consola
                    Debug.Log("Jugador guardado: " + DatosGlobales.usuarioLogueado.nombre_usuario);
                    // ------------------------------------------

                    SceneManager.LoadScene("MesaJuego");
                }
                else
                {
                    Debug.LogError("Usuario o contraseña incorrectos.");
                }
            }
        }
    }
}