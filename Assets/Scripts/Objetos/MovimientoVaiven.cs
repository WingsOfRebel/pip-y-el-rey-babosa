using UnityEngine;

// Mueve un objeto de ida y vuelta (sierras). Opcionalmente lo hace girar.
public class MovimientoVaiven : MonoBehaviour
{
    public Vector2 m_Direccion = Vector2.right;
    public float m_Distancia = 2f;
    public float m_Velocidad = 2f;
    public float m_Giro = 360f;        // grados por segundo (0 = no gira)

    private Vector3 m_Origen;
    private Rigidbody2D m_Rb;
    private float m_Fase;

    private void Awake()
    {
        m_Origen = transform.position;
        m_Rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (!Juego.Instancia.Jugable) return;
        m_Fase += Time.fixedDeltaTime * m_Velocidad;
        float d = Mathf.PingPong(m_Fase, m_Distancia * 2f) - m_Distancia;
        Vector2 pos = (Vector2)m_Origen + m_Direccion.normalized * d;
        if (m_Rb != null) m_Rb.MovePosition(pos); else transform.position = pos;
    }

    private void Update()
    {
        if (m_Giro != 0f && Juego.Instancia.Jugable) transform.Rotate(0f, 0f, -m_Giro * Time.deltaTime);
    }
}
