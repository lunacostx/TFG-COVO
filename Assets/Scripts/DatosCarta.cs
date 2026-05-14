using UnityEngine;

// lista cerrada con los palos para no equivocarnos al escribir
public enum PaloCarta { Oros, Copas, Espadas, Bastos }

// lista con los nombres de las cartas ordenados para el codigo
public enum TipoCarta { As, Dos, Tres, Cuatro, Cinco, Seis, Siete, Sota, Caballo, Rey }

// añade una opcion en el menu de unity para crear cartas con un clic
[CreateAssetMenu(fileName = "NuevaCarta", menuName = "Covo/Datos de Carta")]
public class DatosCarta : ScriptableObject
{
    [Header("Identificación")]
    // el nombre que leera el juego
    public string nombreMostrado;
    
    // a que familia pertenece la carta mirando la lista de arriba
    public PaloCarta palo;
    
    // que numero o figura es mirando la otra lista
    public TipoCarta tipo;

    [Header("Atributos de Reglas")]
    // los puntos que nos da al acabar la ronda
    public int valorPuntos; 
    
    // el numero secreto que le dice a la mesa que magia hace esta carta
    [Tooltip("0 = Ninguno, 4/5 = Ver propia, 6/7 = Ver rival, 10 = Intercambiar")]
    public int idPoderMistico; 

    [Header("Visuales")]
    // el dibujo de la carta por delante
    public Sprite imagenFrente;
}