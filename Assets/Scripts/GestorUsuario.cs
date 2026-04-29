using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GestorUsuario : MonoBehaviour
{
    public TextMeshProUGUI textoNombre;
    public TextMeshProUGUI textoNivel;
    public Slider barraXP;

    void Start()
    {
        CargarDatos();
    }

    public void CargarDatos()
    {
        // Recuperamos el nombre del Login (si no hay, ponemos "Invitado")
        string nombreGuardado = PlayerPrefs.GetString("NombreUsuario", "Jugador");
        int nivelActual = PlayerPrefs.GetInt("Nivel", 1);
        float xpActual = PlayerPrefs.GetFloat("Experiencia", 0f);

        textoNombre.text = nombreGuardado;
        textoNivel.text = "Nivel " + nivelActual;
        
        // La barra de XP va de 0 a 1 (donde 1 es subir de nivel)
        barraXP.value = xpActual;
    }

    // Llamaremos a esto cuando el jugador gane una partida en el ControladorMesa
    public void GanarExperiencia(float cantidad)
    {
        float xp = PlayerPrefs.GetFloat("Experiencia", 0f);
        xp += cantidad;

        if (xp >= 1f) // ¡Subida de nivel!
        {
            xp = 0f;
            int nivel = PlayerPrefs.GetInt("Nivel", 1);
            PlayerPrefs.SetInt("Nivel", nivel + 1);
        }

        PlayerPrefs.SetFloat("Experiencia", xp);
        PlayerPrefs.Save();
        CargarDatos(); // Refrescar visualmente
    }
}