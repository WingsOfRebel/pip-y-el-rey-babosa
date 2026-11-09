using UnityEngine;

// Bandera del final del nivel. En el nivel del jefe aparece solo cuando lo vences.
public class Meta : MonoBehaviour
{
    private bool m_Tocada;

    public void Activar(bool activa)
    {
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>()) sr.enabled = activa;
        foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = activa;
    }

    private void OnTriggerEnter2D(Collider2D otro)
    {
        if (m_Tocada) return;
        var jugador = otro.GetComponent<Jugador>();
        if (jugador == null || jugador.Muerto) return;
        m_Tocada = true;
        Juego.Instancia.NivelCompletado();
    }
}
