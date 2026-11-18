
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class PetalChest : MonoBehaviour, IInteractable
{
    [Header("Configuración")]
    [SerializeField] private KeyCode _interactionKey = KeyCode.E;

    [Header("Recompensas")]
    [SerializeField] private float _timeReward = 15f;
    [SerializeField] private int _seedReward = 5;

    [Header("Sonido")]
    [SerializeField] private AudioClip _openSound;
    [Tooltip("Suena ademas cuando la recompensa es tiempo (reloj de arena).")]
    [SerializeField] private AudioClip _timeRewardSound;
    [Tooltip("Suena ademas cuando la recompensa son semillas.")]
    [SerializeField] private AudioClip _seedRewardSound;
    [SerializeField, Range(0f, 1f)] private float _openVolume = 1f;

    [Header("Efecto visual")]
    [SerializeField] private float _pulseAmount = 0.035f;
    [SerializeField] private float _pulseSpeed = 2.5f;
    

    private bool _playerNearby;
    private bool _isOpened;

    private Vector3 _initialScale;

    private DreamTimer _playerDreamTimer;
    private PlayerCombat _playerCombat;
    private Light _chestLight;
    private float _initialLightIntensity;
    [SerializeField] private RewardNotification _rewardNotification;


    private void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;

        // Guardamos la escala original del cofre.
        _initialScale = transform.localScale;
        _chestLight = GetComponentInChildren<Light>();
       

    if (_chestLight != null)
    _initialLightIntensity = _chestLight.intensity;
    }

    private void Update()
    {
        if (_isOpened)
            return;

        // Efecto de latido mientras el cofre no fue utilizado.
        float pulse = 1f +
            Mathf.Sin(Time.time * _pulseSpeed) * _pulseAmount;

        transform.localScale = _initialScale * pulse;
        if (_chestLight != null)
    _chestLight.intensity = _initialLightIntensity * pulse;

        if (_playerNearby && Input.GetKeyDown(_interactionKey))
        {
            Interact();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        _playerNearby = true;

        _playerDreamTimer = other.GetComponentInParent<DreamTimer>();
        _playerCombat = other.GetComponentInParent<PlayerCombat>();

        Debug.Log("Presioná E para abrir el cofre.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        _playerNearby = false;
        _playerDreamTimer = null;
        _playerCombat = null;
    }

    
public void Interact()
{
    if (_isOpened)
        return;

    // Elegimos una de las dos recompensas al azar.
    int reward = Random.Range(0, 2);

    if (reward == 0)
    {
        if (_playerDreamTimer == null)
        {
            Debug.LogWarning("No se encontró DreamTimer en Nena.");
            return;
        }

        _playerDreamTimer.AddTime(_timeReward);
        Sfx.Play2D(_timeRewardSound, _openVolume);

        Debug.Log("¡Encontraste un reloj de arena! +15 segundos.");

        if (_rewardNotification != null)
            _rewardNotification.Show($"+{_timeReward:0} segundos de sueño");
    }
    else
    {
        if (_playerCombat == null)
        {
            Debug.LogWarning("No se encontró PlayerCombat en Nena.");
            return;
        }

        _playerCombat.AddSeeds(_seedReward);
        Sfx.Play2D(_seedRewardSound, _openVolume);

        Debug.Log("¡Encontraste semillas! +5 semillas.");

        if (_rewardNotification != null)
            _rewardNotification.Show($"+{_seedReward} semillas");
    }

    if (_chestLight != null)
        _chestLight.enabled = false;

    _isOpened = true;
    Sfx.Play2D(_openSound, _openVolume);

    // Detenemos el latido y recuperamos la escala original.
    transform.localScale = _initialScale;

    Debug.Log("¡Cofre utilizado!");
}

}
