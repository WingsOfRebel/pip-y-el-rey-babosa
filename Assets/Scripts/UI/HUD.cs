using UnityEngine;
using UnityEngine.UI;

// Interfaz del juego hecha por código: vidas, monedas, puntos, reloj, nombre del alumno
// y un mensaje central (nivel completado, perdiste una vida, el personaje murió...).
public class HUD : MonoBehaviour
{
    private GameObject m_Panel;      // todo lo de arriba (se oculta en el menú)
    private Text m_Vidas, m_Monedas, m_Puntos, m_Nivel, m_Reloj, m_Jefe, m_Mensaje;
    private GameObject m_FondoMensaje;
    private Font m_Fuente;

    private void Awake()
    {
        m_Fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var canvasGO = new GameObject("HUD");
        canvasGO.transform.SetParent(transform, false);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var escala = canvasGO.AddComponent<CanvasScaler>();
        escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escala.referenceResolution = new Vector2(1920, 1080);
        escala.matchWidthOrHeight = 0.5f;

        // Nombre y matrícula siempre visibles (arriba a la derecha)
        var nombre = CrearTexto(canvasGO.transform, "Nombre", "Edwin Flete - 1-22-1805", 34,
            TextAnchor.UpperRight, new Vector2(1, 1), new Vector2(-30, -20), new Vector2(700, 50));
        nombre.color = new Color(1f, 0.95f, 0.6f);

        m_Panel = new GameObject("Marcadores", typeof(RectTransform));
        m_Panel.transform.SetParent(canvasGO.transform, false);
        Estirar(m_Panel.GetComponent<RectTransform>());

        m_Vidas = CrearTexto(m_Panel.transform, "Vidas", "", 40, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(30, -20), new Vector2(500, 50));
        m_Monedas = CrearTexto(m_Panel.transform, "Monedas", "", 40, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(30, -70), new Vector2(500, 50));
        m_Puntos = CrearTexto(m_Panel.transform, "Puntos", "", 40, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(30, -120), new Vector2(500, 50));
        m_Nivel = CrearTexto(m_Panel.transform, "Nivel", "", 40, TextAnchor.UpperCenter, new Vector2(0.5f, 1), new Vector2(0, -20), new Vector2(900, 50));
        m_Reloj = CrearTexto(m_Panel.transform, "Reloj", "", 44, TextAnchor.UpperCenter, new Vector2(0.5f, 1), new Vector2(0, -70), new Vector2(600, 56));
        m_Jefe = CrearTexto(m_Panel.transform, "Jefe", "", 40, TextAnchor.UpperCenter, new Vector2(0.5f, 1), new Vector2(0, -130), new Vector2(900, 50));
        m_Jefe.color = new Color(1f, 0.45f, 0.35f);

        // Mensaje central con fondo oscuro
        m_FondoMensaje = new GameObject("FondoMensaje", typeof(RectTransform), typeof(Image));
        m_FondoMensaje.transform.SetParent(canvasGO.transform, false);
        var rt = m_FondoMensaje.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(1300, 420);
        m_FondoMensaje.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);
        m_Mensaje = CrearTexto(m_FondoMensaje.transform, "Mensaje", "", 80, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1250, 400));
        m_FondoMensaje.SetActive(false);
    }

    private static void Estirar(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    private Text CrearTexto(Transform padre, string nombre, string texto, int tamano, TextAnchor alineacion,
                            Vector2 ancla, Vector2 posicion, Vector2 tamanoCaja)
    {
        var go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = ancla;
        rt.anchoredPosition = posicion;
        rt.sizeDelta = tamanoCaja;

        var t = go.AddComponent<Text>();
        t.font = m_Fuente;
        t.text = texto;
        t.fontSize = tamano;
        t.fontStyle = FontStyle.Bold;
        t.alignment = alineacion;
        t.color = Color.white;
        t.supportRichText = true;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;

        var borde = go.AddComponent<Outline>();
        borde.effectColor = new Color(0, 0, 0, 0.85f);
        borde.effectDistance = new Vector2(2, -2);
        return t;
    }

    public void MostrarHud(bool visible) => m_Panel.SetActive(visible);

    public void ActualizarMarcadores(int vidas, int monedas, int puntos)
    {
        m_Vidas.text = "VIDAS  x" + vidas;
        m_Monedas.text = "MONEDAS  x" + monedas;
        m_Puntos.text = "PUNTOS  " + puntos.ToString("D6");
    }

    public void ActualizarNivel(int numero, string nombre) => m_Nivel.text = "NIVEL " + numero + "  -  " + nombre;

    public void ActualizarReloj(float segundos)
    {
        int s = Mathf.CeilToInt(segundos);
        m_Reloj.text = "TIEMPO  " + (s / 60) + ":" + (s % 60).ToString("00");
        m_Reloj.color = s <= 30 ? new Color(1f, 0.35f, 0.3f) : Color.white;
    }

    // vida < 0 oculta la barra del jefe
    public void ActualizarJefe(int vida)
    {
        if (vida < 0) { m_Jefe.text = ""; return; }
        // (sin símbolos raros: en el navegador la fuente no trae corazones)
        m_Jefe.text = "REY BABOSA  -  VIDA " + vida;
    }

    public void MostrarMensaje(string texto)
    {
        m_Mensaje.text = texto;
        m_FondoMensaje.SetActive(true);
    }

    public void OcultarMensaje() => m_FondoMensaje.SetActive(false);
}
