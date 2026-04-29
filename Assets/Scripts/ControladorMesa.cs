using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class ControladorMesa : MonoBehaviour
{
    [Header("Configuración de Baraja Real")]
    public List<DatosCarta> baseDeDatosCartas; 
    public Sprite imagenReverso; 

    [Header("Interfaz y UI")]
    public TextMeshProUGUI textoBienvenida;
    public TextMeshProUGUI textoTurno; 
    public GameObject botonTerminar; 
    public GameObject panelResultadoFinal; 
    public TextMeshProUGUI textoResultado; 
    public GameObject botonSiguienteRonda; 
    public GameObject panelPreguntaPoder;

    [Header("Zonas de Juego")]
    public GameObject prefabCarta;
    public Transform zonaJugador;
    public Transform zonaRival;
    public Transform zonaVisibilidad;
    public Transform zonaDescarte;
    
    // --- OPTIMIZACIÓN: Tamaños unificados en caché ---
    private readonly Vector2 tamanoCartaReal = new Vector2(220, 320);
    private readonly Vector2 tamanoCartaDescarte = new Vector2(120, 180);

    // --- OPTIMIZACIÓN: Caché de tiempos de espera para evitar Garbage Collection ---
    private WaitForSeconds esperaCorta = new WaitForSeconds(1.0f);
    private WaitForSeconds esperaMedia = new WaitForSeconds(1.5f);
    private WaitForSeconds esperaLarga = new WaitForSeconds(2.0f);
    private WaitForSeconds esperaPoder = new WaitForSeconds(5.0f);

    private GameObject cartaEnElCentro;
    private List<DatosCarta> mazo = new List<DatosCarta>();
    
    private bool esperandoPoderRevelar = false; 
    private bool esperandoPoderRival = false;   
    private bool esperandoPoder10_Paso1 = false; 
    private bool esperandoPoder10_Paso2 = false;
    private Carta cartaMiaPara10 = null; 

    private bool esTurnoDelJugador = true; 
    public bool partidaTerminada = false; 
    private bool cartaVinoDelDescarte = false; 

    private int covosJugador = 0;
    private int covosRival = 0;
    private int ultimoGanador = 0; 

    private bool faseVistazoInicial = false; 
    private List<Carta> cartasVistas = new List<Carta>();

    private int cantidadCartasCovoAElegir = 0;
    private List<int> indicesVisiblesJugador = new List<int>();
    private List<int> indicesVisiblesRival = new List<int>();
    private int maxVistazosJugador = 2;

    private Coroutine temporizadorPoder;
    private bool jugadorPierdeProximoTurno = false;

    private List<DatosCarta> memoriaRival = new List<DatosCarta>();
    private int turnosCompletos = 0;
    private int turnosMinimosParaCovo = 5;

    private int quienCantoCovo = 0; 

    void Start()
    {
        if (DatosGlobales.usuarioLogueado != null)
            textoBienvenida.text = "Hola, " + DatosGlobales.usuarioLogueado.nombre_usuario;
        else
            textoBienvenida.text = "Modo Pruebas";

        if (panelPreguntaPoder != null) panelPreguntaPoder.SetActive(false);
        if (panelResultadoFinal != null) panelResultadoFinal.SetActive(false); 
        if (botonTerminar != null) botonTerminar.SetActive(false); 

        PrepararNuevaRonda();
    }

    void PrepararNuevaRonda()
    {
        esTurnoDelJugador = (Random.Range(0, 2) == 0);
        jugadorPierdeProximoTurno = false; 
        turnosCompletos = 0; 
        quienCantoCovo = 0;
        
        indicesVisiblesJugador.Clear();
        indicesVisiblesRival.Clear();
        cantidadCartasCovoAElegir = 0;
        
        memoriaRival.Clear();
        for (int i = 0; i < 4; i++) memoriaRival.Add(null);

        if (mazo.Count < 10)
        {
            CrearMazo();
            BarajarMazo();
        }

        RepartirManoInicial();
    }

    void CrearMazo()
    {
        mazo.Clear();
        mazo.AddRange(baseDeDatosCartas); // Optimización de bucle foreach
    }

    void BarajarMazo()
    {
        for (int i = 0; i < mazo.Count; i++)
        {
            DatosCarta cartaTemporal = mazo[i];
            int indiceAleatorio = Random.Range(i, mazo.Count);
            mazo[i] = mazo[indiceAleatorio];
            mazo[indiceAleatorio] = cartaTemporal;
        }
    }

    void RepartirManoInicial()
    {
        int faltanJugador = 4 - zonaJugador.childCount;
        for (int i = 0; i < faltanJugador; i++) RobarCarta(zonaJugador);

        int faltanRival = 4 - zonaRival.childCount;
        for (int i = 0; i < faltanRival; i++) RobarCarta(zonaRival);

        foreach (Transform hijo in zonaJugador) hijo.GetComponent<Carta>().IniciarGiro(false);
        foreach (Transform hijo in zonaRival) hijo.GetComponent<Carta>().IniciarGiro(false);

        if (botonTerminar != null) botonTerminar.SetActive(false);

        if (ultimoGanador == 1 && covosJugador > 0) 
        {
            cantidadCartasCovoAElegir = Mathf.Min(covosJugador, 3);
            faseVistazoInicial = false;
            if (textoTurno != null) textoTurno.text = "¡Ganaste! Elige " + cantidadCartasCovoAElegir + " carta(s) para destapar";
            MemorizarRivalInicial();
        }
        else if (ultimoGanador == 2 && covosRival > 0) 
        {
            int cantidadRival = Mathf.Min(covosRival, 3);
            List<int> disponibles = new List<int>{0, 1, 2, 3};
            for(int i = 0; i < cantidadRival; i++)
            {
                if (disponibles.Count == 0) break;
                int r = Random.Range(0, disponibles.Count);
                int idx = disponibles[r];
                disponibles.RemoveAt(r);
                
                indicesVisiblesRival.Add(idx);
                Carta cartaDestapadaRival = zonaRival.GetChild(idx).GetComponent<Carta>();
                cartaDestapadaRival.IniciarGiro(true); 
                memoriaRival[idx] = cartaDestapadaRival.datos;

                StartCoroutine(DestacarPosicion(cartaDestapadaRival.transform));
            }
            MemorizarRivalInicial();
            IniciarVistazoJugador();
        }
        else 
        {
            MemorizarRivalInicial();
            IniciarVistazoJugador();
        }
    }

    private void MemorizarRivalInicial()
    {
        int cartasParaMemorizar = Mathf.Min(4 - indicesVisiblesRival.Count, 2);
        List<int> ocultas = new List<int>();
        for (int i=0; i<4; i++) {
            if (!indicesVisiblesRival.Contains(i)) ocultas.Add(i);
        }

        for (int i=0; i<cartasParaMemorizar; i++) {
            if (ocultas.Count > 0) {
                int r = Random.Range(0, ocultas.Count);
                int idx = ocultas[r];
                ocultas.RemoveAt(r);
                memoriaRival[idx] = zonaRival.GetChild(idx).GetComponent<Carta>().datos;
            }
        }
    }

    private void IniciarVistazoJugador()
    {
        maxVistazosJugador = Mathf.Min(4 - indicesVisiblesJugador.Count, 2);

        if (maxVistazosJugador > 0)
        {
            faseVistazoInicial = true;
            cartasVistas.Clear();
            if (textoTurno != null) textoTurno.text = "Memoriza hasta " + maxVistazosJugador + " cartas tuyas";
        }
        else
        {
            ComenzarTurnos();
        }
    }

    private void ComenzarTurnos()
    {
        faseVistazoInicial = false;
        if (textoTurno != null) textoTurno.text = esTurnoDelJugador ? "¡Empiezas tú! Tu Turno" : "¡Empieza el Rival! Turno del Rival...";
        if (botonTerminar != null) botonTerminar.SetActive(esTurnoDelJugador && turnosCompletos >= turnosMinimosParaCovo);

        if (!esTurnoDelJugador) StartCoroutine(JugarTurnoRival());
    }

    IEnumerator EfectoAparicion(Transform cartaTransform)
    {
        if (cartaTransform == null) yield break;
        cartaTransform.localScale = Vector3.zero;
        float progreso = 0;
        while(progreso < 1)
        {
            progreso += Time.deltaTime * 8f; 
            if(cartaTransform != null) cartaTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, progreso);
            yield return null;
        }
        if(cartaTransform != null) cartaTransform.localScale = Vector3.one;
    }

    void RobarCarta(Transform zonaDestino)
    {
        if (mazo.Count > 0)
        {
            DatosCarta cartaRobada = mazo[0];
            mazo.RemoveAt(0);
            GameObject nuevaCarta = Instantiate(prefabCarta, zonaDestino, false);
            
            RectTransform rectCarta = nuevaCarta.GetComponent<RectTransform>();
            rectCarta.anchoredPosition = Vector2.zero;
            rectCarta.localScale = Vector3.one;
            rectCarta.sizeDelta = tamanoCartaReal; // OPTIMIZADO
            
            Carta scriptCarta = nuevaCarta.GetComponent<Carta>();
            scriptCarta.ConfigurarCarta(cartaRobada, imagenReverso);

            StartCoroutine(EfectoAparicion(nuevaCarta.transform));
        }
    }

    public void CambiarTurno()
    {
        if (partidaTerminada) return; 

        esTurnoDelJugador = !esTurnoDelJugador;
        if (esTurnoDelJugador) turnosCompletos++;

        if (esTurnoDelJugador && jugadorPierdeProximoTurno)
        {
            jugadorPierdeProximoTurno = false;
            esTurnoDelJugador = false; 
        }
        
        if (textoTurno != null) textoTurno.text = esTurnoDelJugador ? "¡Tu Turno!" : "Turno del Rival...";
        if (botonTerminar != null) botonTerminar.SetActive(esTurnoDelJugador && turnosCompletos >= turnosMinimosParaCovo);

        if (!esTurnoDelJugador) StartCoroutine(JugarTurnoRival());
    }

    public void RobarAlCentro()
    {
        // --- CHIVATOS PARA EL MAZO ---
        Debug.Log(">>> MAZO PULSADO <<<");

        if (!esTurnoDelJugador) { Debug.Log("Fallo Mazo: No es tu turno."); return; }
        if (partidaTerminada) { Debug.Log("Fallo Mazo: La partida ya ha terminado."); return; }
        if (faseVistazoInicial) { Debug.Log("Fallo Mazo: Aún estás en la fase de memorizar tus 2 cartas."); return; }
        if (cantidadCartasCovoAElegir > 0) { Debug.Log("Fallo Mazo: Tienes cartas de Covo pendientes."); return; }
        
        if (cartaEnElCentro != null) { Debug.Log("Fallo Mazo: Ya hay una carta en el centro."); return; }
        if (mazo.Count == 0) { Debug.Log("Fallo Mazo: El mazo está vacío."); return; }
        if (esperandoPoderRevelar || esperandoPoderRival || esperandoPoder10_Paso1 || esperandoPoder10_Paso2) { Debug.Log("Fallo Mazo: Esperando a resolver un poder mágico."); return; }

        Debug.Log("Todo correcto. ¡Robando carta al centro!");
        // -----------------------------

        EjecutarRoboAlCentro();
    }

    private void EjecutarRoboAlCentro()
    {
        DatosCarta cartaRobada = mazo[0];
        mazo.RemoveAt(0);
        cartaEnElCentro = Instantiate(prefabCarta, zonaVisibilidad, false);
        cartaVinoDelDescarte = false; 

        RectTransform rectCarta = cartaEnElCentro.GetComponent<RectTransform>();
        rectCarta.anchorMin = new Vector2(0.5f, 0.5f);
        rectCarta.anchorMax = new Vector2(0.5f, 0.5f);
        rectCarta.pivot = new Vector2(0.5f, 0.5f);
        rectCarta.anchoredPosition = Vector2.zero;
        rectCarta.localScale = Vector3.one;
        rectCarta.sizeDelta = tamanoCartaReal; // OPTIMIZADO
        
        Carta scriptCarta = cartaEnElCentro.GetComponent<Carta>();
        scriptCarta.ConfigurarCarta(cartaRobada, imagenReverso);
        scriptCarta.IniciarGiro(esTurnoDelJugador); 

        StartCoroutine(EfectoAparicion(cartaEnElCentro.transform));
    }

    private void DetenerTemporizador()
    {
        if (temporizadorPoder != null)
        {
            StopCoroutine(temporizadorPoder);
            temporizadorPoder = null;
        }
    }

    IEnumerator DestacarPosicion(Transform cartaDestacada)
    {
        if (cartaDestacada == null) yield break;
        cartaDestacada.localScale = new Vector3(1.15f, 1.15f, 1f);
        yield return esperaMedia;
        if (cartaDestacada != null) cartaDestacada.localScale = Vector3.one;
    }

