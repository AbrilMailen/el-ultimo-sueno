
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Configuración")]
    [SerializeField] private float hoverScale = 1.08f;

    [Header("Sonidos")]
    [SerializeField] private AudioClip _hoverSound;
    [SerializeField] private AudioClip _clickSound;
    [SerializeField, Range(0f, 1f)] private float _soundVolume = 0.7f;

    private Vector3 _originalScale;

    private void Awake()
    {
        _originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = _originalScale * hoverScale;
        Sfx.Play2D(_hoverSound, _soundVolume);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = _originalScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Sfx.Play2D(_clickSound, _soundVolume);
    }

    private void OnDisable()
    {
        transform.localScale = _originalScale;
    }
}
