using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

// script que controla todos los botones y ventanas del menu inicial
public class ControladorMenu : MonoBehaviour
{
    [Header("Elementos del Perfil")]
    public TextMeshProUGUI textoNombreUsuario;
    public Slider barraXP;

    [Header("Paneles de Fondo")]
    [SerializeField] private GameObject menuPrincipal;
    [SerializeField] private GameObject panelPerfil;
    [SerializeField] private GameObject botonAbrirAjustes; 

    [Header("Contenedores de Ajustes")]
    [SerializeField] private GameObject contenidoAjustes; 
    [SerializeField] private GameObject botonReglasObjeto;
    [SerializeField] private GameObject botonCerrarAjustesObjeto; 
    [SerializeField] private GameObject tituloAjustesObjeto; 

    [Header("Paneles Emergentes")]
    public GameObject panelAjustes;
    public GameObject panelReglas;

    [Header("Menú Multijugador (Fusión)")]
    [SerializeField] private GameObject botonComenzarPartida; // boton que solo usa el que crea la sala
    [SerializeField] private GameObject panelOpcionesMultijugador;
    [SerializeField] private GameObject seccionBotones;
    [SerializeField] private GameObject seccionCodigos;
    [SerializeField] private GameObject seccionUnirse; 
    
    [SerializeField] private GameObject seccionSeleccionJugadores; 
    [SerializeField] private GameObject seccionBuscandoPartida; 

    [SerializeField] private TextMeshProUGUI textoCodigoGenerado;
    [SerializeField] private TMP_InputField campoCodigoEntrada; 

    [Header("Controles de Sonido y Extras")]
    public Slider sliderMusica;
    public Slider sliderSonido;
    public Toggle toggleVibracion;
    [Header("Conexión al Servidor")]
    public ConexionAPI conexionAPI; // Esta es la línea nueva
    
    private void Start()
    {
        // preparamos los datos el sonido y escondemos ventanas secundarias
        ConfigurarPerfil();
        ConfigurarSonidoInicial();
        ConfigurarVibracionInicial();
        
        if (panelAjustes != null) panelAjustes.SetActive(false);
        if (panelReglas != null) panelReglas.SetActive(false);
        if (panelOpcionesMultijugador != null) panelOpcionesMultijugador.SetActive(false);
    }

    private void ConfigurarPerfil()
    {
        // leemos la mochila de datos para poner el nombre real o invitado
        if (textoNombreUsuario != null)
        {
            if (DatosGlobales.usuarioLogueado != null && !string.IsNullOrEmpty(DatosGlobales.usuarioLogueado.nombre_usuario))
            {
                textoNombreUsuario.text = DatosGlobales.usuarioLogueado.nombre_usuario;
            }
            else
            {
                textoNombreUsuario.text = PlayerPrefs.GetString("NombreUsuario", "Jugador Invitado");
            }
        }
        if (barraXP != null) barraXP.value = 0.5f; 
    }

    private void ConfigurarSonidoInicial()
    {
        // recuperamos el volumen que el jugador guardo la ultima vez
        float volumenMusicaGuardado = PlayerPrefs.GetFloat("VolumenMusica", 1f);
        float volumenSonidoGuardado = PlayerPrefs.GetFloat("VolumenEfectos", 1f);
        AudioListener.volume = volumenMusicaGuardado;
        if (sliderMusica != null) sliderMusica.value = volumenMusicaGuardado;
        if (sliderSonido != null) sliderSonido.value = volumenSonidoGuardado;
    }

    private void ConfigurarVibracionInicial()
    {
        // miramos en la memoria si el jugador tenia la vibracion encendida
        int vibracionGuardada = PlayerPrefs.GetInt("VibracionActiva", 1);
        if (toggleVibracion != null) toggleVibracion.isOn = (vibracionGuardada == 1);
    }

