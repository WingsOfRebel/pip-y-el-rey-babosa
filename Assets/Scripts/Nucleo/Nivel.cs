using UnityEngine;

// Va en cada escena de nivel. Lleva el reloj y los límites del mapa.
public class Nivel : MonoBehaviour
{
    public int m_Numero = 1;
    public string m_Nombre = "Valle Verde";
    public float m_TiempoLimite = 150f;     // segundos para llegar a la meta
    public bool m_EsJefe;                   // el nivel 3 usa la música del jefe

    [Header("Límites del mapa (los pone la herramienta)")]
    public float m_MinX;
    public float m_MaxX = 100f;
    public float m_MinY = -1f;
    public float m_LimiteCaida = -4f;       // más abajo de esto el jugador muere

    public float TiempoRestante { get; private set; }

    private bool m_Avisado;

    private void Start()
    {
        TiempoRestante = m_TiempoLimite;
        Juego.Instancia.EntrarNivel(this);
        Juego.Instancia.Hud.ActualizarReloj(TiempoRestante);
    }

    private void Update()
    {
        if (!Juego.Instancia.Jugable) return;

        TiempoRestante -= Time.deltaTime;
        if (TiempoRestante <= 0f)
        {
            TiempoRestante = 0f;
            Juego.Instancia.TiempoAgotado();
        }
        else if (!m_Avisado && TiempoRestante <= 30f)
        {
            m_Avisado = true;
            Juego.Instancia.Audio.Sonar("sfx_select");   // aviso de poco tiempo
        }
        Juego.Instancia.Hud.ActualizarReloj(TiempoRestante);
    }
}
