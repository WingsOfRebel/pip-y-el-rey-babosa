using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

// Herramienta del editor: menú "Plataformas > Construir todo".
// Configura capas y sprites, crea los prefabs (jugador, enemigos, objetos)
// y arma las escenas Menu, Nivel1, Nivel2 y Nivel3 a partir de mapas de texto.
public static class ConstruirJuego
{
    const string Kenney = "Assets/Kenney";
    const string Generado = "Assets/Generado";
    const string Prefabs = Generado + "/Prefabs";
    const string Tiles = Generado + "/Tiles";
    const string Plantilla = "Assets/Scenes/SampleScene.unity";

    static readonly StringBuilder s_Reporte = new StringBuilder();

    // ------------------------------------------------------------------
    // Mapas (cada carácter es una casilla; la última fila es el suelo)
    //  #  suelo            =  plataforma       B  ladrillo
    //  ?  bloque moneda    H  bloque vida      o  moneda   g  gema   h  corazón
    //  P  jugador          F  meta (bandera)   S  resorte
    //  s  babosa           c  caracol          f  mosca    r  rana
    //  x  babosa de púas   w  sierra (lado)    W  sierra (arriba/abajo)
    //  ^  púas             L  lava (borde)     l  lava     J  jefe
    //  *  arbusto  k  cactus  t  antorcha  (decoración)
    // ------------------------------------------------------------------
    private static readonly string[] MapaNivel1 =
    {
        "",
        "",
        "",
        "",
        "",
        "",
        "                                 g                                    g",
        "                                                  ooo                                          ===",
        "        ?B?H?                  ooooo                     f   ?B?     ===                      #",
        "                         ooo                                                   oooo          ##",
        "               oooo            =====           ###                ===                       ###",
        "  P  *           *   s              *  c   ^^  ###        *     s         s           c *  ####    ^    F",
        "#########################   ######################   ###########################  ##############################",
        "#########################   ######################   ###########################  ##############################",
    };

    private static readonly string[] MapaNivel2 =
    {
        "",
        "",
        "",
        "",
        "                                              g",
        "",
        "                                                                                                                      oo",
        "                                              ##     ooooo                                             f             #",
        "                    oooo           ??B        ##              f          ooo        ?H?                             ##",
        "                                              ##                                                                   ###",
        "        ooo          ==                       ##                                       W  ====                    ####",
        "  P   k       r              k^^        x r S ##       w k        x             x  k  r         ^^          r k  #####       F",
        "####################    #################################################   ##########################   ###########################",
        "####################    #################################################   ##########################   ###########################",
    };

    private static readonly string[] MapaNivel3 =
    {
        "                                                                                               #",
        "                                                                                               #",
        "                                                                                               #",
        "                                                                                               #",
        "                                                                                               #",
        "                                                                                               #",
        "                                                                                               #",
        "                         ooooo                                                                 #",
        "                                BHB                                                            #",
        "                            ==                      ====                ====                   #",
        "    t     ==          t  ==             t     h             t                   t              #",
        "  P             x   w               ^                                         J             F  #",
        "#########LLLL###########LLLLLL##################################################################",
        "#########llll###########llllll##################################################################",
    };

    class DefNivel
    {
        public string escena, nombre, tema, fondo;
        public int numero;
        public float tiempo;
        public bool jefe;
        public string[] mapa;
        public Color cielo, tinte;
    }