    public void MostrarAjustes()
    {
        // enseñamos la pantalla de ajustes y escondemos lo de atras
        if (panelAjustes != null) panelAjustes.SetActive(true);
        if (contenidoAjustes != null) contenidoAjustes.SetActive(true);
        if (panelReglas != null) panelReglas.SetActive(false);
        
        if (botonReglasObjeto != null) botonReglasObjeto.SetActive(true);
        if (botonCerrarAjustesObjeto != null) botonCerrarAjustesObjeto.SetActive(true);
        if (botonAbrirAjustes != null) botonAbrirAjustes.SetActive(true);
        if (tituloAjustesObjeto != null) tituloAjustesObjeto.SetActive(true); 

        SetEstadoMenuFondo(false);
    }

    public void OcultarAjustes()
    {
    // Solo apaga el panel de ajustes, no hace absolutamente nada más
    panelAjustes.SetActive(false);
    menuPrincipal.SetActive(true);
    botonAbrirAjustes.SetActive(true);
    }

    public void ConmutarReglas()
    {
        // funciona como un interruptor para apagar o encender las reglas
        if (panelReglas == null) return;
        bool mostrarReglas = !panelReglas.activeSelf;
        panelReglas.SetActive(mostrarReglas);

        if (contenidoAjustes != null) contenidoAjustes.SetActive(!mostrarReglas);
        if (botonReglasObjeto != null) botonReglasObjeto.SetActive(!mostrarReglas);
        if (botonCerrarAjustesObjeto != null) botonCerrarAjustesObjeto.SetActive(!mostrarReglas);
        if (botonAbrirAjustes != null) botonAbrirAjustes.SetActive(!mostrarReglas); 
        if (tituloAjustesObjeto != null) tituloAjustesObjeto.SetActive(!mostrarReglas); 
    }

    private void SetEstadoMenuFondo(bool estado)
    {
        // apaga los botones de fondo para que no se puedan pulsar por error
        if (menuPrincipal != null) menuPrincipal.SetActive(estado);
        if (panelPerfil != null) panelPerfil.SetActive(estado);
        if (botonAbrirAjustes != null)
        {
            if (estado == true) botonAbrirAjustes.SetActive(true);
            else if (!panelReglas.activeSelf) botonAbrirAjustes.SetActive(false);
        }
    }

    private void ApagarTodasLasSeccionesMultijugador()
    {
        // limpia el panel multijugador para evitar que se amontonen las pantallas
        if (seccionBotones != null) seccionBotones.SetActive(false);
        if (seccionCodigos != null) seccionCodigos.SetActive(false);
        if (seccionUnirse != null) seccionUnirse.SetActive(false);
        if (seccionSeleccionJugadores != null) seccionSeleccionJugadores.SetActive(false);
        if (seccionBuscandoPartida != null) seccionBuscandoPartida.SetActive(false);
    }

    public void AbrirMenuMultijugador()
    {
        // abre la caja gris grande y enciende los botones principales
        if (panelOpcionesMultijugador != null) panelOpcionesMultijugador.SetActive(true);
        ApagarTodasLasSeccionesMultijugador();
        if (seccionBotones != null) seccionBotones.SetActive(true);
    }

    public void MostrarPantallaCodigos()
    {
        // nos lleva a la pantalla para invitar a un amigo y crea el codigo
        ApagarTodasLasSeccionesMultijugador();
        if (seccionCodigos != null) seccionCodigos.SetActive(true);
        if (botonComenzarPartida != null) botonComenzarPartida.SetActive(true);
        GenerarCodigoSala();
    }

    public void MostrarPantallaUnirse()
    {
        // muestra el pergamino azul donde escribimos para entrar
        ApagarTodasLasSeccionesMultijugador();
        if (seccionUnirse != null) seccionUnirse.SetActive(true);
    }

    public void MostrarSeleccionJugadores()
    {
        // muestra la pantalla para elegir si somos dos tres o cuatro
        ApagarTodasLasSeccionesMultijugador();
        if (seccionSeleccionJugadores != null) seccionSeleccionJugadores.SetActive(true);
    }

