using System.Collections;
using UnityEngine;

// Rey Babosa (jefe del nivel 3). Hay que pisarlo 3 veces. Camina por la arena,
// salta y lanza bolas de fuego. Cada golpe lo vuelve más rápido.
public class Jefe : Enemigo
{
    public int m_Vida = 3;
    public float m_Velocidad = 2.5f;
    public float m_EsperaAtaque = 2.2f;
    public float m_DistanciaDespertar = 14f;
    public float m_MinX = 40f, m_MaxX = 80f;   // límites de la arena (los pone la herramienta)
    public GameObject m_PrefabBolaFuego;
    public Meta m_Meta;                         // la bandera aparece al vencerlo
    public Sprite[] m_Caminar;

    private bool m_Despierto;
    private bool m_Invulnerable;
    private float m_Direccion = -1f;
    private float m_Reloj;
    private float m_RelojAnim;
    private Transform m_Jugador;
    private Collider2D m_Cuerpo;
    private int m_MascaraSuelo;

    protected override void Awake()
    {
        base.Awake();
        m_Pisable = true;
        m_Cuerpo = GetComponent<Collider2D>();
        m_MascaraSuelo = LayerMask.GetMask("Suelo");
        m_Reloj = m_EsperaAtaque;
    }

    private void Start()
    {
        if (m_Meta != null) m_Meta.Activar(false);
    }

    private void FixedUpdate()
    {
        if (!Activo) { if (!m_Muerto) m_Rb.linearVelocity = new Vector2(0f, m_Rb.linearVelocity.y); return; }

        if (m_Jugador == null)
        {
            var j = FindAnyObjectByType<Jugador>();
            if (j == null) return;
            m_Jugador = j.transform;
        }

        // Despierta cuando el jugador entra a la arena
        if (!m_Despierto)
        {
            if (Mathf.Abs(m_Jugador.position.x - transform.position.x) > m_DistanciaDespertar) return;
            m_Despierto = true;
            Juego.Instancia.Hud.ActualizarJefe(m_Vida);
            Juego.Instancia.Audio.Sonar("sfx_magic");
        }

        // Caminar entre los límites de la arena
        if (transform.position.x < m_MinX) m_Direccion = 1f;
        else if (transform.position.x > m_MaxX) m_Direccion = -1f;
        m_Rb.linearVelocity = new Vector2(m_Direccion * m_Velocidad, m_Rb.linearVelocity.y);
        if (m_Sr != null) m_Sr.flipX = m_Direccion > 0f;

        m_RelojAnim += Time.fixedDeltaTime;
        if (m_Caminar != null && m_Caminar.Length > 0 && m_Sr != null)
            m_Sr.sprite = m_Caminar[(int)(m_RelojAnim * 6f) % m_Caminar.Length];

        // Atacar: unas veces salta, otras lanza fuego hacia el jugador
        m_Reloj -= Time.fixedDeltaTime;
        if (m_Reloj <= 0f)
        {
            m_Reloj = m_EsperaAtaque;
            if (Random.value < 0.4f && EnSuelo()) m_Rb.linearVelocity = new Vector2(m_Rb.linearVelocity.x, 13f);
            else LanzarFuego();
        }
    }

    private bool EnSuelo()
    {
        Bounds b = m_Cuerpo.bounds;
        return Physics2D.Raycast(new Vector2(b.center.x, b.min.y + 0.05f), Vector2.down, 0.2f, m_MascaraSuelo);
    }

    private void LanzarFuego()
    {
        if (m_PrefabBolaFuego == null || m_Jugador == null) return;
        float dir = Mathf.Sign(m_Jugador.position.x - transform.position.x);
        Vector3 salida = m_Cuerpo.bounds.center + new Vector3(dir * (m_Cuerpo.bounds.extents.x + 0.3f), 0.3f, 0f);
        var bola = Instantiate(m_PrefabBolaFuego, salida, Quaternion.identity);
        var rb = bola.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = new Vector2(dir * 6f, 4f);
        Juego.Instancia.Audio.Sonar("sfx_throw");
    }

    public override void TocoJugador(Jugador jugador, Bounds zona)
    {
        if (m_Muerto || m_Invulnerable) return;   // tras un golpe no hace daño por un momento
        if (jugador.CaeSobre(zona))
        {
            jugador.RebotarSobreEnemigo();
            jugador.Impulsar(14f);
            RecibirGolpe();
        }
        else
        {
            jugador.Morir();
        }
    }

    private void RecibirGolpe()
    {
        m_Vida--;
        Juego.Instancia.Hud.ActualizarJefe(Mathf.Max(m_Vida, 0));
        if (m_Vida <= 0)
        {
            Juego.Instancia.Hud.ActualizarJefe(-1);
            Juego.Instancia.Audio.Sonar("sfx_gem");
            if (m_Meta != null) m_Meta.Activar(true);
            m_Puntos = 1000;
            Pisado();
            return;
        }
        Juego.Instancia.Audio.Sonar("sfx_hurt");
        m_Velocidad *= 1.35f;
        m_EsperaAtaque *= 0.8f;
        StartCoroutine(Parpadeo());
    }

    private IEnumerator Parpadeo()
    {
        m_Invulnerable = true;
        for (int i = 0; i < 8; i++)
        {
            if (m_Sr != null) m_Sr.enabled = !m_Sr.enabled;
            yield return new WaitForSeconds(0.12f);
        }
        if (m_Sr != null) m_Sr.enabled = true;
        m_Invulnerable = false;
    }
}
