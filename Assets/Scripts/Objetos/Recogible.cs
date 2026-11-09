using UnityEngine;

// Objeto que se recoge al tocarlo: moneda (+10 puntos), gema (+100) o corazón (+1 vida).
public class Recogible : MonoBehaviour
{
    public enum Tipo { Moneda, Gema, Corazon }
    public Tipo m_Tipo = Tipo.Moneda;

    private Vector3 m_Origen;
    private bool m_Recogido;

    private void Start()
    {
        m_Origen = transform.position;
    }

    private void Update()
    {
        // Flota un poquito para que se note
        transform.position = m_Origen + Vector3.up * (Mathf.Sin(Time.time * 3f + m_Origen.x) * 0.08f);
    }

    private void OnTriggerEnter2D(Collider2D otro)
    {
        if (m_Recogido) return;
        var jugador = otro.GetComponent<Jugador>();
        if (jugador == null || jugador.Muerto) return;
        m_Recogido = true;

        switch (m_Tipo)
        {
            case Tipo.Moneda: Juego.Instancia.SumarMoneda(); break;
            case Tipo.Gema:
                Juego.Instancia.SumarPuntos(100);
                Juego.Instancia.Audio.Sonar("sfx_gem");
                break;
            case Tipo.Corazon: Juego.Instancia.SumarVida(); break;
        }
        Destroy(gameObject);
    }
}
