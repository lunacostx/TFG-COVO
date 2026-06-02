using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ControladorPerfil : MonoBehaviour
{
    [Header("Componentes de Interfaz Principal")]
    public Image imagenAvatar; 
    public TextMeshProUGUI textoNombreUsuario; 

    [Header("Componentes de Edición de Perfil")]
    public GameObject panelEdicionPerfil; 
    public TMP_InputField campoNuevoNombre; 
    public Image imagenAvatarPrevia; 

    [Header("Componentes de Selección de Avatar")]
    public GameObject panelSeleccionAvatar; 
    public List<Sprite> listaAvatares; // Lista optimizada para almacenar los 6 sprites

    // OPTIMIZACIÓN: Usar un índice entero evita configuraciones dinámicas complejas en el Inspector
    public void SeleccionarNuevoAvatar(int indice)
    {
        // Validación de seguridad para evitar errores de desbordamiento de memoria
        if (listaAvatares != null && indice >= 0 && indice < listaAvatares.Count)
        {
            if (imagenAvatarPrevia != null)
            {
                imagenAvatarPrevia.sprite = listaAvatares[indice];
                imagenAvatarPrevia.preserveAspect = true; 
            }
            
            CerrarPanelAvatares();
        }
        else
        {
            Debug.LogWarning("El índice de avatar no es válido o la lista está vacía.");
        }
    }

    public void GuardarCambios()
    {
        if (campoNuevoNombre != null && !string.IsNullOrEmpty(campoNuevoNombre.text))
        {
            if (textoNombreUsuario != null)
            {
                textoNombreUsuario.text = campoNuevoNombre.text;
            }
        }

        if (imagenAvatar != null && imagenAvatarPrevia != null && imagenAvatarPrevia.sprite != null)
        {
            imagenAvatar.sprite = imagenAvatarPrevia.sprite;
            imagenAvatar.preserveAspect = true;
        }

        CerrarPanelEdicion();
    }

    // Métodos inline optimizados para el control de ventanas
    public void AbrirPanelEdicion() => panelEdicionPerfil?.SetActive(true);
    public void CerrarPanelEdicion() => panelEdicionPerfil?.SetActive(false);
    public void AbrirPanelAvatares() => panelSeleccionAvatar?.SetActive(true);
    public void CerrarPanelAvatares() => panelSeleccionAvatar?.SetActive(false);
}