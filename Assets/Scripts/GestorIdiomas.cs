using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GestorIdiomas : MonoBehaviour
{
    public static GestorIdiomas instancia;
    public TMP_Dropdown desplegableIdiomas;

    // 0 = Español, 1 = Inglés, 2 = Francés
    private int idiomaActual = 0; 

    // Diccionario optimizado: Clave -> [Español, Inglés, Francés]
    private Dictionary<string, string[]> baseDeDatosIdiomas = new Dictionary<string, string[]>
    {
        { "boton_individual", new string[] { "INDIVIDUAL", "<size=90%>SINGLE PLAYER</size>", "INDIVIDUEL" } },        
        { "boton_multijugador", new string[] { "MULTIJUGADOR", "MULTIPLAYER", "MULTIJOUEUR" } },
        { "texto_ajustes", new string[] { "AJUSTES", "SETTINGS", "<size=80%>PARAMETRES</size>" } },        
        { "texto_musica", new string[] { "MUSICA", "MUSIC", "MUSIQUE" } },
        { "texto_sonido", new string[] { "SONIDO", "SOUND", "SON" } },
        { "texto_idiomas", new string[] { "IDIOMAS", "LANGUAGES", "LANGUES" } },
        { "buscando_partida", new string[] { "Buscando partida...", "Searching for match...", "Recherche de partie..." } },
        { "boton_cerrar_sesion", new string[] { "Cerrar Sesion", "Log Out", "Deconnexion" } },
        { "boton_partida_rapida", new string[] { "Partida Rapida", "Quick Match", "Partie Rapide" } },       
        { "boton_invitar_amigo", new string[] { "Invitar a un amigo", "Invite a friend", "Inviter un ami" } },
        { "boton_iniciar_sesion", new string[] { "Iniciar Sesion", "Log In", "Se Connecter" } },
        { "boton_entrar", new string[] { "ENTRAR", "ENTER", "ENTRER" } },
        { "boton_comenzar", new string[] { "Comenzar", "Start", "Commencer" } },
        { "opcion_3_jugadores", new string[] { "3 Jugadores", "3 Players", "3 Joueurs" } },
        { "opcion_4_jugadores", new string[] { "4 Jugadores", "4 Players", "4 Joueurs" } },
        { "boton_reglas", new string[] { "REGLAS", "RULES", "REGLES" } },
        { "boton_guardar", new string[] { "Guardar", "Save", "Sauvegarder" } },
        { "texto_pregunta_registro", new string[] { "¿No tienes cuenta? Registrate aqui", "Don't have an account? Register here", "Pas de compte ? Inscrivez-vous ici" } },
        { "placeholder_nombre", new string[] { "Nombre del Jugador", "Player Name", "Nom du Joueur" } },
        { "placeholder_contraseña", new string[] { "Contraseña", "Password", "Mot de passe" } },
        { "placeholder_correo", new string[] { "Correo electrónico", "Email", "E-mail" } },
        { "texto_vibracion", new string[] { "Vibracion", "Vibration", "Vibration" } },
        { "texto_tu_perfil", new string[] { "Tu perfil", "Your Profile", "Ton Profil" } }, // Podrás ir añadiendo todas tus palabras aquí poco a poco
    };

    private void Awake()
    {
        if (instancia == null) instancia = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Carga el idioma guardado de la partida anterior (Por defecto 0 = Español)
        idiomaActual = PlayerPrefs.GetInt("IdiomaGuardado", 0);
        
        if(desplegableIdiomas != null)
        {
            desplegableIdiomas.value = idiomaActual;
            // Conecta el desplegable por código para máxima eficiencia
            desplegableIdiomas.onValueChanged.AddListener(CambiarIdioma);
        }
        
        ActualizarTodosLosTextos();
    }

    public void CambiarIdioma(int indiceIdioma)
    {
        idiomaActual = indiceIdioma;
        PlayerPrefs.SetInt("IdiomaGuardado", idiomaActual);
        ActualizarTodosLosTextos();
    }

    public string ObtenerTraduccion(string clave)
    {
        if (baseDeDatosIdiomas.ContainsKey(clave))
        {
            return baseDeDatosIdiomas[clave][idiomaActual];
        }
        return clave; // Si te olvidas de traducir algo, mostrará la clave para que te des cuenta
    }

    private void ActualizarTodosLosTextos()
    {
        // Encuentra todos los textos traducibles (incluso los apagados) y los actualiza de golpe
        TextoTraducible[] textos = FindObjectsOfType<TextoTraducible>(true);
        foreach (TextoTraducible texto in textos)
        {
            texto.ActualizarTexto();
        }
    }
}