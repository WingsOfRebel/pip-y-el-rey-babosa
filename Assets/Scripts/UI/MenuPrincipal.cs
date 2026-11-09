using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Menú de inicio hecho por código: título, botón Jugar, Controles y Salir.
// Enter también empieza la partida.
public class MenuPrincipal : MonoBehaviour
{
    public string m_Titulo = "PIP Y EL REY BABOSA";

    private Font m_Fuente;
    private GameObject m_Controles;

    private void Start()
    {
        Juego.Instancia.EntrarMenu();
        m_Fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        if (FindAnyObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem));
            es.AddComponent<InputSystemUIInputModule>();
        }

        var canvasGO = new GameObject("MenuCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var escala = canvasGO.AddComponent<CanvasScaler>();
        escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escala.referenceResolution = new Vector2(1920, 1080);
        escala.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();
        var raiz = canvasGO.transform;

        var titulo = Texto(raiz, m_Titulo, 110, new Vector2(0, 300), new Vector2(1800, 160));
        titulo.color = new Color(1f, 0.85f, 0.25f);
        Texto(raiz, "Un juego de plataformas 2D", 44, new Vector2(0, 190), new Vector2(1400, 60));

        Boton(raiz, "JUGAR", new Vector2(0, 40), () => Juego.Instancia.IniciarPartida());
        Boton(raiz, "CONTROLES", new Vector2(0, -90), () => m_Controles.SetActive(!m_Controles.activeSelf));
#if !UNITY_WEBGL
        Boton(raiz, "SALIR", new Vector2(0, -220), Application.Quit);
#endif

        // El nombre y la matrícula los muestra el HUD arriba a la derecha (siempre visible)

        // Panel de controles (oculto al inicio)
        m_Controles = new GameObject("Controles", typeof(RectTransform), typeof(Image));
        m_Controles.transform.SetParent(raiz, false);
        var rt = m_Controles.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(1100, 560);
        m_Controles.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);
        Texto(m_Controles.transform,
            "CONTROLES\n\n<size=40>Moverse: A / D  o  flechas\nSaltar: Espacio, W  o  flecha arriba\n(mantén para saltar más alto)\n" +
            "Pisa a los enemigos para vencerlos\nGolpea los bloques ? desde abajo\nPausa: Esc  o  P\n\nHaz clic en CONTROLES para cerrar</size>",
            56, Vector2.zero, new Vector2(1050, 540));
        m_Controles.SetActive(false);
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb != null && (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame))
            Juego.Instancia.IniciarPartida();
    }

    private Text Texto(Transform padre, string texto, int tamano, Vector2 pos, Vector2 caja)
    {
        var go = new GameObject("Texto", typeof(RectTransform));
        go.transform.SetParent(padre, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = caja;
        var t = go.AddComponent<Text>();
        t.font = m_Fuente;
        t.text = texto;
        t.fontSize = tamano;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.supportRichText = true;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        var borde = go.AddComponent<Outline>();
        borde.effectColor = new Color(0, 0, 0, 0.9f);
        borde.effectDistance = new Vector2(3, -3);
        return t;
    }

    private void Boton(Transform padre, string texto, Vector2 pos, UnityEngine.Events.UnityAction accion)
    {
        var go = new GameObject("Boton_" + texto, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(padre, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(460, 100);
        var img = go.GetComponent<Image>();
        img.color = new Color(0.2f, 0.55f, 0.25f, 0.95f);
        var boton = go.GetComponent<Button>();
        var colores = boton.colors;
        colores.highlightedColor = new Color(0.35f, 0.75f, 0.35f);
        colores.pressedColor = new Color(0.15f, 0.4f, 0.2f);
        boton.colors = colores;
        boton.onClick.AddListener(accion);
        Texto(go.transform, texto, 52, Vector2.zero, rt.sizeDelta);
    }
}
