using UnityEngine;

// Mosca: vuela de lado a lado ondulando, sin gravedad. Se puede pisar.
public class EnemigoVolador : Enemigo
{
    public float m_Distancia = 3f;      // cuánto se aleja del punto inicial a cada lado
    public float m_Velocidad = 2f;
    public float m_Ondulacion = 0.6f;   // altura de la onda

    private Vector2 m_Origen;
    private float m_Fase;
    private float m_XAnterior;

    protected override void Awake()
    {
        base.Awake();
        m_Origen = transform.position;
        m_XAnterior = m_Origen.x;
    }

    private void FixedUpdate()
    {
        if (!Activo) return;
        m_Fase += Time.fixedDeltaTime * m_Velocidad;

        float x = m_Origen.x + Mathf.PingPong(m_Fase, m_Distancia * 2f) - m_Distancia;
        float y = m_Origen.y + Mathf.Sin(m_Fase * 2f) * m_Ondulacion;
        m_Rb.MovePosition(new Vector2(x, y));

        if (m_Sr != null) m_Sr.flipX = x > m_XAnterior;
        m_XAnterior = x;
    }
}
