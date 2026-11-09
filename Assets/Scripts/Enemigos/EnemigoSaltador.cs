using UnityEngine;

// Rana: espera quieta y, cuando el jugador está cerca, salta hacia él cada cierto tiempo.
public class EnemigoSaltador : Enemigo
{
    public float m_Espera = 1.6f;          // segundos entre saltos
    public float m_FuerzaSalto = 9f;
    public float m_Avance = 3.5f;          // velocidad horizontal del salto
    public float m_DistanciaVision = 9f;
    public Sprite m_SpriteQuieta;
    public Sprite m_SpriteSaltando;

    private float m_Reloj;
    private Collider2D m_Cuerpo;
    private int m_MascaraSuelo;
    private Transform m_Jugador;

    protected override void Awake()
    {
        base.Awake();
        m_Cuerpo = GetComponent<Collider2D>();
        m_MascaraSuelo = LayerMask.GetMask("Suelo");
        m_Reloj = m_Espera;
    }

    private void FixedUpdate()
    {
        if (!Activo) return;
        if (m_Jugador == null)
        {
            var j = FindAnyObjectByType<Jugador>();
            if (j != null) m_Jugador = j.transform;
        }

        Bounds b = m_Cuerpo.bounds;
        bool enSuelo = m_Rb.linearVelocity.y <= 0.05f &&
                       Physics2D.Raycast(new Vector2(b.center.x, b.min.y + 0.05f), Vector2.down, 0.15f, m_MascaraSuelo);

        if (enSuelo)
        {
            m_Rb.linearVelocity = new Vector2(0f, m_Rb.linearVelocity.y);
            if (m_Sr != null) m_Sr.sprite = m_SpriteQuieta;
            m_Reloj -= Time.fixedDeltaTime;

            if (m_Jugador != null && m_Reloj <= 0f &&
                Mathf.Abs(m_Jugador.position.x - transform.position.x) < m_DistanciaVision)
            {
                float dir = Mathf.Sign(m_Jugador.position.x - transform.position.x);
                if (m_Sr != null) m_Sr.flipX = dir > 0f;
                m_Rb.linearVelocity = new Vector2(dir * m_Avance, m_FuerzaSalto);
                m_Reloj = m_Espera;
            }
        }
        else if (m_Sr != null)
        {
            m_Sr.sprite = m_SpriteSaltando;
        }
    }
}
