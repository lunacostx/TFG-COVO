using UnityEngine;
using TMPro;

public class TextoTraducible : MonoBehaviour
{
    public string claveDeTraduccion; 
    private TextMeshProUGUI textoUI;

    private void Awake()
    {
        textoUI = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        ActualizarTexto();
    }

    public void ActualizarTexto()
    {
        if (textoUI != null && GestorIdiomas.instancia != null)
        {
            textoUI.text = GestorIdiomas.instancia.ObtenerTraduccion(claveDeTraduccion);
        }
    }
}