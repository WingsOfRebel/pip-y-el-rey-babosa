using System.Collections;
using UnityEngine;

// Bloque "?": al golpearlo desde abajo da una moneda (o una vida) y queda vacío.
public class BloqueSorpresa : MonoBehaviour
{
    public bool m_DaVida;            // false = moneda, true = vida extra
    public Sprite m_SpriteVacio;
    public Sprite m_SpritePremio;    // moneda o corazón que salta del bloque

    private bool m_Usado;

    public void Golpear()
    {
        if (m_Usado) return;
        m_Usado = true;

        GetComponent<SpriteRenderer>().sprite = m_SpriteVacio;
        Juego.Instancia.Audio.Sonar("sfx_bump");
        if (m_DaVida) Juego.Instancia.SumarVida();
        else Juego.Instancia.SumarMoneda();

        StartCoroutine(Rebote());
        StartCoroutine(PremioSaltando());
    }

    // El bloque sube y baja un poco
    private IEnumerator Rebote()
    {
        Vector3 inicio = transform.position;
        for (float t = 0f; t < 0.15f; t += Time.deltaTime)
        {
            transform.position = inicio + Vector3.up * Mathf.Sin(t / 0.15f * Mathf.PI) * 0.25f;
            yield return null;
        }
        transform.position = inicio;
    }

    // La moneda o el corazón salen hacia arriba y desaparecen
    private IEnumerator PremioSaltando()
    {
        if (m_SpritePremio == null) yield break;
        var go = new GameObject("Premio");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = m_SpritePremio;
        sr.sortingOrder = 5;
        Vector3 inicio = transform.position + Vector3.up;
        for (float t = 0f; t < 0.5f; t += Time.deltaTime)
        {
            go.transform.position = inicio + Vector3.up * (t * 3f);
            sr.color = new Color(1f, 1f, 1f, 1f - t / 0.5f);
            yield return null;
        }
        Destroy(go);
    }
}