IEnumerator MostrarErrorPenalizacion()
    {
        // 1. Avisamos del error en pantalla
        if (textoTurno != null) textoTurno.text = "¡Fallo! Pierdes el turno";
        
        // 2. Hacemos una pausa para que dé tiempo a leerlo
        yield return esperaMedia;
        
        // 3. Pasamos el turno
        if (!partidaTerminada)
        {
            CambiarTurno(); 
        }
    }
public void AlPulsarCartaDeMano(Carta cartaTocada)
    {
        // 1. PRIORIDAD: Si la partida terminó, solo permitimos girar cartas para verlas
        if (partidaTerminada) 
        {
            if (cartaTocada.transform.parent == zonaJugador)
            {
                cartaTocada.IniciarGiro(true);
            }
            return; 
        }

        // 2. PRIORIDAD: Comprobar si estamos esperando la resolución de un PODER MÁGICO
        // Esto evita que al tocar una carta para espiarla, el juego crea que es un error de emparejamiento.
        if (esperandoPoder10_Paso1)
        {
            if (cartaTocada.transform.parent == zonaJugador)
            {
                DetenerTemporizador(); 
                cartaMiaPara10 = cartaTocada; 
                esperandoPoder10_Paso1 = false;
                esperandoPoder10_Paso2 = true; 
                temporizadorPoder = StartCoroutine(CuentaAtrasPoder()); 
            }
            return; 
        }

        if (esperandoPoder10_Paso2)
        {
            if (cartaTocada.transform.parent == zonaRival)
            {
                DetenerTemporizador(); 
                int indiceMia = cartaMiaPara10.transform.GetSiblingIndex();
                int indiceRival = cartaTocada.transform.GetSiblingIndex();
                
                cartaMiaPara10.transform.SetParent(zonaRival, false);
                cartaMiaPara10.transform.SetSiblingIndex(indiceRival);
                cartaMiaPara10.IniciarGiro(indicesVisiblesRival.Contains(indiceRival)); 
                
                cartaTocada.transform.SetParent(zonaJugador, false);
                cartaTocada.transform.SetSiblingIndex(indiceMia);
                cartaTocada.IniciarGiro(indicesVisiblesJugador.Contains(indiceMia)); 

                memoriaRival[indiceRival] = null; 
                StartCoroutine(EfectoAparicion(cartaMiaPara10.transform));
                StartCoroutine(EfectoAparicion(cartaTocada.transform));

                esperandoPoder10_Paso2 = false;
                cartaMiaPara10 = null;
                CambiarTurno();
            }
            return;
        }

        if (esperandoPoderRevelar)
        {
            if (cartaTocada.transform.parent == zonaJugador)
            {
                DetenerTemporizador(); 
                StartCoroutine(RevelarCartaTemporal(cartaTocada));
                esperandoPoderRevelar = false; 
            }
            return; 
        }

        if (esperandoPoderRival)
        {
            if (cartaTocada.transform.parent == zonaRival)
            {
                DetenerTemporizador(); 
                StartCoroutine(RevelarCartaTemporal(cartaTocada));
                esperandoPoderRival = false; 
            }
            return; 
        }

        // 3. PRIORIDAD: Fases especiales (Vistazo inicial o Covo)
        if (cantidadCartasCovoAElegir > 0)
        {
            if (cartaTocada.transform.parent == zonaJugador)
            {
                int indice = cartaTocada.transform.GetSiblingIndex();
                if (!indicesVisiblesJugador.Contains(indice))
                {
                    indicesVisiblesJugador.Add(indice);
                    cartaTocada.IniciarGiro(true); 
                    cantidadCartasCovoAElegir--;
                    if (cantidadCartasCovoAElegir <= 0) IniciarVistazoJugador();
                }
            }
            return;
        }

        if (faseVistazoInicial)
        {
            if (cartasVistas.Count >= maxVistazosJugador) return; 
            if (cartaTocada.transform.parent == zonaJugador)
            {
                if (!cartasVistas.Contains(cartaTocada))
                {
                    int indice = cartaTocada.transform.GetSiblingIndex();
                    if (indicesVisiblesJugador.Contains(indice)) return;
                    cartaTocada.IniciarGiro(true); 
                    cartasVistas.Add(cartaTocada);
                    if (cartasVistas.Count >= maxVistazosJugador) StartCoroutine(TerminarVistazoInicial());
                }
            }
            return; 
        }

        // 4. LÓGICA DE JUEGO: Emparejar carta de la mano con el descarte (Fuera de turno o en turno)
        if (cartaEnElCentro == null && cartaTocada.transform.parent == zonaJugador && zonaDescarte.childCount > 0)
        {
            Carta topeDescarte = zonaDescarte.GetChild(zonaDescarte.childCount - 1).GetComponent<Carta>();

            if (cartaTocada.datos.tipo == topeDescarte.datos.tipo)
            {
                int indiceTirada = cartaTocada.transform.GetSiblingIndex();
                if (indicesVisiblesJugador.Contains(indiceTirada)) indicesVisiblesJugador.Remove(indiceTirada);
                for (int i = 0; i < indicesVisiblesJugador.Count; i++) {
                    if (indicesVisiblesJugador[i] > indiceTirada) indicesVisiblesJugador[i]--;
                }

                // Usamos la nueva función refactorizada
                MoverCartaAlDescarte(cartaTocada);
                
                if (esTurnoDelJugador) CambiarTurno(); 
                else jugadorPierdeProximoTurno = true; 
                return; 
            }
            else
            {
                // Solo penalizamos si NO estamos en una fase de poder o vistazo
                StartCoroutine(MostrarErrorPenalizacion());
                return; 
            }
        }

        // 5. RESTRICCIÓN DE TURNO: A partir de aquí solo si es tu turno real
        if (!esTurnoDelJugador) return;

        // Robar del descarte
        if (cartaEnElCentro == null && cartaTocada.transform.parent == zonaDescarte)
        {
            if (cartaTocada.transform.GetSiblingIndex() == zonaDescarte.childCount - 1)
            {
                cartaEnElCentro = cartaTocada.gameObject;
                cartaVinoDelDescarte = true; 
                cartaEnElCentro.transform.SetParent(zonaVisibilidad, false);
                cartaEnElCentro.GetComponent<RectTransform>().sizeDelta = tamanoCartaReal;
                StartCoroutine(EfectoAparicion(cartaEnElCentro.transform));
                return;
            }
        }

        // Cambiar carta del centro por una de la mano
        if (cartaEnElCentro != null && cartaTocada.transform.parent == zonaJugador)
        {
            int indiceMano = cartaTocada.transform.GetSiblingIndex();
            
            // Usamos la nueva función refactorizada para la carta de la mano
            MoverCartaAlDescarte(cartaTocada);

            cartaEnElCentro.transform.SetParent(zonaJugador, false);
            cartaEnElCentro.transform.SetSiblingIndex(indiceMano);
            cartaEnElCentro.GetComponent<RectTransform>().sizeDelta = tamanoCartaReal; 
            
            bool debeEstarOculta = !indicesVisiblesJugador.Contains(indiceMano);
            cartaEnElCentro.GetComponent<Carta>().IniciarGiro(!debeEstarOculta); 
            StartCoroutine(EfectoAparicion(cartaEnElCentro.transform));

            cartaEnElCentro = null;
            cartaVinoDelDescarte = false; 
            CambiarTurno(); 
        }
    }
