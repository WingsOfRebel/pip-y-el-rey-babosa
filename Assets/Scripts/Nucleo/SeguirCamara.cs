using UnityEngine;

// Cámara ortográfica que sigue al jugador con suavidad, sin salirse de los bordes del nivel.
[RequireComponent(typeof(Camera))]
public class SeguirCamara : MonoBehaviour
{
    public Transform m_Objetivo;
    public float m_Suavizado = 0.15f;
    public float m_AlturaExtra = 1.5f;    // mira un poco por encima del jugador

    private Camera m_Cam;
    private Vector3 m_Velocidad;
    private Jugador m_Jugador;

    private void Awake()
    {
        m_Cam = GetComponent<Camera>();
    }

    private void Start()
    {
        if (m_Objetivo == null)
        {
            var j = FindAnyObjectByType<Jugador>();
            if (j != null) m_Objetivo = j.transform;
        }
        if (m_Objetivo != null)
        {
            m_Jugador = m_Objetivo.GetComponent<Jugador>();
            transform.position = Destino();
        }
    }

    private void LateUpdate()
    {
        if (m_Objetivo == null) return;
        if (m_Jugador != null && m_Jugador.Muerto) return;   // se queda quieta al morir
        transform.position = Vector3.SmoothDamp(transform.position, Destino(), ref m_Velocidad, m_Suavizado);
    }

    private Vector3 Destino()
    {
        Vector3 p = m_Objetivo.position + Vector3.up * m_AlturaExtra;
        var nivel = Juego.Instancia.NivelActual != null ? Juego.Instancia.NivelActual : FindAnyObjectByType<Nivel>();
        if (nivel != null)
        {
            float alto = m_Cam.orthographicSize;
            float ancho = alto * m_Cam.aspect;
            p.x = Mathf.Clamp(p.x, nivel.m_MinX + ancho, Mathf.Max(nivel.m_MinX + ancho, nivel.m_MaxX - ancho));
            p.y = Mathf.Max(p.y, nivel.m_MinY + alto);
        }
        p.z = -10f;
        return p;
    }
}
