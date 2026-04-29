using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ControladorMenuPro : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelDesplegable;
    public GameObject panelReglas;
    
    [Header("Datos de Usuario")]
    public TextMeshProUGUI textoNombrePerfil;
    public TextMeshProUGUI textoNivel;
    public Slider barraProgresoXP;

    [Header("Ajustes")]
    public Slider sliderMusica;
    public Slider sliderSonido;

    void Start()
    {
        // Recuperamos el nombre que guardamos en el Login
        string nombre = PlayerPrefs.GetString("NombreUsuario", "Invitado");
        textoNombrePerfil.text = nombre;
        
        // Simulación de nivel
        textoNivel.text = "Nivel 5";
        barraProgresoXP.value = 0.4f; // 40% de nivel

        // Cerramos paneles por si acaso
        if(panelDesplegable) panelDesplegable.SetActive(false);
        if(panelReglas) panelReglas.SetActive(false);
    }

    // --- FUNCIONES DE NAVEGACIÓN ---

    public void AbrirCerrarMenu()
    {
        bool estadoActual = panelDesplegable.activeSelf;
        panelDesplegable.SetActive(!estadoActual);
    }

    public void MostrarReglas(bool mostrar)
    {
        panelReglas.SetActive(mostrar);
    }

    public void SeleccionarModo(string modo)
    {
        if (modo == "IA")
        {
            Debug.Log("Iniciando modo contra IA...");
            SceneManager.LoadScene("Juego"); 
        }
        else
        {
            Debug.Log("Buscando mesa multijugador...");
            // Aquí iría la lógica de red en el futuro
        }
    }

    public void GuardarAjustes()
    {
        PlayerPrefs.SetFloat("VolumenMusica", sliderMusica.value);
        PlayerPrefs.Save();
        Debug.Log("Ajustes guardados");
    }
}