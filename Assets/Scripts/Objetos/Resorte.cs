using System.Collections;
using UnityEngine;

// Resorte: al caer encima, lanza al jugador muy alto.
public class Resorte : MonoBehaviour
{
    public float m_Fuerza = 21f;
    public Sprite m_SpriteNormal;
    public Sprite m_SpriteEstirado;

    private void OnTriggerEnter2D(Collider2D otro)
    {
        var jugador = otro.GetComponent<Jugador>();
        if (jugador == null || jugador.Muerto || !jugador.Cayendo) return;

        jugador.Impulsar(m_Fuerza);
        Juego.Instancia.Audio.Sonar("sfx_jump-high");
        StopAllCoroutines();
        StartCoroutine(Estirar());
    }

    private IEnumerator Estirar()
    {
        var sr = GetComponent<SpriteRenderer>();
        sr.sprite = m_SpriteEstirado;
        yield return new WaitForSeconds(0.3f);
        sr.sprite = m_SpriteNormal;
    }
}
