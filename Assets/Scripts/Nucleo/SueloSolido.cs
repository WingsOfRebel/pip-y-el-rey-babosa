using UnityEngine;
using UnityEngine.Tilemaps;

// Asegura que el suelo del Tilemap tenga colisión al empezar el nivel:
// procesa los tiles y vuelve a generar la forma del CompositeCollider2D.
[DefaultExecutionOrder(-100)]
public class SueloSolido : MonoBehaviour
{
    private void Awake()
    {
        var tc = GetComponent<TilemapCollider2D>();
        if (tc != null) tc.ProcessTilemapChanges();
        var comp = GetComponent<CompositeCollider2D>();
        if (comp != null) comp.GenerateGeometry();
    }
}
