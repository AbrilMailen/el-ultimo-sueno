
using UnityEngine;

public class MushroomEffect : MonoBehaviour
{
    [Header("Latido")]
    [SerializeField] private float _pulseAmount = 0.08f;
    [SerializeField] private float _pulseSpeed = 2f;

    [Header("Brillo")]
    [SerializeField] private Renderer _mushroomRenderer;
    [SerializeField] private Color _glowColor = new Color(1f, 0.4f, 0.8f);
    [SerializeField] private float _glowIntensity = 1.5f;

    private Vector3 _initialScale;
    private MaterialPropertyBlock _propertyBlock;

    private static readonly int EmissionColor =
        Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        _initialScale = transform.localScale;
        _propertyBlock = new MaterialPropertyBlock();

        if (_mushroomRenderer != null)
        {
            _mushroomRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(
                EmissionColor,
                _glowColor * _glowIntensity
            );
            _mushroomRenderer.SetPropertyBlock(_propertyBlock);
        }
    }

    private void Update()
    {
        float pulse = 1f +
            Mathf.Sin(Time.time * _pulseSpeed) * _pulseAmount;

        transform.localScale = _initialScale * pulse;
    }
}
