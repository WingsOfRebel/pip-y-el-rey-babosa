using UnityEngine;

// Camina de un lado a otro y se da la vuelta al chocar con una pared o al llegar a un borde.
// Lo usan la babosa, el caracol y la babosa de púas (esta última no se puede pisar).
public class EnemigoCaminante : Enemigo
{
    public float m_Velocidad = 2f;
    public float m_Direccion = -1f;       // -1 izquierda, 1 derecha
    public bool m_NoCaerse = true;        // da la vuelta en los bordes

    private Collider2D m_Cuerpo;
    private int m_MascaraSuelo;

    protected override void Awake()
    {
        base.Awake();
        m_Cuerpo = GetComponent<Collider2D>();
        m_MascaraSuelo = LayerMask.GetMask("Suelo");
    }

    private void FixedUpdate()
    {
        if (!Activo)
        {
            if (!m_Muerto && m_Rb.bodyType == RigidbodyType2D.Dynamic)
                m_Rb.linearVelocity = new Vector2(0f, m_Rb.linearVelocity.y);
            return;
        }

        Bounds b = m_Cuerpo.bounds;
        Vector2 adelante = new Vector2(m_Direccion, 0f);

        // ¿Pared enfrente?
        bool pared = Physics2D.Raycast(b.center, adelante, b.extents.x + 0.08f, m_MascaraSuelo);

        // ¿Se acaba el suelo enfrente?
        bool borde = false;
        if (m_NoCaerse)
        {
            Vector2 pie = new Vector2(b.center.x + m_Direccion * (b.extents.x + 0.1f), b.min.y + 0.1f);
            bool enSuelo = Physics2D.Raycast(new Vector2(b.center.x, b.min.y + 0.1f), Vector2.down, 0.3f, m_MascaraSuelo);
            borde = enSuelo && !Physics2D.Raycast(pie, Vector2.down, 0.6f, m_MascaraSuelo);
        }

        if (pared || borde) m_Direccion = -m_Direccion;

        m_Rb.linearVelocity = new Vector2(m_Direccion * m_Velocidad, m_Rb.linearVelocity.y);
        if (m_Sr != null) m_Sr.flipX = m_Direccion > 0f;   // los sprites de Kenney miran a la izquierda
    }
}
