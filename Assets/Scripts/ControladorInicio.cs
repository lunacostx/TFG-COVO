using System.Collections;
using UnityEngine;

public class ControladorInicio : MonoBehaviour
{
    [SerializeField] private GameObject pantallaPortada;
    [SerializeField] private GameObject menuPrincipal;
    [SerializeField] private float tiempoDeEspera = 2.0f;

    private void Start()
    {
        // 1. Nos aseguramos de que el estado inicial sea el correcto
        pantallaPortada.SetActive(true);
        menuPrincipal.SetActive(false);

        // 2. Iniciamos la cuenta atrás optimizada
        StartCoroutine(TransicionAlMenu());
    }

    private IEnumerator TransicionAlMenu()
    {
        // Congelamos esta función durante los segundos indicados
        yield return new WaitForSeconds(tiempoDeEspera);

        // 3. Hacemos el cambiazo
        pantallaPortada.SetActive(false);
        menuPrincipal.SetActive(true);
    }
}