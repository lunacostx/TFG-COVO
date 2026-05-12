using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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

    [Header("Paneles Emergentes")]
    public GameObject panelAjustes;
    public GameObject panelReglas;

    [Header("Controles de Sonido y Extras")]
    public Slider sliderMusica;
    public Slider sliderSonido;
    public Toggle toggleVibracion; // <--- NUEVA VARIABLE

    private void Start()
    {
        ConfigurarPerfil();
        ConfigurarSonidoInicial();
        ConfigurarVibracionInicial(); // <--- NUEVA LLAMADA
        
        if (panelAjustes != null) panelAjustes.SetActive(false);
        if (panelReglas != null) panelReglas.SetActive(false);
    }

    private void ConfigurarPerfil()
    {
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
        float volumenMusicaGuardado = PlayerPrefs.GetFloat("VolumenMusica", 1f);
        float volumenSonidoGuardado = PlayerPrefs.GetFloat("VolumenEfectos", 1f);
        AudioListener.volume = volumenMusicaGuardado;
        if (sliderMusica != null) sliderMusica.value = volumenMusicaGuardado;
        if (sliderSonido != null) sliderSonido.value = volumenSonidoGuardado;
    }

    // --- NUEVO: MÉTODO PARA CARGAR LA VIBRACIÓN AL INICIO ---
    private void ConfigurarVibracionInicial()
    {
        // 1 = Activado, 0 = Desactivado. Por defecto lo dejamos en 1 (encendido).
        int vibracionGuardada = PlayerPrefs.GetInt("VibracionActiva", 1);
        if (toggleVibracion != null) 
        {
            toggleVibracion.isOn = (vibracionGuardada == 1);
        }
    }

    // --- LÓGICA DE PANELES ---

    public void MostrarAjustes()
    {
        if (panelAjustes != null) panelAjustes.SetActive(true);
        if (contenidoAjustes != null) contenidoAjustes.SetActive(true);
        if (panelReglas != null) panelReglas.SetActive(false);
        SetEstadoMenuFondo(false);
    }

    public void OcultarAjustes()
    {
        if (panelAjustes != null) panelAjustes.SetActive(false);
        SetEstadoMenuFondo(true);
    }

    public void ConmutarReglas()
    {
        if (panelReglas == null) return;
        bool mostrar = !panelReglas.activeSelf;
        panelReglas.SetActive(mostrar);
        if (contenidoAjustes != null) contenidoAjustes.SetActive(!mostrar);
    }

    private void SetEstadoMenuFondo(bool estado)
    {
        if (menuPrincipal != null) menuPrincipal.SetActive(estado);
        if (panelPerfil != null) panelPerfil.SetActive(estado);
        if (botonAbrirAjustes != null) botonAbrirAjustes.SetActive(estado);
    }

    // --- MÉTODOS PARA LOS SLIDERS Y TOGGLES ---

    public void CambiarVolumenMusica(float nuevoVolumen)
    {
        AudioListener.volume = nuevoVolumen; 
        PlayerPrefs.SetFloat("VolumenMusica", nuevoVolumen);
        PlayerPrefs.Save();
    }

    public void CambiarVolumenEfectos(float nuevoVolumen)
    {
        PlayerPrefs.SetFloat("VolumenEfectos", nuevoVolumen);
        PlayerPrefs.Save();
    }

    // --- NUEVO: MÉTODO PARA EL BOTÓN DE VIBRACIÓN ---
    public void CambiarVibracion(bool activar)
    {
        if (activar)
        {
            PlayerPrefs.SetInt("VibracionActiva", 1);
            Handheld.Vibrate(); // Hace vibrar el móvil como prueba al encenderlo
        }
        else
        {
            PlayerPrefs.SetInt("VibracionActiva", 0);
        }
        PlayerPrefs.Save();
    }

    // --- MÉTODOS DE NAVEGACIÓN ---
    public void IniciarDueloIndividual()
    {
        SceneManager.LoadScene("MesaJuego");
    }
    // --- MÉTODOS DE IDIOMA Y SESIÓN ---

    public void CambiarIdioma(int indiceIdioma)
    {
        // indice 0 = Español, indice 1 = Inglés
        if (indiceIdioma == 0)
        {
            Debug.Log("Idioma cambiado a Español");
            PlayerPrefs.SetInt("IdiomaSeleccionado", 0);
        }
        else if (indiceIdioma == 1)
        {
            Debug.Log("Idioma cambiado a Inglés");
            PlayerPrefs.SetInt("IdiomaSeleccionado", 1);
        }
        PlayerPrefs.Save();
        
        // Aquí en el futuro puedes conectar tu sistema de traducción
    }

    public void CerrarSesion()
    {
        // Borramos los datos del jugador actual
        DatosGlobales.usuarioLogueado = null;
        PlayerPrefs.DeleteKey("NombreUsuario");
        PlayerPrefs.Save();

        // Cargamos la pantalla de inicio de sesión (Asegúrate de que el nombre coincida con tu escena)
        SceneManager.LoadScene("PantallaLogin"); 
    }
}