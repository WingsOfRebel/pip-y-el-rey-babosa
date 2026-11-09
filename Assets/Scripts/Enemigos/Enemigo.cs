using System.Collections;
using UnityEngine;

// Base de todos los enemigos. Si es "pisable", al saltarle encima muere;
// si no (babosa de púas, jefe invulnerable), tocarlo mata al jugador.
public class Enemigo : MonoBehaviour
{
    public bool m_Pisable = true;
    public int m_Puntos = 100;
    public Sprite m_SpriteAplastado;     // se muestra un momento al morir

    protected bool m_Muerto;
    protected Rigidbody2D m_Rb;
    protected SpriteRenderer m_Sr;

    // Solo actúa si está vivo y el juego no está en pausa o en un mensaje
    protected bool Activo => !m_Muerto && Juego.Instancia.Jugable;

    protected virtual void Awake()
    {
        m_Rb = GetComponent<Rigidbody2D>();
        m_Sr = GetComponent<SpriteRenderer>();
    }

    // Lo llama el jugador cuando entra en la zona de golpe (trigger hijo)
    public virtual void TocoJugador(Jugador jugador, Bounds zona)
    {
        if (m_Muerto) return;
        if (m_Pisable && jugador.CaeSobre(zona))
        {
            jugador.RebotarSobreEnemigo();
            Pisado();
        }
        else
        {
            jugador.Morir();
        }
    }

    public virtual void Pisado()
    {
        if (m_Muerto) return;
        m_Muerto = true;
        Juego.Instancia.SumarPuntos(m_Puntos);
        Juego.Instancia.Audio.Sonar("sfx_disappear");
        StartCoroutine(Desaparecer());
    }

    private IEnumerator Desaparecer()
    {
        var anim = GetComponent<AnimadorSprites>();
        if (anim != null) anim.enabled = false;
        if (m_SpriteAplastado != null && m_Sr != null) m_Sr.sprite = m_SpriteAplastado;

        foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
        if (m_Rb != null)
        {
            m_Rb.linearVelocity = Vector2.zero;
            m_Rb.bodyType = RigidbodyType2D.Kinematic;
        }
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}
