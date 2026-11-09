using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Cerebro del juego. Vive entre escenas (DontDestroyOnLoad) y guarda vidas, monedas,
// puntos y el nivel actual. También decide qué pasa al morir, al ganar o al acabarse el tiempo.
public class Juego : MonoBehaviour
{
    public const string EscenaMenu = "Menu";
    public static readonly string[] EscenasNiveles = { "Nivel1", "Nivel2", "Nivel3" };

    public const int VidasIniciales = 3;
    public const int MonedasParaVida = 50;   // cada 50 monedas, una vida extra

    private static Juego s_Instancia;

    // Si alguien pide el Juego y no existe (por ejemplo, al darle Play directo en Nivel2), se crea solo.
    public static Juego Instancia
    {
        get
        {
            if (s_Instancia == null)
            {
                var go = new GameObject("Juego");
                s_Instancia = go.AddComponent<Juego>();
            }
            return s_Instancia;
        }
    }

    public int Vidas { get; private set; } = VidasIniciales;
    public int Monedas { get; private set; }
    public int Puntos { get; private set; }

    // true mientras el jugador puede moverse (no hay mensaje, pausa ni transición)
    public bool Jugable { get; private set; }
    public bool Pausado { get; private set; }

    public Nivel NivelActual { get; private set; }
    public GestorAudio Audio { get; private set; }
    public HUD Hud { get; private set; }

    private bool m_EnTransicion;
    private bool m_PartidaGanada;

    private void Awake()
    {
        if (s_Instancia != null && s_Instancia != this) { Destroy(gameObject); return; }
        s_Instancia = this;
        DontDestroyOnLoad(gameObject);

        Audio = gameObject.AddComponent<GestorAudio>();
        Hud = gameObject.AddComponent<HUD>();
        ConfigurarCapas();
    }

    // Los enemigos no chocan físicamente con el jugador ni entre ellos:
    // el contacto se detecta con sus "zonas de golpe" (triggers).
    private static void ConfigurarCapas()
    {
        int jugador = LayerMask.NameToLayer("Jugador");
        int enemigos = LayerMask.NameToLayer("Enemigos");
        int golpe = LayerMask.NameToLayer("Golpe");
        int suelo = LayerMask.NameToLayer("Suelo");
        if (jugador < 0 || enemigos < 0 || golpe < 0 || suelo < 0) return;

        Physics2D.IgnoreLayerCollision(jugador, enemigos, true);
        Physics2D.IgnoreLayerCollision(enemigos, enemigos, true);
        Physics2D.IgnoreLayerCollision(golpe, suelo, true);
        Physics2D.IgnoreLayerCollision(golpe, enemigos, true);
        Physics2D.IgnoreLayerCollision(golpe, golpe, true);
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        // Pausa con Esc o P durante un nivel
        if (NivelActual != null && !m_EnTransicion && (kb.escapeKey.wasPressedThisFrame || kb.pKey.wasPressedThisFrame))
            CambiarPausa(!Pausado);

        // Tras ganar, Enter vuelve al menú
        if (m_PartidaGanada && (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame))
            IrAlMenu();
    }

    private void CambiarPausa(bool pausa)
    {
        Pausado = pausa;
        Time.timeScale = pausa ? 0f : 1f;
        Jugable = !pausa;
        if (pausa) Hud.MostrarMensaje("PAUSA\n<size=40>Esc o P para continuar</size>");
        else Hud.OcultarMensaje();
    }

    // ---------- Flujo de partida ----------

    public void IniciarPartida()
    {
        if (m_EnTransicion) return;
        ReiniciarMarcadores();
        Audio.Sonar("sfx_select");
        StartCoroutine(Cargar(EscenasNiveles[0], 0f));
    }

    public void IrAlMenu()
    {
        m_PartidaGanada = false;
        Time.timeScale = 1f;
        Pausado = false;
        ReiniciarMarcadores();
        StartCoroutine(Cargar(EscenaMenu, 0f));
    }

    private void ReiniciarMarcadores()
    {
        Vidas = VidasIniciales;
        Monedas = 0;
        Puntos = 0;
        m_PartidaGanada = false;
    }

    // Lo llama el componente Nivel de cada escena al empezar
    public void EntrarNivel(Nivel nivel)
    {
        NivelActual = nivel;
        m_EnTransicion = false;
        Time.timeScale = 1f;
        Pausado = false;
        Audio.PonerMusica(nivel.m_EsJefe ? "musica_jefe" : "musica_niveles");
        Hud.MostrarHud(true);
        Hud.ActualizarNivel(nivel.m_Numero, nivel.m_Nombre);
        Hud.ActualizarJefe(-1);
        ActualizarHud();
        StartCoroutine(AnuncioNivel(nivel));
    }

    // Lo llama el menú al abrirse
    public void EntrarMenu()
    {
        NivelActual = null;
        m_EnTransicion = false;
        Jugable = false;
        Time.timeScale = 1f;
        Audio.PonerMusica("musica_niveles");
        Hud.MostrarHud(false);
        Hud.OcultarMensaje();
    }

