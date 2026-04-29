using UnityEngine;

// Definimos los palos y los tipos de forma estricta
public enum PaloCarta { Oros, Copas, Espadas, Bastos }
public enum TipoCarta { As, Dos, Tres, Cuatro, Cinco, Seis, Siete, Sota, Caballo, Rey }

[CreateAssetMenu(fileName = "NuevaCarta", menuName = "Covo/Datos de Carta")]
public class DatosCarta : ScriptableObject
{
    [Header("Identificación")]
    public string nombreMostrado;
    public PaloCarta palo;
    public TipoCarta tipo;

    [Header("Atributos de Reglas")]
    public int valorPuntos; // El valor que suma al final de la partida (0 para los Reyes especiales)
    
    [Tooltip("0 = Ninguno, 4/5 = Ver propia, 6/7 = Ver rival, 10 = Intercambiar")]
    public int idPoderMistico; 

    [Header("Visuales")]
    public Sprite imagenFrente;
}