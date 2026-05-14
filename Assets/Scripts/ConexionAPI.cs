using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; 
using System.Collections;
using System;

// molde para crear el paquete de datos del usuario
[Serializable]
public class DatosUsuario {
    // añadimos el id porque lo necesitamos para el multijugador
    public long id_usuario; 
    public string nombre_usuario;
    public string email;
    public string password;
}

// molde para que unity entienda que es una partida
[Serializable]
public class DatosPartida {
    public long id_partida;
    public long id_usuario;
    public string modo_juego;
    public string codigo_sala;
    public int jugadores_maximos;
    public string estado;
    
    // --- ESTE CAMPO ES NUEVO Y VITAL PARA SABER DE QUIEN ES EL TURNO ---
    public long turnoActualId; 
}

// script que hace de puente entre el juego y la base de datos
public class ConexionAPI : MonoBehaviour
{
    // cajas de texto donde el jugador escribe en la pantalla
    public TMP_InputField inputNombre;
    public TMP_InputField inputEmail;
    public TMP_InputField inputPassword;

    // rutas donde escucha nuestro servidor local
    private string urlRegistro = "http://localhost:8080/usuarios/registro";
    private string urlLogin = "http://localhost:8080/usuarios/login";
    
    // nuevas rutas para el multijugador
    private string urlCrearSala = "http://localhost:8080/partidas/crear";
    private string urlUnirseSala = "http://localhost:8080/partidas/unirse";

    // boton de registro que lanza la tarea en segundo plano
    public void ClickRegistro()
    {
        StartCoroutine(PostRegistro(inputNombre.text, inputEmail.text, inputPassword.text));
    }

    // tarea que envia los datos de registro al servidor
    IEnumerator PostRegistro(string nombre, string correo, string pass)
    {
        // metemos los datos en el molde y los pasamos a formato json
        DatosUsuario nuevo = new DatosUsuario { nombre_usuario = nombre, email = correo, password = pass };
        string json = JsonUtility.ToJson(nuevo);

        // preparamos la carta para enviarla por internet
        using (UnityWebRequest request = new UnityWebRequest(urlRegistro, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // esperamos a que el servidor conteste
            yield return request.SendWebRequest();

            // si el servidor nos da el visto bueno lo apuntamos
            if (request.result == UnityWebRequest.Result.Success) 
            {
                Debug.Log("registrado ahora puedes iniciar sesion");
            }
            else 
            {
                Debug.LogError("error registro " + request.error);
            }
        }
    }

    // boton de entrar que lanza la tarea de inicio de sesion
    public void ClickLogin()
    {
        StartCoroutine(PostLogin(inputNombre.text, inputPassword.text));
    }

    // tarea que comprueba si el usuario existe en la base de datos
    IEnumerator PostLogin(string nombre, string pass)
    {
        // empaquetamos el nombre y la contrasena en texto json
        DatosUsuario loginData = new DatosUsuario { nombre_usuario = nombre, password = pass };
        string json = JsonUtility.ToJson(loginData);

        // mandamos el paquete a la direccion de login
        using (UnityWebRequest request = new UnityWebRequest(urlLogin, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // pausamos el codigo hasta recibir respuesta
            yield return request.SendWebRequest();

            // si no hay errores de conexion miramos que dice el servidor
           if (request.result == UnityWebRequest.Result.Success)
            {
                // si el servidor devuelve datos es que la contrasena es correcta
                if (!string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    Debug.Log("login correcto cargando juego");

                    // guardamos toda la info del usuario en nuestra mochila global
                    DatosGlobales.usuarioLogueado = JsonUtility.FromJson<DatosUsuario>(request.downloadHandler.text);
                    
                    Debug.Log("jugador guardado " + DatosGlobales.usuarioLogueado.nombre_usuario);

                    // le abrimos la puerta y cargamos la escena del menu
                    SceneManager.LoadScene("MenuPrincipal");
                }
                else
                {
                    Debug.LogError("usuario o contrasena incorrectos");
                }
            }
        }
    }
    
    // funcion que se llama al dar a crear sala en el menu
    public void CrearNuevaSala(string codigo, int maxJugadores)
    {
        StartCoroutine(PostCrearSala(codigo, maxJugadores));
    }

    // tarea que manda los datos de la nueva sala al servidor
    IEnumerator PostCrearSala(string codigo, int maxJugadores)
    {
        // creamos el paquete con los datos de la nueva sala
        DatosPartida nueva = new DatosPartida {
            id_usuario = DatosGlobales.usuarioLogueado.id_usuario,
            modo_juego = "Multijugador",
            codigo_sala = codigo,
            jugadores_maximos = maxJugadores,
            estado = "Esperando"
        };

        string json = JsonUtility.ToJson(nueva);

        using (UnityWebRequest request = new UnityWebRequest(urlCrearSala, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("sala creada correctamente en la base de datos");
                
                // GUARDAMOS LA PARTIDA EN LA MOCHILA PARA LA JEFA DE SALA
                DatosGlobales.partidaActual = JsonUtility.FromJson<DatosPartida>(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("error al crear la sala en el servidor");
            }
        }

    }

    // funcion para unirse a la sala de un amigo
    public void UnirseASalaPrivada(string codigo)
    {
        StartCoroutine(PostUnirse(codigo));
    }

    // tarea que comprueba si la sala existe y hay hueco
    IEnumerator PostUnirse(string codigo)
    {
        // construimos la ruta con el codigo y nuestro id de usuario
        string urlFinal = urlUnirseSala + "/" + codigo + "/" + DatosGlobales.usuarioLogueado.id_usuario;

        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(urlFinal, ""))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("te has unido a la sala con exito");
                
                // GUARDAMOS LA PARTIDA EN LA MOCHILA ANTES DE CARGAR LA MESA
                DatosGlobales.partidaActual = JsonUtility.FromJson<DatosPartida>(request.downloadHandler.text);
                
                // ya puedes saltar a la escena de la mesa
                SceneManager.LoadScene("MesaJuego");
            }
            else
            {
                Debug.LogError("error al unirse puede que la sala este llena o no exista");
            }
        }
    }
    
