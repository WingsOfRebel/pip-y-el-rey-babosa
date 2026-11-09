using UnityEngine;

// Animación cuadro por cuadro sencilla: va cambiando el sprite (alas de la mosca, bandera, etc.).
[RequireComponent(typeof(SpriteRenderer))]
public class AnimadorSprites : MonoBehaviour
{
    public Sprite[] m_Cuadros;
    public float m_CuadrosPorSegundo = 6f;

    private SpriteRenderer m_Sr;
    private float m_Reloj;

    private void Awake()
    {
        m_Sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (m_Cuadros == null || m_Cuadros.Length == 0) return;
        m_Reloj += Time.deltaTime;
        m_Sr.sprite = m_Cuadros[(int)(m_Reloj * m_CuadrosPorSegundo) % m_Cuadros.Length];
    }
}
