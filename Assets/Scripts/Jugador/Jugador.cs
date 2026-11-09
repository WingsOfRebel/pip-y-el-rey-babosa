using UnityEngine;
using UnityEngine.InputSystem;

// Personaje principal (Pip). Se mueve, salta (más alto si mantienes la tecla),
// salta encima de los enemigos y golpea bloques "?" desde abajo.
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(SpriteRenderer))]
public class Jugador : MonoBehaviour
{
    [Header("Movimiento")]
    public float m_Velocidad = 7f;
    public float m_FuerzaSalto = 15f;
    public float m_Gravedad = 3.5f;          // gravityScale del Rigidbody2D
    public float m_TiempoCoyote = 0.1f;      // margen para saltar justo después de dejar el borde
    public float m_MargenSalto = 0.12f;      // si presionas saltar un poco antes de tocar el suelo
    public float m_ReboteEnemigo = 11f;      // impulso al pisar un enemigo

    [Header("Sprites (los pone la herramienta)")]
    public Sprite m_Quieto;
    public Sprite m_Caminar1;
    public Sprite m_Caminar2;
    public Sprite m_Saltar;
    public Sprite m_Golpeado;

    public bool Muerto { get; private set; }

    private Rigidbody2D m_Rb;
    private BoxCollider2D m_Col;
    private SpriteRenderer m_Sr;
    private int m_MascaraSuelo;

    private float m_Horizontal;
    private float m_ContadorCoyote;
    private float m_ContadorSalto;
    private bool m_EnSuelo;
    private float m_RelojAnimacion;

    private void Awake()
    {
        m_Rb = GetComponent<Rigidbody2D>();
        m_Col = GetComponent<BoxCollider2D>();
        m_Sr = GetComponent<SpriteRenderer>();
        m_MascaraSuelo = LayerMask.GetMask("Suelo");

        m_Rb.gravityScale = m_Gravedad;
        m_Rb.freezeRotation = true;
        m_Rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        m_Rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Update()
    {
        if (Muerto) return;

        bool control = Juego.Instancia.Jugable;
        var kb = Keyboard.current;

        m_Horizontal = 0f;
        bool saltoPresionado = false, saltoSoltado = false;
        if (control && kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) m_Horizontal -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) m_Horizontal += 1f;
            saltoPresionado = kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame;
            saltoSoltado = kb.spaceKey.wasReleasedThisFrame || kb.wKey.wasReleasedThisFrame || kb.upArrowKey.wasReleasedThisFrame;
        }

        // ¿Toca el suelo? Una cajita delgada justo debajo de los pies
        Bounds b = m_Col.bounds;
        m_EnSuelo = m_Rb.linearVelocity.y <= 0.1f &&
                    Physics2D.OverlapBox(new Vector2(b.center.x, b.min.y), new Vector2(b.size.x * 0.9f, 0.12f), 0f, m_MascaraSuelo) != null;

        m_ContadorCoyote = m_EnSuelo ? m_TiempoCoyote : m_ContadorCoyote - Time.deltaTime;
        m_ContadorSalto = saltoPresionado ? m_MargenSalto : m_ContadorSalto - Time.deltaTime;

        if (m_ContadorSalto > 0f && m_ContadorCoyote > 0f)
        {
            Impulsar(m_FuerzaSalto);
            m_ContadorSalto = 0f;
            m_ContadorCoyote = 0f;
            Juego.Instancia.Audio.Sonar("sfx_jump", 0.7f);
        }

        // Salto variable: si sueltas la tecla mientras subes, el salto se corta
        if (saltoSoltado && m_Rb.linearVelocity.y > 0f)
            m_Rb.linearVelocity = new Vector2(m_Rb.linearVelocity.x, m_Rb.linearVelocity.y * 0.5f);

        Animar();

        // Caer al vacío o a un pozo
        var nivel = Juego.Instancia.NivelActual;
        if (nivel != null && transform.position.y < nivel.m_LimiteCaida) Morir();
    }

    private void FixedUpdate()
    {
        if (Muerto) return;
        m_Rb.linearVelocity = new Vector2(m_Horizontal * m_Velocidad, m_Rb.linearVelocity.y);

        // No dejar que salga por la izquierda del mapa
        var nivel = Juego.Instancia.NivelActual;
        if (nivel != null && m_Rb.position.x < nivel.m_MinX + 0.5f)
        {
            m_Rb.position = new Vector2(nivel.m_MinX + 0.5f, m_Rb.position.y);
            if (m_Rb.linearVelocity.x < 0f) m_Rb.linearVelocity = new Vector2(0f, m_Rb.linearVelocity.y);
        }
    }

    private void Animar()
    {
        if (m_Horizontal > 0.01f) m_Sr.flipX = false;
        else if (m_Horizontal < -0.01f) m_Sr.flipX = true;

        if (!m_EnSuelo) { m_Sr.sprite = m_Saltar; return; }
        if (Mathf.Abs(m_Horizontal) > 0.01f)
        {
            m_RelojAnimacion += Time.deltaTime;
            m_Sr.sprite = ((int)(m_RelojAnimacion * 10f) % 2 == 0) ? m_Caminar1 : m_Caminar2;
        }
        else
        {
            m_RelojAnimacion = 0f;
            m_Sr.sprite = m_Quieto;
        }
    }

    // Salto normal, rebote sobre enemigos y resortes
    public void Impulsar(float fuerza)
    {
        m_Rb.linearVelocity = new Vector2(m_Rb.linearVelocity.x, fuerza);
    }

    public void RebotarSobreEnemigo() => Impulsar(m_ReboteEnemigo);

    // true si el jugador viene cayendo y sus pies están por encima del centro de la zona
    public bool CaeSobre(Bounds zona)
    {
        return m_Rb.linearVelocity.y < 0.5f && m_Col.bounds.min.y > zona.center.y;
    }

    public bool Cayendo => m_Rb.linearVelocity.y <= 0.1f;

    public void Morir()
    {
        if (Muerto || !Juego.Instancia.Jugable) return;
        Muerto = true;
        Juego.Instancia.JugadorMurio();

        // Animación de muerte estilo Mario: salta hacia arriba y cae atravesando el piso
        m_Sr.sprite = m_Golpeado;
        m_Sr.sortingOrder = 20;
        m_Col.enabled = false;
        m_Rb.linearVelocity = new Vector2(0f, 12f);
    }

    private void OnCollisionEnter2D(Collision2D colision)
    {
        if (Muerto) return;
        // Golpear un bloque con la cabeza (la normal apunta hacia abajo)
        var bloque = colision.collider.GetComponent<BloqueSorpresa>();
        if (bloque == null) return;
        for (int i = 0; i < colision.contactCount; i++)
        {
            if (colision.GetContact(i).normal.y < -0.5f) { bloque.Golpear(); break; }
        }
    }

    private void OnTriggerEnter2D(Collider2D otro) => RevisarContacto(otro);
    private void OnTriggerStay2D(Collider2D otro) => RevisarContacto(otro);

    private void RevisarContacto(Collider2D otro)
    {
        if (Muerto) return;
        var enemigo = otro.GetComponentInParent<Enemigo>();
        if (enemigo != null) { enemigo.TocoJugador(this, otro.bounds); return; }
        if (otro.GetComponent<Peligro>() != null) Morir();
    }
}
