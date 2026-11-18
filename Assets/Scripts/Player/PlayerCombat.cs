using System;
using System.Collections.Generic;
using UnityEngine;

// Combate de la niña: melee (clic izquierdo) y ranged (clic derecho).
// Apunta hacia donde mira el personaje (PlayerController ya lo gira hacia el mouse).
public class PlayerCombat : MonoBehaviour
{
    [Header("Melee (tallo de diente de león)")]
    [SerializeField] private float _meleeDamage = 10f;
    [SerializeField] private float _meleeRange = 1.2f;   // distancia al centro del golpe
    [SerializeField] private float _meleeRadius = 1f;
    [SerializeField] private float _meleeCooldown = 0.4f;

    [Header("Ranged (soplido de semillas)")]
    [SerializeField] private SeedPool _seedPool;
    [SerializeField] private Transform _shootPoint;
    [SerializeField] private int _maxSeeds = 10;
    [SerializeField] private float _rangedCooldown = 0.5f;
    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _seedShotSound;
    [Tooltip("Suena al balancear el tallo (melee), pegue o no.")]
    [SerializeField] private AudioClip _meleeSwingSound;
    [Tooltip("Suena ademas cuando el golpe melee le pega a un enemigo.")]
    [SerializeField] private AudioClip _meleeHitSound;
    [SerializeField, Range(0f, 1f)] private float _meleeVolume = 1f;

    private int _currentSeeds;
    private float _nextMeleeTime;
    private float _nextRangedTime;

    private Animator _animator;

    public bool IsAttacking => Time.time < _nextMeleeTime;

    // Se reutiliza para no generar basura en cada golpe
    private readonly HashSet<IDamageable> _hitTargets = new HashSet<IDamageable>();

    // La UI y el audio solo escuchan
    public event Action<int, int> OnSeedsChanged; // actuales, máximas
    public event Action OnMeleeAttack;
    public event Action OnSeedShot;

    public int CurrentSeeds => _currentSeeds;
    public int MaxSeeds => _maxSeeds;

    private void Awake()
    {
        _currentSeeds = _maxSeeds;
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        OnSeedsChanged?.Invoke(_currentSeeds, _maxSeeds);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= _nextMeleeTime)
            Melee();

        if (Input.GetMouseButtonDown(1) && Time.time >= _nextRangedTime)
            ShootSeed();
    }

    private void Melee()
    {
        _nextMeleeTime = Time.time + _meleeCooldown;

        if (_animator !=null) _animator.SetTrigger("Atacar");
        Sfx.Play2D(_meleeSwingSound, _meleeVolume);

        Vector3 center = transform.position + transform.forward * _meleeRange;
        Collider[] hits = Physics.OverlapSphere(center, _meleeRadius);

        _hitTargets.Clear();
        foreach (Collider hit in hits)
        {
            // No pegarse a sí misma
            if (hit.transform.IsChildOf(transform))
                continue;

            IDamageable target = hit.GetComponentInParent<IDamageable>();

            // Add devuelve false si ya lo golpeamos (un enemigo puede tener varios colliders)
            if (target != null && _hitTargets.Add(target))
                target.TakeDamage(_meleeDamage);
        }

        if (_hitTargets.Count > 0)
            Sfx.Play2D(_meleeHitSound, _meleeVolume);

        OnMeleeAttack?.Invoke();
    }

    private void ShootSeed()
{
    if (_currentSeeds <= 0)
        return;

    _nextRangedTime = Time.time + _rangedCooldown;
    _currentSeeds--;

    Seed seed = _seedPool.Get(_shootPoint.position, _shootPoint.rotation);
    seed.Launch(transform);

    if (_audioSource != null && _seedShotSound != null)
        _audioSource.PlayOneShot(_seedShotSound);

    OnSeedsChanged?.Invoke(_currentSeeds, _maxSeeds);
    OnSeedShot?.Invoke();
}

    // Lo llama el item diente de león al recogerlo
    public void AddSeeds(int amount)
    {
        _currentSeeds = Mathf.Min(_currentSeeds + amount, _maxSeeds);
        OnSeedsChanged?.Invoke(_currentSeeds, _maxSeeds);
    }

    // Muestra el área del golpe en la Scene para poder ajustarla
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + transform.forward * _meleeRange, _meleeRadius);
    }
}
