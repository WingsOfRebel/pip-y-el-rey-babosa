using UnityEngine;

// Fondo con efecto parallax: se mueve más lento que la cámara y parece estar lejos.
public class Parallax : MonoBehaviour
{
    public float m_Factor = 0.6f;   // 0 = pegado al mundo, 1 = pegado a la cámara

    private Transform m_Cam;
    private Vector3 m_Inicio;

    private void Start()
    {
        if (Camera.main != null) m_Cam = Camera.main.transform;
        m_Inicio = transform.position;
    }

    private void LateUpdate()
    {
        if (m_Cam == null) return;
        transform.position = new Vector3(m_Inicio.x + m_Cam.position.x * m_Factor,
                                         m_Inicio.y + (m_Cam.position.y - 6f) * m_Factor,
                                         m_Inicio.z);
    }
}
