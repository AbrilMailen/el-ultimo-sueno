using UnityEngine;

// Reproduce un sonido cuando la jugadora entra en esta zona (por ejemplo, al cruzar el puente).
// Se usa poniendolo en un objeto vacio con un Collider; el Collider se pone como Trigger solo.
[RequireComponent(typeof(Collider))]
public class SoundTrigger : MonoBehaviour
{
    [SerializeField] private AudioClip _clip;
    [SerializeField, Range(0f, 1f)] private float _volume = 1f;

    [Tooltip("Si esta activo, suena solo la primera vez que entra.")]
    [SerializeField] private bool _playOnce = true;

    [Tooltip("Segundos minimos entre dos sonidos (solo si no es 'solo una vez').")]
    [SerializeField] private float _cooldown = 2f;

    private bool _played;
    private float _nextPlayTime;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (_playOnce && _played)
            return;

        if (Time.time < _nextPlayTime)
            return;

        _played = true;
        _nextPlayTime = Time.time + _cooldown;
        Sfx.Play2D(_clip, _volume);
    }
}