    [MenuItem("Plataformas/Construir todo")]
    public static void ConstruirTodo()
    {
        s_Reporte.Clear();
        if (!File.Exists(Plantilla))
        {
            EditorUtility.DisplayDialog("Plataformas", "No encuentro " + Plantilla + " (la escena de ejemplo de la plantilla 2D).", "OK");
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Carpeta(Generado); Carpeta(Prefabs); Carpeta(Tiles);
        CrearCapas();
        ConfigurarSprites();
        var prefabs = CrearPrefabs();

        var cielo = new Color32(195, 227, 255, 255);
        var niveles = new[]
        {
            new DefNivel { escena = "Nivel1", numero = 1, nombre = "Valle Verde", tema = "grass", fondo = "hills", tiempo = 150, mapa = MapaNivel1, cielo = cielo, tinte = Color.white },
            new DefNivel { escena = "Nivel2", numero = 2, nombre = "Desierto Dorado", tema = "sand", fondo = "desert", tiempo = 170, mapa = MapaNivel2, cielo = cielo, tinte = Color.white },
            new DefNivel { escena = "Nivel3", numero = 3, nombre = "Castillo del Rey Babosa", tema = "stone", fondo = "mushrooms", tiempo = 180, jefe = true, mapa = MapaNivel3,
                           cielo = new Color32(48, 40, 70, 255), tinte = new Color(0.55f, 0.5f, 0.7f) },
        };

        CrearMenu();
        foreach (var n in niveles) CrearNivel(n, prefabs);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/Menu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Nivel1.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Nivel2.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Nivel3.unity", true),
        };
        Log("Escenas añadidas a Build Settings: Menu, Nivel1, Nivel2, Nivel3");

        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene("Assets/Scenes/Menu.unity", OpenSceneMode.Single);
        File.WriteAllText("Plataformas_construccion.txt", s_Reporte.ToString());
        Log("LISTO. Abre la escena Menu y dale a Play.");
    }

    static void Log(string texto)
    {
        s_Reporte.AppendLine(texto);
        Debug.Log("Plataformas: " + texto);
    }

    static void Carpeta(string ruta)
    {
        if (AssetDatabase.IsValidFolder(ruta)) return;
        string padre = Path.GetDirectoryName(ruta).Replace('\\', '/');
        Carpeta(padre);
        AssetDatabase.CreateFolder(padre, Path.GetFileName(ruta));
    }

    static int Capa(string nombre) => LayerMask.NameToLayer(nombre);

    // ---------------- Capas ----------------

    static void CrearCapas()
    {
        var tm = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var capas = tm.FindProperty("layers");
        foreach (var nombre in new[] { "Suelo", "Jugador", "Enemigos", "Golpe" })
        {
            bool existe = false;
            for (int i = 0; i < capas.arraySize; i++)
                if (capas.GetArrayElementAtIndex(i).stringValue == nombre) existe = true;
            if (existe) continue;
            for (int i = 8; i < capas.arraySize; i++)
            {
                var p = capas.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(p.stringValue)) { p.stringValue = nombre; break; }
            }
        }
        tm.ApplyModifiedPropertiesWithoutUndo();

