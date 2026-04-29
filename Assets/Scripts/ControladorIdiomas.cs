using UnityEngine;
using UnityEngine.Localization.Settings;

public class ControladorIdioma : MonoBehaviour
{
    // Función optimizada y directa para cambiar el idioma
    public void CambiarIdioma(int indiceIdioma)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[indiceIdioma];
    }
}