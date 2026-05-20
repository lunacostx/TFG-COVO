using UnityEngine;
using UnityEngine.UI;

public class ControladorSonido : MonoBehaviour
{
    public static ControladorSonido instancia;

    [Header("Reproductores de Sonido")]
    public AudioSource reproductorMusica;
    public AudioSource reproductorEfectos;

    [Header("Archivos de Audio (Clips)")]
    public AudioClip sonidoLevantarCarta;
    public AudioClip sonidoSoltarCarta;

    [Header("Interfaz de Ajustes")]
    public Slider deslizadorMusica;
    public Slider deslizadorEfectos;
    public Toggle interruptorSilencio;

    private void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        CargarAjustes();
    }

    // --- FUNCIONES PARA LOS DESLIZADORES ---

    public void CambiarVolumenMusica()
    {
        if (deslizadorMusica != null && reproductorMusica != null)
        {
            float volumen = deslizadorMusica.value;
            reproductorMusica.volume = volumen;
            PlayerPrefs.SetFloat("VolumenMusica", volumen);
            PlayerPrefs.Save();
        }
    }

    public void CambiarVolumenEfectos()
    {
        if (deslizadorEfectos != null && reproductorEfectos != null)
        {
            float volumen = deslizadorEfectos.value;
            reproductorEfectos.volume = volumen;
            PlayerPrefs.SetFloat("VolumenEfectos", volumen);
            PlayerPrefs.Save();
        }
    }

    public void AlternarSilencio()
    {
        bool silenciado = interruptorSilencio != null && interruptorSilencio.isOn;
        if (reproductorMusica != null) reproductorMusica.mute = silenciado;
        if (reproductorEfectos != null) reproductorEfectos.mute = silenciado;

        PlayerPrefs.SetInt("Silenciado", silenciado ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void CargarAjustes()
    {
        if (PlayerPrefs.HasKey("VolumenMusica"))
        {
            float volMusica = PlayerPrefs.GetFloat("VolumenMusica");
            if (reproductorMusica != null) reproductorMusica.volume = volMusica;
            if (deslizadorMusica != null) deslizadorMusica.value = volMusica;
        }

        if (PlayerPrefs.HasKey("VolumenEfectos"))
        {
            float volEfectos = PlayerPrefs.GetFloat("VolumenEfectos");
            if (reproductorEfectos != null) reproductorEfectos.volume = volEfectos;
            if (deslizadorEfectos != null) deslizadorEfectos.value = volEfectos;
        }

        if (PlayerPrefs.HasKey("Silenciado"))
        {
            bool silenciadoGuardado = PlayerPrefs.GetInt("Silenciado") == 1;
            if (reproductorMusica != null) reproductorMusica.mute = silenciadoGuardado;
            if (reproductorEfectos != null) reproductorEfectos.mute = silenciadoGuardado;
            if (interruptorSilencio != null) interruptorSilencio.isOn = silenciadoGuardado;
        }
    }

    // --- FUNCIONES PARA EFECTOS DE CARTAS ---
    
    public void SonidoLevantar()
    {
        if (sonidoLevantarCarta != null && reproductorEfectos != null)
        {
            reproductorEfectos.PlayOneShot(sonidoLevantarCarta);
        }
    }

    public void SonidoSoltar()
    {
        if (sonidoSoltarCarta != null && reproductorEfectos != null)
        {
            reproductorEfectos.PlayOneShot(sonidoSoltarCarta);
        }
    }

    public void SonidoSoltarRival()
    {
        if (sonidoSoltarCarta != null && reproductorEfectos != null)
        {
            // Cambiamos ligeramente el tono para que suene distinto al tuyo
            reproductorEfectos.pitch = 0.9f; 
            reproductorEfectos.PlayOneShot(sonidoSoltarCarta);
            reproductorEfectos.pitch = 1.0f; // Lo devolvemos a la normalidad
        }
    }
}