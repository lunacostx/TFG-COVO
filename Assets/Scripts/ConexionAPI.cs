using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; 
using System.Collections;
using System;

[Serializable]
public class DatosUsuario {
    public long id_usuario; 
    public string nombre_usuario;
    public string email;
    public string password;
}

[Serializable]
public class DatosPartida {
    public long id_partida;
    public long id_usuario;
    public string modo_juego;
    public string codigo_sala;
    public int jugadores_maximos;
    public string estado;
    public long turnoActualId; 
}

public class ConexionAPI : MonoBehaviour
{
    // --- PANELES DE LA INTERFAZ ---
    [Header("Control de Pantallas")]
    public GameObject panelRegistroUsuario;
    public GameObject panelInicioSesion;

    // --- CAJAS DE TEXTO PARA EL REGISTRO ---
    [Header("Panel Registro")]
    public TMP_InputField inputNombreRegistro;
    public TMP_InputField inputEmailRegistro;
    public TMP_InputField inputPasswordRegistro;
    public TextMeshProUGUI textoErrorRegistro;

    // --- CAJAS DE TEXTO PARA EL INICIO DE SESION ---
    [Header("Panel Inicio de Sesion")]
    public TMP_InputField inputNombreLogin;
    public TMP_InputField inputPasswordLogin;

    // --- CONFIGURACIÓN DE SERVIDOR (OPTIMIZADA) ---
    // ¡ATENCIÓN! Pon aquí el enlace exacto que te ha dado Render (sin la barra / al final)
    private string urlBase = "https://covo-api.onrender.com";

    private string urlRegistro => urlBase + "/usuarios/registro";
    private string urlLogin => urlBase + "/usuarios/login";
    private string urlCrearSala => urlBase + "/partidas/crear";
    private string urlUnirseSala => urlBase + "/partidas/unirse";

    // --------------------------------------------------------
    // ZONA DE REGISTRO
    // --------------------------------------------------------
    void Start()
    {
        // --- ESTAS LÍNEAS FUERZAN EL ESTADO INICIAL ---
        if (panelInicioSesion != null) panelInicioSesion.SetActive(true);
        if (panelRegistroUsuario != null) panelRegistroUsuario.SetActive(false);
        
        // ... resto de tu código del Start ...
    }
    public void ClickRegistro()
    {
        string nombre = inputNombreRegistro.text;
        string correo = inputEmailRegistro.text;
        string pass = inputPasswordRegistro.text;

        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(pass))
        {
            Debug.LogWarning("¡Alto ahí! Faltan campos por rellenar.");
            return; 
        }

        if (pass.Length < 4)
        {
            Debug.LogWarning("La contraseña es muy corta. Debe tener al menos 4 caracteres.");
            return;
        }

        if (!correo.Contains("@"))
        {
            Debug.LogWarning("Por favor, introduce un correo válido.");
            return;
        }

