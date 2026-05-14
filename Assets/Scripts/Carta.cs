using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// script que controla como se ve y que hace cada carta en la mesa
public class Carta : MonoBehaviour, IPointerClickHandler
{
    [Header("Datos de la Carta")]
    public DatosCarta datos;

    [Header("Componentes Visuales")]
    public Image imagenCara;
    public Image imagenReverso;

    [Header("Objeto para Ocultar (Reverso)")]
    // guardamos la tapa de la carta para no buscarla todo el rato
    [SerializeField] private GameObject objetoOcultador;

    // guardamos al jefe de la mesa para avisarle cuando tocamos la carta
    private ControladorMesa controlador;

    private void Awake()
    {
        // buscamos al controlador en la escena al nacer
        controlador = FindObjectOfType<ControladorMesa>();

        // si se nos olvido arrastrar la tapa en unity la buscamos nosotras
        if (objetoOcultador == null)
        {
            Transform tOcultador = transform.Find("Ocultador");
            if (tOcultador != null)
            {
                objetoOcultador = tOcultador.gameObject;
            }
        }

        // obligamos a la carta a nacer boca abajo para evitar trampas
        if (objetoOcultador != null)
        {
            objetoOcultador.SetActive(true);
        }
    }

    // le pasamos la informacion y los dibujos a la carta
    public void ConfigurarCarta(DatosCarta nuevosDatos, Sprite spriteReversoBaraja)
    {
        datos = nuevosDatos;

        if (imagenCara != null && datos != null)
        {
            imagenCara.sprite = datos.imagenFrente;
        }

        if (imagenReverso != null && spriteReversoBaraja != null)
        {
            imagenReverso.sprite = spriteReversoBaraja;
        }
    }

    // quitamos o ponemos la tapa para ver la carta o esconderla
    public void IniciarGiro(bool mostrarCara)
    {
        if (objetoOcultador != null)
        {
            objetoOcultador.SetActive(!mostrarCara);
        }
    }

    // detecta el clic del raton sin tener que usar el componente boton
    public void OnPointerClick(PointerEventData eventData)
    {
        // si el juego no ha terminado le chivamos al controlador que nos han tocado
        if (controlador != null && !controlador.partidaTerminada)
        {
            controlador.AlPulsarCartaDeMano(this);
        }
    }
}