    public void BuscarPartidaRapida() {
        StartCoroutine(PostPartidaRapida());
    }

    IEnumerator PostPartidaRapida() {
        string url = "http://localhost:8080/partidas/rapida/" + DatosGlobales.usuarioLogueado.id_usuario;

        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(url, "")) {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                Debug.Log("partida rapida encontrada o creada");
                
                // GUARDAMOS LA PARTIDA EN LA MOCHILA ANTES DE CARGAR LA MESA
                DatosGlobales.partidaActual = JsonUtility.FromJson<DatosPartida>(request.downloadHandler.text);
                
                SceneManager.LoadScene("MesaJuego");
            }
        }
    }
    
    // funcion para obtener los datos actualizados de la partida
    public void ObtenerEstadoPartida(long idPartida, Action<DatosPartida> alFinalizar)
    {
        StartCoroutine(GetEstadoPartida(idPartida, alFinalizar));
    }

    IEnumerator GetEstadoPartida(long idPartida, Action<DatosPartida> alFinalizar)
    {
        string url = "http://localhost:8080/partidas/" + idPartida;

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                DatosPartida estado = JsonUtility.FromJson<DatosPartida>(request.downloadHandler.text);
                alFinalizar?.Invoke(estado);
            }
        }
    }

    // --- ESTA ES LA FUNCIÓN QUE FALTABA ---
    // Le dice al servidor que hemos terminado nuestra jugada
    public void PasarTurnoServidor()
    {
        StartCoroutine(PostPasarTurno());
    }

    IEnumerator PostPasarTurno()
    {
        // Cogemos los datos de la mochila global
        long idPartida = DatosGlobales.partidaActual.id_partida;
        long miId = DatosGlobales.usuarioLogueado.id_usuario;
        
        // Esta es la ruta que creamos en Spring Boot
        string url = "http://localhost:8080/partidas/" + idPartida + "/pasarTurno/" + miId;

        // Mandamos una petición POST vacía para ejecutar la orden
        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(url, ""))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Turno pasado al rival correctamente");
            }
            else
            {
                Debug.LogError("Error al intentar pasar el turno: " + request.error);
            }
        }
    }
}