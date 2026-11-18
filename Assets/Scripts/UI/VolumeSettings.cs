using UnityEngine;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private Slider _volumeSlider;

    private const string VolumeKey = "MasterVolume";

    private void Start()
    {
        // Recuperamos el último volumen guardado.
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);

        // Actualizamos la perilla sin disparar el evento.
        _volumeSlider.SetValueWithoutNotify(savedVolume);

        // Aplicamos el volumen guardado.
        ApplyVolume(savedVolume);

        // Escuchamos los cambios de la perilla.
        _volumeSlider.onValueChanged.AddListener(ApplyVolume);
    }

    private void ApplyVolume(float volume)
    {
        // Cambiamos el volumen global del juego.
        AudioListener.volume = volume;

        // Guardamos la preferencia.
        PlayerPrefs.SetFloat(VolumeKey, volume);
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        if (_volumeSlider != null)
            _volumeSlider.onValueChanged.RemoveListener(ApplyVolume);
    }
}