    public void IniciarBusquedaAleatoria(int cantidadJugadores)
    {
        // recoge el numero de personas e inicia la espera ficticia de conexion
        ApagarTodasLasSeccionesMultijugador();
        if (seccionBuscandoPartida != null) seccionBuscandoPartida.SetActive(true);

        Debug.Log($"Buscando partida rápida para {cantidadJugadores} jugadores...");
        
        // simula buscar oponente durante un rato antes de cargar la mesa
        Invoke("CargarMesaJuego", 2.5f); 
    }

    public void ConfirmarUnionSala()
    {
        // recoge el codigo escrito por el jugador y lo pone en mayusculas
        if (campoCodigoEntrada != null && !string.IsNullOrEmpty(campoCodigoEntrada.text))
        {
            string codigo = campoCodigoEntrada.text.ToUpper();
            Debug.Log("Intentando unirse a sala: " + codigo);
            if (conexionAPI != null) 
            {
                conexionAPI.UnirseASalaPrivada(codigo);
            }
        }
    }

    public void CerrarMenuMultijugador()
    {
        // cierra toda la caja gris del multijugador de golpe
        if (panelOpcionesMultijugador != null) panelOpcionesMultijugador.SetActive(false);
    }

    private void GenerarCodigoSala()
    {
        // elige cuatro letras o numeros al azar para crear una sala unica
        const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string codigoNuevo = "";
        for (int i = 0; i < 4; i++) 
        {
            int indice = Random.Range(0, caracteres.Length);
            codigoNuevo += caracteres[indice];
        }
        if (textoCodigoGenerado != null) textoCodigoGenerado.text = codigoNuevo;
        if (conexionAPI != null) 
        {
            conexionAPI.CrearNuevaSala(codigoNuevo, 2); // Le decimos que es para 4 jugadores
        }
    }

    public void CambiarVolumenMusica(float nuevoVolumen)
    {
        // ajusta la musica general del juego y lo guarda en la memoria
        AudioListener.volume = nuevoVolumen; 
        PlayerPrefs.SetFloat("VolumenMusica", nuevoVolumen);
        PlayerPrefs.Save();
    }

    public void CambiarVolumenEfectos(float nuevoVolumen)
    {
        // guarda a que volumen queremos los efectos de sonido
        PlayerPrefs.SetFloat("VolumenEfectos", nuevoVolumen);
        PlayerPrefs.Save();
    }

    public void CambiarVibracion(bool activar)
    {
        // enciende o apaga el motor de vibracion del movil y lo recuerda
        if (activar)
        {
            PlayerPrefs.SetInt("VibracionActiva", 1);
        }
        else PlayerPrefs.SetInt("VibracionActiva", 0);
        
        PlayerPrefs.Save();
    }

    public void CargarMesaJuego()
    {
        // funcion que salta a la escena de las cartas
        SceneManager.LoadScene("MesaJuego");
    }

    public void CambiarIdioma(int indiceIdioma)
    {
        // guarda en que idioma queremos leer los textos
        PlayerPrefs.SetInt("IdiomaSeleccionado", indiceIdioma);
        PlayerPrefs.Save();
    }

    public void CerrarSesion()
    {
        // borra al jugador actual de la mochila y nos devuelve al principio
        DatosGlobales.usuarioLogueado = null;
        PlayerPrefs.DeleteKey("NombreUsuario");
        PlayerPrefs.Save();
        SceneManager.LoadScene("PantallaLogin"); 
    }
    public void ComenzarPartida()
    {
        // este boton simplemente nos lleva a la mesa cuando estemos todos listos
        Debug.Log("El juego ha comenzado");
        SceneManager.LoadScene("MesaJuego"); 
    }
    // esta funcion es la que activara el boton de partida rapida
    public void ClickPartidaRapida()
    {
        Debug.Log("buscando partida rapida...");
        
        // le pedimos a la api que busque o cree una sala automatica
        if (conexionAPI != null)
        {
            conexionAPI.BuscarPartidaRapida();
        }
        else
        {
            Debug.LogError("no hay referencia a conexionapi en el controlador del menu");
        }
    }
    
}