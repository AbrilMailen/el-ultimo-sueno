using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Feedback cuando la niña recibe un golpe: parpadeo rojo, sonido y destello en pantalla.
// Solo escucha al DreamTimer; no modifica el tiempo.
public class PlayerHitFeedback : MonoBehaviour
{
    [SerializeField] private DreamTimer _timer;

    [Header("Parpadeo")]
    [SerializeField] private Color _flashColor = new Color(1f, 0.25f, 0.25f);
    [SerializeField] private float _flashTime = 0.15f;

    [Header("Sonido")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _hitSound;

    [Header("Destello en pantalla (opcional)")]
    [Tooltip("Una Image roja a pantalla completa, con alpha 0 y Raycast Target apagado.")]
    [SerializeField] private Image _screenFlash;
    [SerializeField, Range(0f, 1f)] private float _screenFlashAlpha = 0.35f;
    [SerializeField] private float _screenFlashTime = 0.25f;

    private Renderer[] _renderers;
    private Coroutine _flashRoutine;
    private Coroutine _screenRoutine;

    private void Awake()
    {
        if (_timer == null)
            _timer = GetComponent<DreamTimer>();

        _renderers = GetComponentsInChildren<Renderer>();
    }

    private void OnEnable()
    {
        if (_timer != null)
            _timer.OnDamaged += HandleDamaged;
    }

    private void OnDisable()
    {
        if (_timer != null)
            _timer.OnDamaged -= HandleDamaged;
    }

    private void HandleDamaged(float seconds)
    {
        if (_audioSource != null && _hitSound != null)
            _audioSource.PlayOneShot(_hitSound);
        else if (_hitSound != null)
            AudioSource.PlayClipAtPoint(_hitSound, transform.position);

        if (_flashRoutine != null)
            StopCoroutine(_flashRoutine);
        _flashRoutine = StartCoroutine(FlashRoutine());

        if (_screenFlash != null)
        {
            if (_screenRoutine != null)
                StopCoroutine(_screenRoutine);
            _screenRoutine = StartCoroutine(ScreenFlashRoutine());
        }
    }

    private IEnumerator FlashRoutine()
    {
        var block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", _flashColor);
        block.SetColor("_Color", _flashColor);

        foreach (Renderer rend in _renderers)
            rend.SetPropertyBlock(block);

        yield return new WaitForSeconds(_flashTime);

        foreach (Renderer rend in _renderers)
            rend.SetPropertyBlock(null);

        _flashRoutine = null;
    }

    // Usa tiempo sin escala para que el destello se vea aunque el juego se pause por derrota
    private IEnumerator ScreenFlashRoutine()
    {
        Color c = _screenFlash.color;

        for (float t = 0f; t < _screenFlashTime; t += Time.unscaledDeltaTime)
        {
            c.a = Mathf.Lerp(_screenFlashAlpha, 0f, t / _screenFlashTime);
            _screenFlash.color = c;
            yield return null;
        }

        c.a = 0f;
        _screenFlash.color = c;
        _screenRoutine = null;
    }
}
