using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Este script controla el comportamiento visual de cada carta individual
public class Carta : MonoBehaviour, IPointerClickHandler
{
    [Header("Datos de la Carta")]
    public DatosCarta datos;

    [Header("Componentes Visuales")]
    public Image imagenCara;
    public Image imagenReverso;

    [Header("Objeto para Ocultar (Reverso)")]
    // --- OPTIMIZACIÓN: Caché del objeto ocultador para evitar búsquedas repetidas ---
    [SerializeField] private GameObject objetoOcultador;

    // Referencia al controlador de la mesa para notificar los clics
    private ControladorMesa controlador;

    private void Awake()
    {
        // Buscamos el controlador en la escena al inicio
        controlador = FindObjectOfType<ControladorMesa>();

        if (objetoOcultador == null)
        {
            Transform tOcultador = transform.Find("Ocultador");
            if (tOcultador != null)
            {
                objetoOcultador = tOcultador.gameObject;
            }
        }

        // --- SOLUCIÓN: Movemos esto al Awake ---
        // Así nace boca abajo desde el milisegundo 1, y luego la mesa puede girarla si quiere
        if (objetoOcultador != null)
        {
            objetoOcultador.SetActive(true);
        }
    }

    // ⚠️ BORRA LA FUNCIÓN Start() POR COMPLETO. Ya no la necesitamos aquí.


    // Función para configurar la carta con sus datos y el reverso de la baraja
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

    // Función para mostrar u ocultar la cara de la carta activando/desactivando el reverso
    public void IniciarGiro(bool mostrarCara)
    {
        if (objetoOcultador != null)
        {
            objetoOcultador.SetActive(!mostrarCara);
        }
    }

    // --- MANTENEMOS LA FUNCIONALIDAD: Implementamos la interfaz para detectar clics en la carta ---
    // Esto es más eficiente que usar un componente 'Button' extra.
    public void OnPointerClick(PointerEventData eventData)
    {
        if (controlador != null && !controlador.partidaTerminada)
        {
            // Notificamos al controlador de la mesa que se ha pulsado esta carta
            controlador.AlPulsarCartaDeMano(this);
        }
    }
}