public void DescartarCartaDelCentro()
    {
        // --- CHIVATOS PARA LA CONSOLA ---
        Debug.Log(">>> BOTÓN DESCARTAR PULSADO <<<");

        if (!esTurnoDelJugador) { Debug.Log("Fallo: No es tu turno."); return; }
        if (partidaTerminada) { Debug.Log("Fallo: La partida terminó."); return; }
        if (faseVistazoInicial) { Debug.Log("Fallo: Estás en Vistazo Inicial."); return; }
        if (cantidadCartasCovoAElegir > 0) { Debug.Log("Fallo: Tienes cartas Covo pendientes."); return; }
        
        if (cartaEnElCentro == null) { Debug.Log("Fallo: El juego cree que NO hay carta robada."); return; }
        if (cartaVinoDelDescarte) { Debug.Log("Fallo: Robaste del descarte, no puedes volver a tirarla."); return; }

        Debug.Log("Todo correcto. ¡La carta se va al descarte!");
        // --------------------------------

        Carta scriptCarta = cartaEnElCentro.GetComponent<Carta>();
        
        bool poderConTiempo = false;
        bool poderConPanel = false;
        
        if (scriptCarta.datos.idPoderMistico == 4 || scriptCarta.datos.idPoderMistico == 5)
        {
            esperandoPoderRevelar = true;
            poderConTiempo = true;
        }
        else if (scriptCarta.datos.idPoderMistico == 6 || scriptCarta.datos.idPoderMistico == 7)
        {
            esperandoPoderRival = true;
            poderConTiempo = true;
        }
        else if (scriptCarta.datos.idPoderMistico == 10)
        {
            poderConPanel = true;
        }

        // --- CÓDIGO OPTIMIZADO ---
        // Usamos la función auxiliar para mover, girar y encajar la carta de golpe
        MoverCartaAlDescarte(scriptCarta);
        
        cartaEnElCentro = null;
        // -------------------------

        if (poderConPanel)
        {
            if (panelPreguntaPoder != null) panelPreguntaPoder.SetActive(true);
        }
        else if (poderConTiempo) 
        {
            temporizadorPoder = StartCoroutine(CuentaAtrasPoder());
        }
        else
        {
            CambiarTurno();
        }
    }

    public void AceptarPoder10()
    {
        esperandoPoder10_Paso1 = true;
        if (panelPreguntaPoder != null) panelPreguntaPoder.SetActive(false);
        temporizadorPoder = StartCoroutine(CuentaAtrasPoder()); 
    }

    public void RechazarPoder()
    {
        if (panelPreguntaPoder != null) panelPreguntaPoder.SetActive(false);
        CambiarTurno(); 
    }

    IEnumerator CuentaAtrasPoder()
    {
        yield return esperaPoder; 

        if (esperandoPoderRevelar || esperandoPoderRival || esperandoPoder10_Paso1 || esperandoPoder10_Paso2)
        {
            esperandoPoderRevelar = false;
            esperandoPoderRival = false;
            esperandoPoder10_Paso1 = false;
            esperandoPoder10_Paso2 = false;
            cartaMiaPara10 = null;

            CambiarTurno();
        }
    }

    IEnumerator RevelarCartaTemporal(Carta carta)
    {
        carta.IniciarGiro(true); 
        yield return esperaLarga;
        carta.IniciarGiro(false); 
        
        if (esTurnoDelJugador) CambiarTurno();
    }

    IEnumerator TerminarVistazoInicial()
    {
        yield return esperaLarga; 

        foreach (Carta c in cartasVistas) c.IniciarGiro(false); 
        
        cartasVistas.Clear();
        ComenzarTurnos();
    }

    IEnumerator JugarTurnoRival()
    {
        yield return esperaCorta; 

        if (turnosCompletos >= turnosMinimosParaCovo)
        {
            int puntosEstimados = 0;
            for (int i = 0; i < memoriaRival.Count; i++)
            {
                puntosEstimados += (memoriaRival[i] == null) ? 5 : memoriaRival[i].valorPuntos;
            }

            if (puntosEstimados <= 5)
            {
                if (textoTurno != null) textoTurno.text = "¡EL RIVAL HA CANTADO COVO!";
                yield return esperaMedia; 
                quienCantoCovo = 2; 
                EjecutarFinalRonda();
                yield break; 
            }
        }

        if (zonaDescarte.childCount > 0)
        {
            Carta topeDescarte = zonaDescarte.GetChild(zonaDescarte.childCount - 1).GetComponent<Carta>();
            int indiceParaEmparejar = -1;

            for (int i = 0; i < zonaRival.childCount; i++)
            {
                if (memoriaRival[i] != null && memoriaRival[i].tipo == topeDescarte.datos.tipo)
                {
                    indiceParaEmparejar = i;
                    break;
                }
            }

            if (indiceParaEmparejar != -1)
            {
                Transform cartaDeSuMano = zonaRival.GetChild(indiceParaEmparejar);

                cartaDeSuMano.SetParent(zonaDescarte, false);
                cartaDeSuMano.GetComponent<Carta>().IniciarGiro(true); 
                RectTransform rectRivalTirada = cartaDeSuMano.GetComponent<RectTransform>();
                rectRivalTirada.anchorMin = new Vector2(0.5f, 0.5f);
                rectRivalTirada.anchorMax = new Vector2(0.5f, 0.5f);
                rectRivalTirada.pivot = new Vector2(0.5f, 0.5f);
                rectRivalTirada.anchoredPosition = Vector2.zero;
                rectRivalTirada.localScale = Vector3.one;
                rectRivalTirada.sizeDelta = tamanoCartaDescarte; // OPTIMIZADO

                StartCoroutine(EfectoAparicion(cartaDeSuMano));

                if (indicesVisiblesRival.Contains(indiceParaEmparejar)) indicesVisiblesRival.Remove(indiceParaEmparejar);
                for (int j = 0; j < indicesVisiblesRival.Count; j++) {
                    if (indicesVisiblesRival[j] > indiceParaEmparejar) indicesVisiblesRival[j]--;
                }
                memoriaRival.RemoveAt(indiceParaEmparejar);

                yield return esperaCorta;
                CambiarTurno();
                yield break; 
            }
        }

        bool roboDelDescarte = false;
        if (zonaDescarte.childCount > 0)
        {
            Carta topeDescarte = zonaDescarte.GetChild(zonaDescarte.childCount - 1).GetComponent<Carta>();
            if (topeDescarte.datos.valorPuntos <= 4)
            {
                cartaEnElCentro = topeDescarte.gameObject;
                cartaVinoDelDescarte = true;

                cartaEnElCentro.transform.SetParent(zonaVisibilidad, false);
                RectTransform rectCarta = cartaEnElCentro.GetComponent<RectTransform>();
                rectCarta.anchorMin = new Vector2(0.5f, 0.5f);
                rectCarta.anchorMax = new Vector2(0.5f, 0.5f);
                rectCarta.pivot = new Vector2(0.5f, 0.5f);
                rectCarta.anchoredPosition = Vector2.zero;
                rectCarta.localScale = Vector3.one;
                rectCarta.sizeDelta = tamanoCartaReal; // OPTIMIZADO

                StartCoroutine(EfectoAparicion(cartaEnElCentro.transform));
                roboDelDescarte = true;
                yield return esperaCorta;
            }
        }

        if (!roboDelDescarte && mazo.Count > 0)
        {
            EjecutarRoboAlCentro();
            yield return esperaCorta; 
        }

        if (cartaEnElCentro != null)
        {
            int valorRobado = cartaEnElCentro.GetComponent<Carta>().datos.valorPuntos;
            bool quiereCambiar = false;
            int indiceParaCambiar = -1;

            if (valorRobado <= 4 || roboDelDescarte)
            {
                quiereCambiar = true;
                int peorValor = -1;

                for (int i = 0; i < zonaRival.childCount; i++)
                {
                    int puntosMemorizados = (memoriaRival[i] == null) ? 5 : memoriaRival[i].valorPuntos;
                    if (puntosMemorizados > peorValor)
                    {
                        peorValor = puntosMemorizados;
                        indiceParaCambiar = i;
                    }
                }

                if (peorValor <= valorRobado && !roboDelDescarte) quiereCambiar = false;
            }

            if (quiereCambiar && indiceParaCambiar != -1)
            {
                Transform cartaDeSuMano = zonaRival.GetChild(indiceParaCambiar);

                cartaDeSuMano.SetParent(zonaDescarte, false);
                cartaDeSuMano.GetComponent<Carta>().IniciarGiro(true); 
                RectTransform rectRivalTirada = cartaDeSuMano.GetComponent<RectTransform>();
                rectRivalTirada.anchorMin = new Vector2(0.5f, 0.5f);
                rectRivalTirada.anchorMax = new Vector2(0.5f, 0.5f);
                rectRivalTirada.pivot = new Vector2(0.5f, 0.5f);
                rectRivalTirada.anchoredPosition = Vector2.zero;
                rectRivalTirada.localScale = Vector3.one;
                rectRivalTirada.sizeDelta = tamanoCartaDescarte; // OPTIMIZADO

                StartCoroutine(EfectoAparicion(cartaDeSuMano));

                cartaEnElCentro.transform.SetParent(zonaRival, false);
                cartaEnElCentro.transform.SetSiblingIndex(indiceParaCambiar);
                cartaEnElCentro.GetComponent<RectTransform>().sizeDelta = tamanoCartaReal; // OPTIMIZADO
                
                bool debeEstarOculta = !indicesVisiblesRival.Contains(indiceParaCambiar);
                cartaEnElCentro.GetComponent<Carta>().IniciarGiro(!debeEstarOculta); 

                StartCoroutine(EfectoAparicion(cartaEnElCentro.transform));

                memoriaRival[indiceParaCambiar] = cartaEnElCentro.GetComponent<Carta>().datos;
                StartCoroutine(DestacarPosicion(cartaEnElCentro.transform));
                cartaEnElCentro = null;
            }
            else 
            {
                int idMagicoDescartado = cartaEnElCentro.GetComponent<Carta>().datos.idPoderMistico;

                cartaEnElCentro.transform.SetParent(zonaDescarte, false);
                RectTransform rectRivalDescarte = cartaEnElCentro.GetComponent<RectTransform>();
                rectRivalDescarte.anchorMin = new Vector2(0.5f, 0.5f);
                rectRivalDescarte.anchorMax = new Vector2(0.5f, 0.5f);
                rectRivalDescarte.pivot = new Vector2(0.5f, 0.5f);
                rectRivalDescarte.anchoredPosition = Vector2.zero;
                rectRivalDescarte.localScale = Vector3.one;
                rectRivalDescarte.sizeDelta = tamanoCartaDescarte; // OPTIMIZADO
                
                cartaEnElCentro.GetComponent<Carta>().IniciarGiro(true); 
                StartCoroutine(EfectoAparicion(cartaEnElCentro.transform));

                cartaEnElCentro = null;

                if (idMagicoDescartado == 4 || idMagicoDescartado == 5)
                {
                    int indiceParaEspiar = -1;
                    for (int i = 0; i < zonaRival.childCount; i++)
                    {
                        if (memoriaRival[i] == null) { indiceParaEspiar = i; break; }
                    }

                    if (indiceParaEspiar != -1)
                    {
                        Carta cartaEspiada = zonaRival.GetChild(indiceParaEspiar).GetComponent<Carta>();
                        memoriaRival[indiceParaEspiar] = cartaEspiada.datos;
                        
                        StartCoroutine(DestacarPosicion(cartaEspiada.transform));
                        yield return esperaMedia;
                    }
                }
                else if (idMagicoDescartado == 6 || idMagicoDescartado == 7)
                {
                    if (zonaJugador.childCount > 0)
                    {
                        int randomJugador = Random.Range(0, zonaJugador.childCount);
                        Carta cartaEspiada = zonaJugador.GetChild(randomJugador).GetComponent<Carta>();
                        StartCoroutine(DestacarPosicion(cartaEspiada.transform));
                        yield return esperaMedia;
                    }
                }
                else if (idMagicoDescartado == 10)
                {
                    if (zonaJugador.childCount > 0)
                    {
                        int peorIndice = 0;
                        int peorValor = -1;
                        for (int i = 0; i < zonaRival.childCount; i++)
                        {
                            int puntos = (memoriaRival[i] == null) ? 5 : memoriaRival[i].valorPuntos;
                            if (puntos > peorValor) { peorValor = puntos; peorIndice = i; }
                        }

                        int randomJugador = Random.Range(0, zonaJugador.childCount);

                        Transform suCarta = zonaRival.GetChild(peorIndice);
                        Transform tuCarta = zonaJugador.GetChild(randomJugador);

                        suCarta.SetParent(zonaJugador, false);
                        suCarta.SetSiblingIndex(randomJugador);
                        suCarta.GetComponent<Carta>().IniciarGiro(indicesVisiblesJugador.Contains(randomJugador)); 

                        tuCarta.SetParent(zonaRival, false);
                        tuCarta.SetSiblingIndex(peorIndice);
                        tuCarta.GetComponent<Carta>().IniciarGiro(indicesVisiblesRival.Contains(peorIndice)); 

                        memoriaRival[peorIndice] = null; 

                        StartCoroutine(EfectoAparicion(suCarta));
                        StartCoroutine(EfectoAparicion(tuCarta));
                        yield return esperaMedia;
                    }
                }
            }
        }

        yield return esperaCorta;
        if (!partidaTerminada) CambiarTurno();
    }

    public void TerminarRonda()
    {
        if (partidaTerminada) return;
        quienCantoCovo = 1; 
        EjecutarFinalRonda();
    }

    private void EjecutarFinalRonda()
    {
        partidaTerminada = true;
        esTurnoDelJugador = false;
        
        if (botonTerminar != null) botonTerminar.SetActive(false);
        if (textoTurno != null) textoTurno.text = "¡FIN DE LA RONDA!";

        if (panelResultadoFinal != null) panelResultadoFinal.SetActive(true);

        int puntosJugador = 0;
        int puntosRival = 0;

        foreach (Transform hijo in zonaJugador)
        {
            Carta scriptCarta = hijo.GetComponent<Carta>();
            scriptCarta.IniciarGiro(true); 
            puntosJugador += scriptCarta.datos.valorPuntos; 
        }

        foreach (Transform hijo in zonaRival)
        {
            Carta scriptCarta = hijo.GetComponent<Carta>();
            scriptCarta.IniciarGiro(true); 
            puntosRival += scriptCarta.datos.valorPuntos;
        }

        TextMeshProUGUI textoBotonSiguiente = botonSiguienteRonda != null ? botonSiguienteRonda.GetComponentInChildren<TextMeshProUGUI>() : null;

        if (textoResultado != null)
        {
            if (puntosJugador < puntosRival || (puntosJugador == puntosRival && quienCantoCovo == 1))
            {
                covosJugador++;
                ultimoGanador = 1;
                
                string textoGane = "¡HAS GANADO!\n";
                if (quienCantoCovo == 2) textoGane += "<size=30>(El Rival cantó Covo y le quitaste la victoria)</size>\n";
                else if (puntosJugador == puntosRival) textoGane += "<size=30>(¡Empate! Pero tú te llevas la victoria por cantar Covo)</size>\n";

                if (covosJugador >= 4)
                {
                    textoResultado.text = textoGane + "¡ERES EL CAMPEÓN DEFINITIVO!\nHas alcanzado tu 4º Covo.";
                    textoResultado.color = Color.cyan;
                    if (textoBotonSiguiente != null) textoBotonSiguiente.text = "Volver a Jugar";
                }
                else
                {
                    textoResultado.text = textoGane + "Puntos: " + puntosJugador + " | Rival: " + puntosRival + "\nCovos tuyos: " + covosJugador;
                    textoResultado.color = Color.green;
                    if (textoBotonSiguiente != null) textoBotonSiguiente.text = "Siguiente Ronda";
                }
            }
            else if (puntosJugador > puntosRival || (puntosJugador == puntosRival && quienCantoCovo == 2))
            {
                covosRival++;
                ultimoGanador = 2;

                string textoPierdo = "¡EL RIVAL GANA!\n";
                if (quienCantoCovo == 1) textoPierdo += "<size=30>(Cantaste Covo y te salió mal la jugada)</size>\n";
                else if (puntosJugador == puntosRival) textoPierdo += "<size=30>(¡Empate! Pero el Rival se lleva la victoria por cantar Covo)</size>\n";

                if (covosRival >= 4)
                {
                    textoResultado.text = textoPierdo + "¡EL RIVAL GANA LA PARTIDA!\nHa alcanzado su 4º Covo.";
                    textoResultado.color = Color.red;
                    if (textoBotonSiguiente != null) textoBotonSiguiente.text = "Volver a Jugar";
                }
                else
                {
                    textoResultado.text = textoPierdo + "Puntos: " + puntosJugador + " | Rival: " + puntosRival + "\nCovos rival: " + covosRival;
                    textoResultado.color = Color.red;
                    if (textoBotonSiguiente != null) textoBotonSiguiente.text = "Siguiente Ronda";
                }
            }
            else 
            {
                ultimoGanador = 0;
                string textoEmpate = "¡EMPATE TÉCNICO!\n";

                textoResultado.text = textoEmpate + "Puntos: " + puntosJugador + " | Rival: " + puntosRival;
                textoResultado.color = Color.yellow;
                if (textoBotonSiguiente != null) textoBotonSiguiente.text = "Siguiente Ronda";
            }
        }

        if (botonSiguienteRonda != null) botonSiguienteRonda.SetActive(true);
    }

    public void SiguienteRonda()
    {
        if (covosJugador >= 4 || covosRival >= 4)
        {
            SceneManager.LoadScene("MenuPrincipal");
            return; 
        }

        LimpiarZona(zonaDescarte);
        LimpiarZona(zonaVisibilidad);
        cartaEnElCentro = null;
        if (panelResultadoFinal != null) panelResultadoFinal.SetActive(false);
        partidaTerminada = false;

        PrepararNuevaRonda();
    }

    private void LimpiarZona(Transform zona)
    {
        for (int i = zona.childCount - 1; i >= 0; i--)
        {
            Transform hijo = zona.GetChild(i);
            Destroy(hijo.gameObject);
        }
    }
    
    public void BotonSalirAlMenu()
    {
        SceneManager.LoadScene("MenuPrincipal");
    }

    private void MoverCartaAlDescarte(Carta carta)
    {
        carta.transform.SetParent(zonaDescarte, false);
        carta.IniciarGiro(true); 
        
        RectTransform rect = carta.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero; // La clave para que vaya al centro
        rect.localScale = Vector3.one;
        rect.sizeDelta = tamanoCartaDescarte; 

        StartCoroutine(EfectoAparicion(carta.transform));
    }
}