    private IEnumerator AnuncioNivel(Nivel nivel)
    {
        Jugable = false;
        Hud.MostrarMensaje("NIVEL " + nivel.m_Numero + "\n<size=48>" + nivel.m_Nombre + "</size>");
        yield return new WaitForSecondsRealtime(1.5f);
        Hud.OcultarMensaje();
        Jugable = true;
    }

    // ---------- Marcadores ----------

    public void SumarMoneda()
    {
        Monedas++;
        Puntos += 10;
        Audio.Sonar("sfx_coin");
        if (Monedas % MonedasParaVida == 0) SumarVida();
        ActualizarHud();
    }

    public void SumarPuntos(int puntos)
    {
        Puntos += puntos;
        ActualizarHud();
    }

    public void SumarVida()
    {
        Vidas++;
        Audio.Sonar("sfx_magic");
        ActualizarHud();
    }

    public void ActualizarHud()
    {
        Hud.ActualizarMarcadores(Vidas, Monedas, Puntos);
    }

    // ---------- Fin de vida, tiempo y nivel ----------

    // El jugador tocó un enemigo, un peligro o cayó al vacío
    public void JugadorMurio()
    {
        if (m_EnTransicion) return;
        m_EnTransicion = true;
        Jugable = false;
        Vidas--;
        ActualizarHud();
        Audio.Sonar("sfx_hurt");

        if (Vidas <= 0)
            StartCoroutine(VolverAlPrimerNivel("¡EL PERSONAJE MURIÓ!\n<size=44>Te quedaste sin vidas. Vuelves al nivel 1</size>"));
        else
            StartCoroutine(RepetirNivel("¡PERDISTE UNA VIDA!\n<size=44>Te quedan " + Vidas + "</size>"));
    }

    // El reloj del nivel llegó a 0: no completaste el nivel
    public void TiempoAgotado()
    {
        if (m_EnTransicion) return;
        int numero = NivelActual != null ? NivelActual.m_Numero : 1;

        if (numero >= 2)
        {
            // Regla de la tarea: no completar el nivel 2 o 3 te regresa al primero
            m_EnTransicion = true;
            Jugable = false;
            Audio.Sonar("sfx_hurt");
            StartCoroutine(VolverAlPrimerNivel("¡SE ACABÓ EL TIEMPO!\n<size=44>El personaje murió. Vuelves al nivel 1</size>"));
        }
        else
        {
            JugadorMurio();   // en el nivel 1 cuenta como perder una vida
        }
    }

    public void NivelCompletado()
    {
        if (m_EnTransicion || NivelActual == null) return;
        m_EnTransicion = true;
        Jugable = false;

        int bono = Mathf.CeilToInt(NivelActual.TiempoRestante) * 5;
        Puntos += bono;
        ActualizarHud();
        Audio.Sonar("sfx_gem");

        int siguiente = NivelActual.m_Numero;   // índice del próximo nivel en el arreglo
        if (siguiente < EscenasNiveles.Length)
            StartCoroutine(SiguienteNivel(siguiente, bono));
        else
            StartCoroutine(Victoria(bono));
    }

    private IEnumerator SiguienteNivel(int indice, int bono)
    {
        string aviso = indice == EscenasNiveles.Length - 1 ? "¡Cuidado! Sigue el jefe" : "Sigue el nivel " + (indice + 1);
        Hud.MostrarMensaje("¡NIVEL COMPLETADO!\n<size=44>Bono de tiempo: +" + bono + "\n" + aviso + "</size>");
        yield return new WaitForSecondsRealtime(2.5f);
        yield return Cargar(EscenasNiveles[indice], 0f);
    }

    private IEnumerator Victoria(int bono)
    {
        m_PartidaGanada = true;
        Audio.PonerMusica("musica_niveles");
        Hud.MostrarMensaje("¡GANASTE!\n<size=44>Derrotaste al Rey Babosa\nPuntos: " + Puntos + "   Monedas: " + Monedas +
                           "\n\nPresiona Enter para volver al menú</size>");
        yield break;
    }

    private IEnumerator RepetirNivel(string mensaje)
    {
        Hud.MostrarMensaje(mensaje);
        yield return new WaitForSecondsRealtime(2f);
        yield return Cargar(SceneManager.GetActiveScene().name, 0f);
    }

    private IEnumerator VolverAlPrimerNivel(string mensaje)
    {
        Hud.MostrarMensaje(mensaje);
        yield return new WaitForSecondsRealtime(3f);
        ReiniciarMarcadores();
        yield return Cargar(EscenasNiveles[0], 0f);
    }

    private IEnumerator Cargar(string escena, float espera)
    {
        m_EnTransicion = true;
        Jugable = false;
        if (espera > 0f) yield return new WaitForSecondsRealtime(espera);
        Time.timeScale = 1f;
        Hud.OcultarMensaje();
        SceneManager.LoadScene(escena);
    }
}
