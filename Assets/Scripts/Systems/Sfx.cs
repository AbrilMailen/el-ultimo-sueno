using UnityEngine;

// Reproduce efectos de sonido sueltos en 2D (se oyen igual sin importar la distancia a la camara).
// Cada sonido crea un objeto temporal que se destruye al terminar, asi sigue sonando
// aunque el objeto que lo pidio se desactive (por ejemplo un coleccionable que desaparece).
public static class Sfx
{
    public static void Play2D(AudioClip clip, float volume = 1f)
    {
        if (clip == null)
            return;

        GameObject go = new GameObject("Sfx_" + clip.name);
        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = Mathf.Clamp01(volume);
        source.spatialBlend = 0f; // 2D
        source.playOnAwake = false;
        source.Play();

        Object.Destroy(go, clip.length + 0.1f);
    }
}
