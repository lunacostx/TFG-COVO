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
    [SerializeField] private GameObject contenidoAjustes; // Agrupa aquí sliders y etiquetas

    [Header("Paneles Emergentes")]
    public GameObject panelAjustes;
    public GameObject panelReglas;

    [Header("Controles de Sonido")]
    public Slider sliderMusica;
    public Slider sliderSonido;

    private void Start()
    {
        ConfigurarPerfil();
        ConfigurarSonidoInicial();
        
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

    // --- LÓGICA DE PANELES ---

    public void MostrarAjustes()
    {
        if (panelAjustes != null) panelAjustes.SetActive(true);
        if (contenidoAjustes != null) contenidoAjustes.SetActive(true);
        if (panelReglas != null) panelReglas.SetActive(false);

        // Ocultamos el menú principal
        SetEstadoMenuFondo(false);
    }

    public void OcultarAjustes()
    {
        if (panelAjustes != null) panelAjustes.SetActive(false);
        SetEstadoMenuFondo(true);
    }

    // Esta es la nueva función para el botón de Reglas
    public void ConmutarReglas()
    {
        if (panelReglas == null) return;

        // Invertimos el estado actual (si está encendido, se apaga; si está apagado, se enciende)
        bool mostrar = !panelReglas.activeSelf;
        
        panelReglas.SetActive(mostrar);

        // Si mostramos las reglas, ocultamos los sliders de ajustes para que no haya "texto por detrás"
        if (contenidoAjustes != null)
        {
            contenidoAjustes.SetActive(!mostrar);
        }
    }

    private void SetEstadoMenuFondo(bool estado)
    {
        if (menuPrincipal != null) menuPrincipal.SetActive(estado);
        if (panelPerfil != null) panelPerfil.SetActive(estado);
        if (botonAbrirAjustes != null) botonAbrirAjustes.SetActive(estado);
    }

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

    public void IniciarDueloIndividual()
    {
        SceneManager.LoadScene("MesaJuego");
    }
}