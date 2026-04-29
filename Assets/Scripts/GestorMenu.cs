using UnityEngine;
using TMPro; // ¡Nueva librería obligatoria para tocar textos TMP!

public class GestorMenus : MonoBehaviour
{
    [Header("Paneles Principales")]
    [SerializeField] private GameObject panelOpcionesMultijugador;

    [Header("Secciones Internas")]
    [SerializeField] private GameObject seccionBotones;
    [SerializeField] private GameObject seccionCodigos;

    [Header("Textos")]
    // Nueva variable para enlazar tu texto visual
    [SerializeField] private TextMeshProUGUI textoCodigoGenerado; 

    public void AbrirMenuMultijugador()
    {
        panelOpcionesMultijugador.SetActive(true);
        seccionBotones.SetActive(true);
        seccionCodigos.SetActive(false); 
    }

    public void MostrarPantallaCodigos()
    {
        seccionBotones.SetActive(false);
        seccionCodigos.SetActive(true);
        
        // Llamamos a la función creadora justo al abrir esta sección
        GenerarCodigoSala();
    }

    public void CerrarMenuMultijugador()
    {
        panelOpcionesMultijugador.SetActive(false);
    }

    // Función optimizada para crear un código de 4 caracteres
    private void GenerarCodigoSala()
    {
        // El diccionario de letras y números permitidos
        const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string codigoNuevo = "";
        
        // Bucle rápido para coger 4 caracteres al azar
        for (int i = 0; i < 4; i++) 
        {
            int indice = Random.Range(0, caracteres.Length);
            codigoNuevo += caracteres[indice];
        }

        // Plasmamos el resultado en la pantalla
        textoCodigoGenerado.text = codigoNuevo;
    }
}