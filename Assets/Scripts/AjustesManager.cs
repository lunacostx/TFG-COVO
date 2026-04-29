using UnityEngine;
using UnityEngine.UI;

public class ControladorToggle : MonoBehaviour
{
    public Toggle miToggle;
    public Image imagenGema; // El Checkmark (gema verde)

    void Start()
    {
        // Cargar el estado guardado al iniciar
        bool estadoGuardado = PlayerPrefs.GetInt("Vibracion", 1) == 1;
        miToggle.isOn = estadoGuardado;
        
        // Suscribirse al evento de cambio
        miToggle.onValueChanged.AddListener(AlCambiarValor);
        
        ActualizarVisual(estadoGuardado);
    }

    void AlCambiarValor(bool activado)
    {
        // Guardar el ajuste de forma optimizada
        PlayerPrefs.SetInt("Vibracion", activado ? 1 : 0);
        PlayerPrefs.Save();

        if (activado) {
            #if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate(); // Vibra al activar para dar feedback
            #endif
            Debug.Log("Vibración Activada");
        }

        ActualizarVisual(activado);
    }

    void ActualizarVisual(bool activado)
    {
        // Si quieres que la gema brille más o cambie de color
        imagenGema.color = activado ? Color.white : new Color(1, 1, 1, 0.2f); 
    }
}