        StartCoroutine(PostRegistro(nombre, correo, pass));
    }

    IEnumerator PostRegistro(string nombre, string correo, string pass)
    {
        DatosUsuario nuevo = new DatosUsuario { nombre_usuario = nombre, email = correo, password = pass };
        string json = JsonUtility.ToJson(nuevo);

        using (UnityWebRequest peticion = new UnityWebRequest(urlRegistro, "POST"))
        {
            byte[] cuerpoBruto = System.Text.Encoding.UTF8.GetBytes(json);
            peticion.uploadHandler = new UploadHandlerRaw(cuerpoBruto);
            peticion.downloadHandler = new DownloadHandlerBuffer();
            peticion.SetRequestHeader("Content-Type", "application/json");

            yield return peticion.SendWebRequest();

            if (peticion.result == UnityWebRequest.Result.Success) 
            {
                Debug.Log("Respuesta REAL del servidor al Registrar: " + peticion.downloadHandler.text);
                Debug.Log("registrado ahora puedes iniciar sesion");
                if (textoErrorRegistro != null) textoErrorRegistro.text = "";
                
                if (panelRegistroUsuario != null && panelInicioSesion != null)
                {
                    panelRegistroUsuario.SetActive(false); 
                    panelInicioSesion.SetActive(true);     
                }
            }
            else 
            {
                Debug.LogError("error registro " + peticion.error);
                if (textoErrorRegistro != null) 
                {
                    textoErrorRegistro.text = "Error: El nombre o correo ya está en uso.";
                }
            }
        }
    }

    // --------------------------------------------------------
    // ZONA DE LOGIN
    // --------------------------------------------------------

    public void ClickLogin()
    {
        string nombre = inputNombreLogin.text;
        string pass = inputPasswordLogin.text;

        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(pass))
        {
            Debug.LogWarning("Por favor, introduce tu nombre y contraseña.");
            return;
        }

        StartCoroutine(PostLogin(nombre, pass));
    }

    IEnumerator PostLogin(string nombre, string pass)
    {
        DatosUsuario loginData = new DatosUsuario { nombre_usuario = nombre, password = pass };
        string json = JsonUtility.ToJson(loginData);

        using (UnityWebRequest peticion = new UnityWebRequest(urlLogin, "POST"))
        {
            byte[] cuerpoBruto = System.Text.Encoding.UTF8.GetBytes(json);
            peticion.uploadHandler = new UploadHandlerRaw(cuerpoBruto);
            peticion.downloadHandler = new DownloadHandlerBuffer();
            peticion.SetRequestHeader("Content-Type", "application/json");

            yield return peticion.SendWebRequest();

           if (peticion.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Respuesta REAL del servidor al Login: " + peticion.downloadHandler.text);
                if (!string.IsNullOrEmpty(peticion.downloadHandler.text))
                {
                    Debug.Log("login correcto cargando juego");
                    DatosGlobales.usuarioLogueado = JsonUtility.FromJson<DatosUsuario>(peticion.downloadHandler.text);
                    SceneManager.LoadScene("MenuPrincipal");
                }
                else
                {
                    Debug.LogError("usuario o contrasena incorrectos");
                }
            }
        }
    }
    
    // --------------------------------------------------------
    // ZONA MULTIJUGADOR
    // --------------------------------------------------------

    public void CrearNuevaSala(string codigo, int maxJugadores)
    {
        StartCoroutine(PostCrearSala(codigo, maxJugadores));
    }

    IEnumerator PostCrearSala(string codigo, int maxJugadores)
    {
        DatosPartida nueva = new DatosPartida {
            id_usuario = DatosGlobales.usuarioLogueado.id_usuario,
            modo_juego = "Multijugador",
            codigo_sala = codigo,
            jugadores_maximos = maxJugadores,
            estado = "Esperando"
        };

        string json = JsonUtility.ToJson(nueva);

        using (UnityWebRequest peticion = new UnityWebRequest(urlCrearSala, "POST"))
        {
            byte[] cuerpoBruto = System.Text.Encoding.UTF8.GetBytes(json);
            peticion.uploadHandler = new UploadHandlerRaw(cuerpoBruto);
            peticion.downloadHandler = new DownloadHandlerBuffer();
            peticion.SetRequestHeader("Content-Type", "application/json");

            yield return peticion.SendWebRequest();

            if (peticion.result == UnityWebRequest.Result.Success)
            {
                DatosGlobales.partidaActual = JsonUtility.FromJson<DatosPartida>(peticion.downloadHandler.text);
            }
        }
    }

    public void UnirseASalaPrivada(string codigo)
    {
        StartCoroutine(PostUnirse(codigo));
    }

    IEnumerator PostUnirse(string codigo)
    {
        string urlFinal = urlUnirseSala + "/" + codigo + "/" + DatosGlobales.usuarioLogueado.id_usuario;

        using (UnityWebRequest peticion = UnityWebRequest.PostWwwForm(urlFinal, ""))
        {
            yield return peticion.SendWebRequest();

            if (peticion.result == UnityWebRequest.Result.Success)
            {
                DatosGlobales.partidaActual = JsonUtility.FromJson<DatosPartida>(peticion.downloadHandler.text);
                SceneManager.LoadScene("MesaJuego");
            }
        }
    }
    
    public void BuscarPartidaRapida() {
        StartCoroutine(PostPartidaRapida());
    }

    IEnumerator PostPartidaRapida() {
        string enlaceRapida = urlBase + "/partidas/rapida/" + DatosGlobales.usuarioLogueado.id_usuario;

        using (UnityWebRequest peticion = UnityWebRequest.PostWwwForm(enlaceRapida, "")) {
            yield return peticion.SendWebRequest();

            if (peticion.result == UnityWebRequest.Result.Success) {
                DatosGlobales.partidaActual = JsonUtility.FromJson<DatosPartida>(peticion.downloadHandler.text);
                SceneManager.LoadScene("MesaJuego");
            }
        }
    }
    
    public void ObtenerEstadoPartida(long idPartida, Action<DatosPartida> alFinalizar)
    {
        StartCoroutine(GetEstadoPartida(idPartida, alFinalizar));
    }

    IEnumerator GetEstadoPartida(long idPartida, Action<DatosPartida> alFinalizar)
    {
        string enlaceEstado = urlBase + "/partidas/" + idPartida;

        using (UnityWebRequest peticion = UnityWebRequest.Get(enlaceEstado))
        {
            yield return peticion.SendWebRequest();

            if (peticion.result == UnityWebRequest.Result.Success)
            {
                DatosPartida estado = JsonUtility.FromJson<DatosPartida>(peticion.downloadHandler.text);
                alFinalizar?.Invoke(estado);
            }
        }
    }

    public void PasarTurnoServidor()
    {
        StartCoroutine(PostPasarTurno());
    }

    IEnumerator PostPasarTurno()
    {
        long idPartida = DatosGlobales.partidaActual.id_partida;
        long miId = DatosGlobales.usuarioLogueado.id_usuario;
        string enlaceTurno = urlBase + "/partidas/" + idPartida + "/pasarTurno/" + miId;

        using (UnityWebRequest peticion = UnityWebRequest.PostWwwForm(enlaceTurno, ""))
        {
            yield return peticion.SendWebRequest();
        }
    }
}