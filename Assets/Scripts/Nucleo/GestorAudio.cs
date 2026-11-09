using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Música de fondo y efectos. Los sonidos se cargan desde Assets/Resources/Audio.
public class GestorAudio : MonoBehaviour
{
    private AudioSource m_Musica;
    private AudioSource m_Efectos;
    private string m_MusicaActual;
    private readonly Dictionary<string, AudioClip> m_Cache = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        m_Musica = gameObject.AddComponent<AudioSource>();
        m_Musica.loop = true;
        m_Musica.playOnAwake = false;
        m_Musica.volume = 0.45f;

        m_Efectos = gameObject.AddComponent<AudioSource>();
        m_Efectos.playOnAwake = false;
        m_Efectos.volume = 0.8f;
    }

    private void Update()
    {
        // En el navegador el audio queda bloqueado hasta que el usuario toca algo.
        // Si la música no arrancó, se intenta de nuevo con la primera tecla o clic.
        if (m_Musica.clip != null && !m_Musica.isPlaying)
        {
            bool tecla = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
            bool clic = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            if (tecla || clic) m_Musica.Play();
        }
    }

    private AudioClip Clip(string nombre)
    {
        if (!m_Cache.TryGetValue(nombre, out var clip))
        {
            clip = Resources.Load<AudioClip>("Audio/" + nombre);
            if (clip == null) Debug.LogWarning("Plataformas: no encuentro el sonido Resources/Audio/" + nombre);
            m_Cache[nombre] = clip;
        }
        return clip;
    }

    public void PonerMusica(string nombre)
    {
        if (m_MusicaActual == nombre && m_Musica.isPlaying) return;
        m_MusicaActual = nombre;
        m_Musica.clip = Clip(nombre);
        if (m_Musica.clip != null) m_Musica.Play();
    }

    public void Sonar(string nombre, float volumen = 1f)
    {
        var clip = Clip(nombre);
        if (clip != null) m_Efectos.PlayOneShot(clip, volumen);
    }
}
