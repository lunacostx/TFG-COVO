using UnityEngine;

public class AnimacionCarta : MonoBehaviour
{
    private Vector3 posicionObjetivo;
    private Quaternion rotacionObjetiva;
    private bool moviendo = false;
    public float velocidad = 10f;

    void Update()
    {
        if (moviendo)
        {
            // Movemos la carta suavemente hacia el objetivo
            transform.position = Vector3.Lerp(transform.position, posicionObjetivo, Time.deltaTime * velocidad);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotacionObjetiva, Time.deltaTime * velocidad);

            // Si ya está muy cerca, paramos para ahorrar recursos
            if (Vector3.Distance(transform.position, posicionObjetivo) < 0.01f)
            {
                transform.position = posicionObjetivo;
                transform.rotation = rotacionObjetiva;
                moviendo = false;
            }
        }
    }

    public void IniciarMovimiento(Vector3 destino, Quaternion rotacion)
    {
        posicionObjetivo = destino;
        rotacionObjetiva = rotacion;
        moviendo = true;
    }
}