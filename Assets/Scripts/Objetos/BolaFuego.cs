using UnityEngine;

// Bola de fuego del jefe: vuela en arco, gira y se apaga al tocar el suelo.
// Lleva un Peligro, así que mata al jugador si lo toca.
public class BolaFuego : MonoBehaviour
{
    public float m_Vida = 5f;

    private void Start()
    {
        Destroy(gameObject, m_Vida);
    }

    private void Update()
    {
        transform.Rotate(0f, 0f, -540f * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D otro)
    {
        if (otro.gameObject.layer == LayerMask.NameToLayer("Suelo")) Destroy(gameObject);
    }
}