        int jugador = Capa("Jugador"), enemigos = Capa("Enemigos"), golpe = Capa("Golpe"), suelo = Capa("Suelo");
        Physics2D.IgnoreLayerCollision(jugador, enemigos, true);
        Physics2D.IgnoreLayerCollision(enemigos, enemigos, true);
        Physics2D.IgnoreLayerCollision(golpe, suelo, true);
        Physics2D.IgnoreLayerCollision(golpe, enemigos, true);
        Physics2D.IgnoreLayerCollision(golpe, golpe, true);
        Log("Capas: Suelo=" + suelo + " Jugador=" + jugador + " Enemigos=" + enemigos + " Golpe=" + golpe);
    }

    // ---------------- Sprites ----------------

    static void ConfigurarSprites()
    {
        int cambiados = 0;
        var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { Kenney });
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var g in guids)
            {
                string ruta = AssetDatabase.GUIDToAssetPath(g);
                var ti = AssetImporter.GetAtPath(ruta) as TextureImporter;
                if (ti == null) continue;

                bool fondo = ruta.Contains("/Backgrounds/");
                // Personajes y enemigos con el punto de apoyo en los pies (menos la sierra, que gira)
                bool pies = (ruta.Contains("/Characters/") || ruta.Contains("/Enemies/")) && !ruta.Contains("/saw");
                int alineacion = pies ? (int)SpriteAlignment.BottomCenter : (int)SpriteAlignment.Center;

                var s = new TextureImporterSettings();
                ti.ReadTextureSettings(s);
                bool igual = ti.textureType == TextureImporterType.Sprite && ti.spriteImportMode == SpriteImportMode.Single &&
                             Mathf.Approximately(ti.spritePixelsPerUnit, 64f) && s.spriteAlignment == alineacion && !ti.mipmapEnabled &&
                             ti.textureCompression == TextureImporterCompression.Uncompressed &&
                             (!fondo || (s.spriteMeshType == SpriteMeshType.FullRect && ti.wrapMode == TextureWrapMode.Repeat));
                if (igual) continue;

                ti.textureType = TextureImporterType.Sprite;
                ti.spriteImportMode = SpriteImportMode.Single;
                ti.spritePixelsPerUnit = 64f;
                ti.mipmapEnabled = false;
                ti.filterMode = FilterMode.Bilinear;
                ti.textureCompression = TextureImporterCompression.Uncompressed;
                ti.ReadTextureSettings(s);
                s.spriteAlignment = alineacion;
                s.spriteMeshType = fondo ? SpriteMeshType.FullRect : SpriteMeshType.Tight;
                ti.SetTextureSettings(s);
                if (fondo) ti.wrapMode = TextureWrapMode.Repeat;
                ti.SaveAndReimport();
                cambiados++;
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
        AssetDatabase.Refresh();
        Log("Sprites revisados: " + guids.Length + ", configurados ahora: " + cambiados + " (64 píxeles = 1 unidad)");
    }

    static Sprite S(string carpeta, string nombre)
    {
        var sp = AssetDatabase.LoadAssetAtPath<Sprite>(Kenney + "/" + carpeta + "/" + nombre + ".png");
        if (sp == null) Log("AVISO: falta el sprite " + carpeta + "/" + nombre);
        return sp;
    }

    static Tile T(string nombre)
    {
        string ruta = Tiles + "/" + nombre + ".asset";
        var t = AssetDatabase.LoadAssetAtPath<Tile>(ruta);
        if (t == null)
        {
            t = ScriptableObject.CreateInstance<Tile>();
            t.sprite = S("Tiles", nombre);
            t.colliderType = Tile.ColliderType.Grid;
            AssetDatabase.CreateAsset(t, ruta);
        }
        return t;
    }

    // ---------------- Prefabs ----------------

    static GameObject NuevoSprite(string nombre, Sprite sprite, int orden)
    {
        var go = new GameObject(nombre);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = orden;
        return go;
    }

    static GameObject Guardar(GameObject go)
    {
        var p = PrefabUtility.SaveAsPrefabAsset(go, Prefabs + "/" + go.name + ".prefab");
        Object.DestroyImmediate(go);
        return p;
    }

    static Rigidbody2D Cuerpo(GameObject go, RigidbodyType2D tipo, float gravedad)
    {
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = tipo;
        rb.gravityScale = gravedad;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        return rb;
    }

    static BoxCollider2D Caja(GameObject go, Vector2 tam, Vector2 desplazamiento, bool trigger)
    {
        var c = go.AddComponent<BoxCollider2D>();
        c.size = tam;
        c.offset = desplazamiento;
        c.isTrigger = trigger;
        return c;
    }

    // Zona de golpe del enemigo (trigger en la capa Golpe, la única que toca al jugador)
    static void ZonaGolpe(GameObject padre, Vector2 tam, Vector2 desplazamiento)
    {
        var g = new GameObject("Golpe");
        g.layer = Capa("Golpe");
        g.transform.SetParent(padre.transform, false);
        Caja(g, tam, desplazamiento, true);
    }

    static AnimadorSprites Animar(GameObject go, float fps, params Sprite[] cuadros)
    {
        var a = go.AddComponent<AnimadorSprites>();
        a.m_Cuadros = cuadros;
        a.m_CuadrosPorSegundo = fps;
        return a;
    }

    static GameObject Caminante(string nombre, string sprite, float velocidad, bool pisable, int puntos,
                                Vector2 cuerpo, Vector2 golpe, string aplastado)
    {
        var go = NuevoSprite(nombre, S("Enemies", sprite + "_walk_a"), 6);
        go.layer = Capa("Enemigos");
        Cuerpo(go, RigidbodyType2D.Dynamic, 3f);
        CajaEnemigo(go, cuerpo);
        ZonaGolpe(go, golpe, new Vector2(0, golpe.y / 2f));
        Animar(go, 5f, S("Enemies", sprite + "_walk_a"), S("Enemies", sprite + "_walk_b"));
        var e = go.AddComponent<EnemigoCaminante>();
        e.m_Velocidad = velocidad;
        e.m_Pisable = pisable;
        e.m_Puntos = puntos;
        e.m_SpriteAplastado = S("Enemies", aplastado);
        return Guardar(go);
    }

    static PhysicsMaterial2D s_SinFriccion;

    // Cuerpo de enemigo: sin fricción y con bordes redondeados para que no se trabe
    static BoxCollider2D CajaEnemigo(GameObject go, Vector2 tam)
    {
        var c = Caja(go, tam - new Vector2(0.06f, 0.06f), new Vector2(0f, tam.y / 2f), false);
        c.edgeRadius = 0.03f;
        c.sharedMaterial = s_SinFriccion;
        return c;
    }

    static Dictionary<char, GameObject> CrearPrefabs()
    {
        var p = new Dictionary<char, GameObject>();
        var sinFriccion = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(Generado + "/SinFriccion.physicsMaterial2D");
        if (sinFriccion == null)
        {
            sinFriccion = new PhysicsMaterial2D("SinFriccion") { friction = 0f, bounciness = 0f };
            AssetDatabase.CreateAsset(sinFriccion, Generado + "/SinFriccion.physicsMaterial2D");
        }
        s_SinFriccion = sinFriccion;

        // Jugador (Pip)
        {
            var go = NuevoSprite("Jugador", S("Characters", "character_green_idle"), 10);
            go.layer = Capa("Jugador");
            go.tag = "Player";
            Cuerpo(go, RigidbodyType2D.Dynamic, 3.5f);
            var c = Caja(go, new Vector2(0.74f, 1.34f), new Vector2(0f, 0.7f), false);
            c.edgeRadius = 0.03f;          // bordes redondeados: no se traba entre casillas
            c.sharedMaterial = sinFriccion;
            var j = go.AddComponent<Jugador>();
            j.m_Quieto = S("Characters", "character_green_idle");
            j.m_Caminar1 = S("Characters", "character_green_walk_a");
            j.m_Caminar2 = S("Characters", "character_green_walk_b");
            j.m_Saltar = S("Characters", "character_green_jump");
            j.m_Golpeado = S("Characters", "character_green_hit");
            p['P'] = Guardar(go);
        }

        // Enemigos que caminan
        p['s'] = Caminante("Babosa", "slime_normal", 2f, true, 100, new Vector2(0.8f, 0.6f), new Vector2(0.85f, 0.65f), "slime_normal_flat");
        p['c'] = Caminante("Caracol", "snail", 1.2f, true, 150, new Vector2(0.9f, 0.7f), new Vector2(0.9f, 0.75f), "snail_shell");
        p['x'] = Caminante("BabosaPuas", "slime_spike", 1.6f, false, 0, new Vector2(0.8f, 0.6f), new Vector2(0.85f, 0.85f), "slime_spike_flat");

        // Mosca (vuela, sin gravedad)
        {
            var go = NuevoSprite("Mosca", S("Enemies", "fly_a"), 6);
            go.layer = Capa("Enemigos");
            Cuerpo(go, RigidbodyType2D.Kinematic, 0f);
            ZonaGolpe(go, new Vector2(0.7f, 0.6f), new Vector2(0f, 0.5f));
            Animar(go, 12f, S("Enemies", "fly_a"), S("Enemies", "fly_b"));
            var e = go.AddComponent<EnemigoVolador>();
            e.m_Puntos = 150;
            e.m_SpriteAplastado = S("Enemies", "fly_rest");
            p['f'] = Guardar(go);
        }

        // Rana (salta hacia el jugador)
        {
            var go = NuevoSprite("Rana", S("Enemies", "frog_idle"), 6);
            go.layer = Capa("Enemigos");
            Cuerpo(go, RigidbodyType2D.Dynamic, 3f);
            CajaEnemigo(go, new Vector2(0.8f, 0.7f));
            ZonaGolpe(go, new Vector2(0.85f, 0.75f), new Vector2(0f, 0.38f));
            var e = go.AddComponent<EnemigoSaltador>();
            e.m_Puntos = 200;
            e.m_SpriteQuieta = S("Enemies", "frog_idle");
            e.m_SpriteSaltando = S("Enemies", "frog_jump");
            e.m_SpriteAplastado = S("Enemies", "frog_rest");
            p['r'] = Guardar(go);
        }

        // Sierra (obstáculo que va y viene, no se puede pisar)
        {
            var go = NuevoSprite("Sierra", S("Enemies", "saw_a"), 5);
            Cuerpo(go, RigidbodyType2D.Kinematic, 0f);
            var c = go.AddComponent<CircleCollider2D>();
            c.radius = 0.42f;
            c.isTrigger = true;
            go.AddComponent<Peligro>();
            Animar(go, 10f, S("Enemies", "saw_a"), S("Enemies", "saw_b"));
            go.AddComponent<MovimientoVaiven>();
            p['w'] = Guardar(go);
        }

        // Púas
        {
            var go = NuevoSprite("Puas", S("Tiles", "spikes"), 4);
            Caja(go, new Vector2(0.8f, 0.35f), new Vector2(0f, -0.25f), true);
            go.AddComponent<Peligro>();
            p['^'] = Guardar(go);
        }

        // Bola de fuego del jefe
        GameObject bola;
        {
            var go = NuevoSprite("BolaFuego", S("Tiles", "fireball"), 7);
            var rb = Cuerpo(go, RigidbodyType2D.Dynamic, 0.6f);
            rb.freezeRotation = false;
            var c = go.AddComponent<CircleCollider2D>();
            c.radius = 0.25f;
            c.isTrigger = true;
            go.AddComponent<Peligro>();
            go.AddComponent<BolaFuego>();
            bola = Guardar(go);
        }

        // Jefe: Rey Babosa (babosa de fuego gigante)
        {
            var go = NuevoSprite("Jefe", S("Enemies", "slime_fire_walk_a"), 6);
            go.layer = Capa("Enemigos");
            go.transform.localScale = new Vector3(2.5f, 2.5f, 1f);
            var rb = Cuerpo(go, RigidbodyType2D.Dynamic, 3f);
            rb.mass = 10f;
            CajaEnemigo(go, new Vector2(0.8f, 0.6f));
            ZonaGolpe(go, new Vector2(0.85f, 0.68f), new Vector2(0f, 0.34f));
            var e = go.AddComponent<Jefe>();
            e.m_Caminar = new[] { S("Enemies", "slime_fire_walk_a"), S("Enemies", "slime_fire_walk_b") };
            e.m_SpriteAplastado = S("Enemies", "slime_fire_flat");
            e.m_PrefabBolaFuego = bola;
            p['J'] = Guardar(go);
        }

        // Objetos para recoger
        p['o'] = CrearRecogible("Moneda", Recogible.Tipo.Moneda, "coin_gold", "coin_gold_side");
        p['g'] = CrearRecogible("Gema", Recogible.Tipo.Gema, "gem_blue", null);
        p['h'] = CrearRecogible("Corazon", Recogible.Tipo.Corazon, "heart", null);

        // Bloques
        p['?'] = Bloque("BloqueMoneda", "block_coin", false, "coin_gold");
        p['H'] = Bloque("BloqueVida", "block_exclamation", true, "heart");
        {
            var go = NuevoSprite("Ladrillo", S("Tiles", "bricks_brown"), 3);
            go.layer = Capa("Suelo");
            Caja(go, Vector2.one, Vector2.zero, false);
            p['B'] = Guardar(go);
        }

        // Resorte
        {
            var go = NuevoSprite("Resorte", S("Tiles", "spring"), 4);
            Caja(go, new Vector2(0.8f, 0.4f), new Vector2(0f, -0.05f), true);
            var r = go.AddComponent<Resorte>();
            r.m_SpriteNormal = S("Tiles", "spring");
            r.m_SpriteEstirado = S("Tiles", "spring_out");
            p['S'] = Guardar(go);
        }

        // Meta (bandera)
        {
            var go = NuevoSprite("Meta", S("Tiles", "flag_green_a"), 4);
            Caja(go, new Vector2(0.8f, 3f), new Vector2(0f, 1f), true);
            Animar(go, 4f, S("Tiles", "flag_green_a"), S("Tiles", "flag_green_b"));
            go.AddComponent<Meta>();
            p['F'] = Guardar(go);
        }

        Log("Prefabs creados en " + Prefabs + ": " + p.Count + " tipos + BolaFuego");
        return p;
    }

    static GameObject CrearRecogible(string nombre, Recogible.Tipo tipo, string sprite, string spriteGiro)
    {
        var go = NuevoSprite(nombre, S("Tiles", sprite), 4);
        var c = go.AddComponent<CircleCollider2D>();
        c.radius = 0.32f;
        c.isTrigger = true;
        go.AddComponent<Recogible>().m_Tipo = tipo;
        if (spriteGiro != null) Animar(go, 4f, S("Tiles", sprite), S("Tiles", spriteGiro));
        return Guardar(go);
    }

    static GameObject Bloque(string nombre, string sprite, bool daVida, string premio)
    {
        var go = NuevoSprite(nombre, S("Tiles", sprite), 3);
        go.layer = Capa("Suelo");
        Caja(go, Vector2.one, Vector2.zero, false);
        var b = go.AddComponent<BloqueSorpresa>();
        b.m_DaVida = daVida;
        b.m_SpriteVacio = S("Tiles", "block_empty");
        b.m_SpritePremio = S("Tiles", premio);
        return Guardar(go);
    }

    // ---------------- Escenas ----------------

    static Scene NuevaEscena(string nombre, Color cielo, out Camera cam)
    {
        string ruta = "Assets/Scenes/" + nombre + ".unity";
        AssetDatabase.DeleteAsset(ruta);
        AssetDatabase.CopyAsset(Plantilla, ruta);
        var escena = EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);
        cam = Object.FindAnyObjectByType<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = cielo;
        return escena;
    }

    static Tilemap NuevoTilemap(Transform grid, string nombre, int orden)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(grid, false);
        var tm = go.AddComponent<Tilemap>();
        go.AddComponent<TilemapRenderer>().sortingOrder = orden;
        return tm;
    }

    static void Fondos(string fondo, float ancho, Color tinte, bool parallax)
    {
        CapaFondo("background_fade_" + fondo, -20, 0.8f, ancho, tinte, parallax);
        CapaFondo("background_color_" + fondo, -10, 0.55f, ancho, tinte, parallax);
    }

    static void CapaFondo(string sprite, int orden, float factor, float ancho, Color tinte, bool parallax)
    {
        var go = NuevoSprite("Fondo_" + sprite, S("Backgrounds", sprite), orden);
        var sr = go.GetComponent<SpriteRenderer>();
        sr.drawMode = SpriteDrawMode.Tiled;
        float total = ancho * 0.5f + 40f;                  // ancho en el mundo
        sr.size = new Vector2(total / 3f, 4f);             // el sprite mide 4 unidades; escala 3 = 12
        sr.color = tinte;
        go.transform.localScale = new Vector3(3f, 3f, 1f);
        go.transform.position = new Vector3(parallax ? ancho * 0.2f : ancho * 0.5f, 6f, 5f);
        if (parallax) go.AddComponent<Parallax>().m_Factor = factor;
    }

    static string Suelo(string tema, string pieza) => "terrain_" + tema + "_" + pieza;

    static void CrearNivel(DefNivel def, Dictionary<char, GameObject> prefabs)
    {
        var escena = NuevaEscena(def.escena, def.cielo, out var cam);
        cam.gameObject.AddComponent<SeguirCamara>();

        string[] filas = def.mapa;
        int alto = filas.Length, ancho = 0;
        foreach (var f in filas) ancho = Mathf.Max(ancho, f.Length);
        char C(int x, int y)
        {
            if (y < 0 || y >= alto || x < 0) return ' ';
            string fila = filas[alto - 1 - y];
            return x < fila.Length ? fila[x] : ' ';
        }

        // Nivel (reloj y límites)
        var nivelGO = new GameObject("Nivel");
        var nivel = nivelGO.AddComponent<Nivel>();
        nivel.m_Numero = def.numero;
        nivel.m_Nombre = def.nombre;
        nivel.m_TiempoLimite = def.tiempo;
        nivel.m_EsJefe = def.jefe;
        nivel.m_MinX = 0f;
        nivel.m_MaxX = ancho;
        nivel.m_MinY = 0f;
        nivel.m_LimiteCaida = -2f;

        Fondos(def.fondo, ancho, def.tinte, true);

        // Tilemaps
        var grid = new GameObject("Mapa", typeof(Grid)).transform;
        var suelo = NuevoTilemap(grid, "Suelo", 0);
        suelo.gameObject.layer = Capa("Suelo");
        // Colisión del suelo unida en una sola forma (así nada se traba entre casillas)
        var rb = suelo.gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
        var comp = suelo.gameObject.AddComponent<CompositeCollider2D>();
        comp.geometryType = CompositeCollider2D.GeometryType.Polygons;
        var tc = suelo.gameObject.AddComponent<TilemapCollider2D>();
        tc.compositeOperation = Collider2D.CompositeOperation.Merge;

        var deco = NuevoTilemap(grid, "Decoracion", -1);
        var lava = NuevoTilemap(grid, "Lava", 1);
        var lavaCol = lava.gameObject.AddComponent<TilemapCollider2D>();
        lavaCol.isTrigger = true;
        lava.gameObject.AddComponent<Peligro>();

        var enemigos = new GameObject("Enemigos").transform;
        var objetos = new GameObject("Objetos").transform;
        int cuentaEnemigos = 0, cuentaObjetos = 0;
        Vector3 inicioJugador = new Vector3(2.5f, 2f, 0f);

        for (int y = 0; y < alto; y++)
        {
            for (int x = 0; x < ancho; x++)
            {
                char c = C(x, y);
                var celda = new Vector3Int(x, y, 0);
                switch (c)
                {
                    case ' ': break;
                    case '#':
                    {
                        bool arriba = C(x, y + 1) == '#', izq = C(x - 1, y) == '#', der = C(x + 1, y) == '#';
                        string pieza;
                        if (!arriba) pieza = !izq && der ? "block_top_left" : izq && !der ? "block_top_right" : "block_top";
                        else pieza = !izq && der ? "block_left" : izq && !der ? "block_right" : "block_center";
                        suelo.SetTile(celda, T(Suelo(def.tema, pieza)));
                        break;
                    }
                    case '=':
                    {
                        bool izq = C(x - 1, y) == '=', der = C(x + 1, y) == '=';
                        string pieza = !izq && der ? "horizontal_left" : izq && !der ? "horizontal_right" : "horizontal_middle";
                        suelo.SetTile(celda, T(Suelo(def.tema, pieza)));
                        break;
                    }
                    case 'L': lava.SetTile(celda, T("lava_top")); break;
                    case 'l': lava.SetTile(celda, T("lava")); break;
                    case '*': deco.SetTile(celda, T("bush")); break;
                    case 'k': deco.SetTile(celda, T("cactus")); break;
                    case 't': deco.SetTile(celda, T("torch_on_a")); break;
                    default:
                    {
                        char clave = c == 'W' ? 'w' : c;
                        if (!prefabs.TryGetValue(clave, out var prefab)) { Log("AVISO: carácter desconocido '" + c + "' en " + def.escena); break; }

                        // Personajes y enemigos se apoyan en los pies; lo demás va centrado en la casilla
                        bool pies = "PscxrfJ".IndexOf(c) >= 0;
                        var pos = new Vector3(x + 0.5f, pies ? y : y + 0.5f, 0f);
                        if (c == 'P') { inicioJugador = pos; }

                        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, escena);
                        go.transform.position = pos;
                        bool esEnemigo = "scxrfJwW^".IndexOf(c) >= 0;
                        if (c != 'P') go.transform.SetParent(esEnemigo ? enemigos : objetos, true);
                        if (esEnemigo) cuentaEnemigos++; else if (c != 'P') cuentaObjetos++;

                        if (c == 'w' || c == 'W')
                        {
                            var mv = go.GetComponent<MovimientoVaiven>();
                            mv.m_Direccion = c == 'w' ? Vector2.right : Vector2.up;
                            mv.m_Distancia = c == 'w' ? 2f : 1.5f;
                        }
                        break;
                    }
                }
            }
        }

        // Generar ya la colisión del suelo (si no, el CompositeCollider2D se guarda vacío)
        suelo.gameObject.AddComponent<SueloSolido>();
        tc.ProcessTilemapChanges();
        comp.GenerateGeometry();
        Log(def.escena + ": formas de colisión del suelo = " + comp.pathCount + " (debe ser mayor que 0)");

        // Unir al jefe con su arena y con la bandera
        var jefe = Object.FindAnyObjectByType<Jefe>();
        var meta = Object.FindAnyObjectByType<Meta>();
        if (jefe != null)
        {
            jefe.m_MinX = 44f;
            jefe.m_MaxX = ancho - 6f;
            jefe.m_Meta = meta;
            EditorUtility.SetDirty(jefe);
        }

        cam.transform.position = new Vector3(inicioJugador.x + 8f, 6f, -10f);
        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        Log(def.escena + " (" + def.nombre + "): " + ancho + "x" + alto + " casillas, " + cuentaEnemigos + " enemigos/obstáculos, " + cuentaObjetos + " objetos" + (jefe != null ? ", con jefe" : ""));
    }

    static void CrearMenu()
    {
        var escena = NuevaEscena("Menu", new Color32(195, 227, 255, 255), out var cam);
        cam.transform.position = new Vector3(11f, 6f, -10f);

        Fondos("hills", 22f, Color.white, false);

        var grid = new GameObject("Mapa", typeof(Grid)).transform;
        var suelo = NuevoTilemap(grid, "Suelo", 0);
        for (int x = -4; x < 28; x++)
        {
            suelo.SetTile(new Vector3Int(x, 1, 0), T(Suelo("grass", "block_top")));
            suelo.SetTile(new Vector3Int(x, 0, 0), T(Suelo("grass", "block_center")));
        }

        // Decoración: el personaje, algunos enemigos y monedas (solo dibujos, sin scripts)
        void Dibujo(string carpeta, string sprite, float x, float y, bool voltear = false)
        {
            var go = NuevoSprite(sprite, S(carpeta, sprite), 5);
            go.transform.position = new Vector3(x, y, 0f);
            go.GetComponent<SpriteRenderer>().flipX = voltear;
        }
        Dibujo("Characters", "character_green_jump", 4f, 3.2f);
        Dibujo("Enemies", "slime_normal_rest", 15f, 2f);
        Dibujo("Enemies", "snail_rest", 17f, 2f);
        Dibujo("Enemies", "fly_a", 7.5f, 5f);
        Dibujo("Enemies", "slime_fire_rest", 20.5f, 2f);
        Dibujo("Tiles", "flag_green_a", 1.5f, 2.5f);
        for (int i = 0; i < 4; i++) Dibujo("Tiles", "coin_gold", 9.5f + i, 3.5f);

        new GameObject("Menu").AddComponent<MenuPrincipal>();

        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        Log("Menu creado");
    }
}
