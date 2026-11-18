
using UnityEngine;

public class ObjetoColeccionable : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private KeyCode teclaInteraccion = KeyCode.E;

    [Header("Tipo de objeto")]
    [SerializeField] private bool _esFlorSalida = false;

    [Header("Referencias")]
    [SerializeField] private LevelManager _levelManager;

    [Header("Sonido")]
    [SerializeField] private AudioClip _pickupSound;
    [SerializeField, Range(0f, 1f)] private float _pickupVolume = 1f;

    private bool _jugadorCerca;
    private bool _recogido;

    private void Update()
    {
        if (_jugadorCerca && !_recogido &&
            Input.GetKeyDown(teclaInteraccion))
        {
            RecogerObjeto();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _jugadorCerca = true;
            Debug.Log("Presioná E para recoger el objeto");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _jugadorCerca = false;
        }
    }

    private void RecogerObjeto()
    {
        if (_levelManager == null)
        {
            Debug.LogWarning(
                "Falta asignar LevelManager al objeto coleccionable."
            );
            return;
        }

        _recogido = true;
        Sfx.Play2D(_pickupSound, _pickupVolume);

        if (_esFlorSalida)
        {
            _levelManager.CollectFlower();
        }
        else
        {
            _levelManager.CollectCommonMushroom();
        }

        Debug.Log("¡Objeto coleccionado!");
        gameObject.SetActive(false);
    }
}
