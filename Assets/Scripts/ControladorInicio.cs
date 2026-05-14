using System.Collections;
using UnityEngine;

// script que controla la pantalla de presentacion al abrir el juego
public class ControladorInicio : MonoBehaviour
{
    [SerializeField] private GameObject pantallaPortada;
    [SerializeField] private GameObject menuPrincipal;
    [SerializeField] private float tiempoDeEspera = 2.0f;

    private void Start()
    {
        // encendemos la portada y apagamos el menu para empezar limpios
        pantallaPortada.SetActive(true);
        menuPrincipal.SetActive(false);

        // arrancamos el temporizador en segundo plano
        StartCoroutine(TransicionAlMenu());
    }

    private IEnumerator TransicionAlMenu()
    {
        // pausamos el codigo los segundos que hayamos puesto en unity
        yield return new WaitForSeconds(tiempoDeEspera);

        // escondemos la portada y mostramos el menu principal
        pantallaPortada.SetActive(false);
        menuPrincipal.SetActive(true);
    }
}