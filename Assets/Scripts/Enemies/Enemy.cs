using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Idle,
    Chase,
    Attack,
    Dead
}

// Base de todos los enemigos: detectar -> perseguir -> atacar -> recibir daño -> morir.
// Los hijos solo definen CÓMO atacan (PerformAttack).
[RequireComponent(typeof(NavMeshAgent))]
public abstract class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyData _data;
    [SerializeField] private Transform _target; // la jugadora

    [Header("Orientacion del modelo")]
    [Tooltip("Giro en Y que corrige hacia donde mira el modelo (180 si camina para atras).")]
    [SerializeField] private float _modelYawOffset = 180f;
    [SerializeField] private float _turnSpeed = 540f; // grados por segundo

    [Header("Feedback")]
    [SerializeField] private AudioClip _attackSound;
    [SerializeField] private AudioClip _hitSound;
    [SerializeField] private AudioClip _deathSound;
    [SerializeField] private Color _hitFlashColor = new Color(1f, 0.3f, 0.3f);
    [SerializeField] private float _hitFlashTime = 0.12f;

    [Header("Muerte")]
    [Tooltip("Segundos que el cuerpo queda en el piso despues de la animacion de muerte.")]
    [SerializeField] private float _deathLingerTime = 1.5f;
    [Tooltip("Si esta apagado, el cuerpo se queda en el piso con la pose final de la animacion de muerte.")]
    [SerializeField] private bool _removeBodyAfterDeath = true;
    [SerializeField] private float _sinkTime = 0.8f;
    [SerializeField] private float _sinkDepth = 1f;

    protected EnemyData Data => _data;
    protected IDamageable TargetDamageable { get; private set; }

    private static readonly int MovingHash = Animator.StringToHash("Moving");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int Attack2Hash = Animator.StringToHash("Attack2");
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int DeathHash = Animator.StringToHash("Death");
    // Controllers con triggers sueltos (ej. el pack del Toad): Idle/Walk, Hit1/Hit2, Death1/Death2
    private static readonly int IdleHash = Animator.StringToHash("Idle");
    private static readonly int WalkHash = Animator.StringToHash("Walk");
    private static readonly int Hit1Hash = Animator.StringToHash("Hit1");
    private static readonly int Hit2Hash = Animator.StringToHash("Hit2");
    private static readonly int Death1Hash = Animator.StringToHash("Death1");
    private static readonly int Death2Hash = Animator.StringToHash("Death2");
    private bool _hasAttackTrigger;
    private bool _hasAttack2Trigger;
    private bool _hasHitTrigger;
    private bool _hasDeathTrigger;
    private bool _hasMovingBool;
    private bool _hasIdleTrigger;
    private bool _hasWalkTrigger;
    private bool _hasHit1Trigger;
    private bool _hasHit2Trigger;
    private bool _hasDeath1Trigger;
    private bool _hasDeath2Trigger;
    private bool _lastMoving;
    private Renderer[] _renderers;
    private Coroutine _flashRoutine;

    private NavMeshAgent _agent;
    private Animator _animator;
    private EnemyState _state = EnemyState.Idle;
    private float _health;
    private float _nextAttackTime;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponentInChildren<Animator>();
        _hasAttackTrigger = HasParameter("Attack", AnimatorControllerParameterType.Trigger);
        _hasAttack2Trigger = HasParameter("Attack2", AnimatorControllerParameterType.Trigger);
        _hasHitTrigger = HasParameter("Hit", AnimatorControllerParameterType.Trigger);
        _hasDeathTrigger = HasParameter("Death", AnimatorControllerParameterType.Trigger);
        _hasMovingBool = HasParameter("Moving", AnimatorControllerParameterType.Bool);
        _hasIdleTrigger = HasParameter("Idle", AnimatorControllerParameterType.Trigger);
        _hasWalkTrigger = HasParameter("Walk", AnimatorControllerParameterType.Trigger);
        _hasHit1Trigger = HasParameter("Hit1", AnimatorControllerParameterType.Trigger);
        _hasHit2Trigger = HasParameter("Hit2", AnimatorControllerParameterType.Trigger);
        _hasDeath1Trigger = HasParameter("Death1", AnimatorControllerParameterType.Trigger);
        _hasDeath2Trigger = HasParameter("Death2", AnimatorControllerParameterType.Trigger);
        _renderers = GetComponentsInChildren<Renderer>();
        _agent.updateRotation = false; // la rotacion la maneja este script
        _agent.speed = _data.MoveSpeed;
        _agent.stoppingDistance = _data.AttackRange * 0.8f;
        _health = _data.MaxHealth;
    }

    private void Start()
    {
        if (_target == null)
        {
            Debug.LogWarning($"{name}: no tiene Target asignado.", this);
            return;
        }

        // La jugadora implementa IDamageable a través de su DreamTimer.
        // Se busca en el objeto, en sus padres y en sus hijos.
        TargetDamageable = FindDamageable(_target);

        if (TargetDamageable == null)
            Debug.LogWarning($"{name}: '{_target.name}' no tiene ningún componente que implemente IDamageable. No va a poder hacer daño.", this);
    }

    private static IDamageable FindDamageable(Transform target)
    {
        IDamageable damageable = target.GetComponentInParent<IDamageable>();

        if (damageable == null)
            damageable = target.GetComponentInChildren<IDamageable>();

        return damageable;
    }

    private void Update()
    {
        UpdateAnimation();
        FaceMovementDirection();

        if (_target == null || _state == EnemyState.Dead)
            return;

        float distance = GetFlatDistanceToTarget();

        switch (_state)
        {
            case EnemyState.Idle:
                if (distance <= _data.DetectionRange)
                    _state = EnemyState.Chase;
                break;

            case EnemyState.Chase:
                UpdateChase(distance);
                break;

            case EnemyState.Attack:
                UpdateAttack(distance);
                break;
        }
    }

    // Evita warnings si el controller todavia no tiene el parametro
    private bool HasParameter(string parameterName, AnimatorControllerParameterType type)
    {
        if (_animator == null || _animator.runtimeAnimatorController == null)
            return false;

        foreach (AnimatorControllerParameter parameter in _animator.parameters)
        {
            if (parameter.name == parameterName && parameter.type == type)
                return true;
        }

        return false;
    }

    // Gira hacia donde camina, corrigiendo la orientacion del modelo
    private void FaceMovementDirection()
    {
        if (_state != EnemyState.Chase)
            return;

        Vector3 velocity = _agent.velocity;
        velocity.y = 0f;
        if (velocity.sqrMagnitude < 0.05f)
            return;

        Quaternion desired = Quaternion.LookRotation(velocity) * Quaternion.Euler(0f, _modelYawOffset, 0f);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, desired, _turnSpeed * Time.deltaTime);
    }

    // El Animator camina solo mientras el agente se desplaza
    private void UpdateAnimation()
    {
        // Muerto: la animacion de muerte no se pisa con Idle/Walk
        if (_animator == null || _state == EnemyState.Dead)
            return;

        bool moving = _agent.isOnNavMesh
                      && !_agent.isStopped
                      && _agent.velocity.sqrMagnitude > 0.05f;
        SetMoving(moving);
    }

    // Soporta controllers con bool "Moving" o con triggers "Idle"/"Walk"
    private void SetMoving(bool moving)
    {
        if (_hasMovingBool)
        {
            _animator.SetBool(MovingHash, moving);
            return;
        }

        if (!_hasIdleTrigger || !_hasWalkTrigger || moving == _lastMoving)
            return;

        _lastMoving = moving;
        _animator.ResetTrigger(moving ? IdleHash : WalkHash);
        _animator.SetTrigger(moving ? WalkHash : IdleHash);
    }

    private void PlayHitAnimation()
    {
        if (_hasHitTrigger)
            _animator.SetTrigger(HitHash);
        else if (_hasHit1Trigger && _hasHit2Trigger)
            _animator.SetTrigger(Random.value < 0.5f ? Hit1Hash : Hit2Hash);
        else if (_hasHit1Trigger)
            _animator.SetTrigger(Hit1Hash);
    }

    private void PlayDeathAnimation()
    {
        // Evita que un Idle/Walk pendiente pise la animacion de muerte
        if (_hasIdleTrigger) _animator.ResetTrigger(IdleHash);
        if (_hasWalkTrigger) _animator.ResetTrigger(WalkHash);

        if (_hasDeathTrigger)
            _animator.SetTrigger(DeathHash);
        else if (_hasDeath1Trigger && _hasDeath2Trigger)
            _animator.SetTrigger(Random.value < 0.5f ? Death1Hash : Death2Hash);
        else if (_hasDeath1Trigger)
            _animator.SetTrigger(Death1Hash);
    }

    // Distancia en el plano horizontal: ignora la diferencia de altura
    // (el Base Offset del agente haría que la distancia real no coincida).
    private float GetFlatDistanceToTarget()
    {
        Vector3 offset = _target.position - transform.position;
        offset.y = 0f;
        return offset.magnitude;
    }

    private void UpdateChase(float distance)
    {
        if (distance > _data.DetectionRange)
        {
            _state = EnemyState.Idle;
            StopMoving();
            return;
        }

        if (distance <= _data.AttackRange)
        {
            _state = EnemyState.Attack;
            StopMoving();
            return;
        }

        if (_agent.isOnNavMesh)
        {
            _agent.isStopped = false;
            _agent.SetDestination(_target.position);
        }
    }

    private void UpdateAttack(float distance)
    {
        // Si la jugadora se alejó, vuelve a perseguirla
        if (distance > _data.AttackRange)
        {
            _state = EnemyState.Chase;
            return;
        }

        // Mirar a la jugadora (solo en el plano horizontal)
        Vector3 look = _target.position - transform.position;
        look.y = 0f;
        if (look.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(look) * Quaternion.Euler(0f, _modelYawOffset, 0f);

        if (Time.time >= _nextAttackTime)
        {
            _nextAttackTime = Time.time + _data.AttackCooldown;

            // Alterna al azar entre las dos animaciones de ataque (si existen)
            if (_hasAttackTrigger && _hasAttack2Trigger)
                _animator.SetTrigger(Random.value < 0.5f ? AttackHash : Attack2Hash);
            else if (_hasAttackTrigger)
                _animator.SetTrigger(AttackHash);

            PlaySound(_attackSound);
            PerformAttack();
        }
    }

    private void StopMoving()
    {
        if (_agent.isOnNavMesh)
            _agent.isStopped = true;
    }

    // Cada tipo de enemigo decide cómo ataca
    protected abstract void PerformAttack();

    public void TakeDamage(float damage)
    {
        if (_state == EnemyState.Dead)
            return;

        _health -= damage;

        if (_health <= 0f)
        {
            Die();
            return;
        }

        // Feedback de golpe: animacion, parpadeo rojo y sonido
        if (_animator != null)
            PlayHitAnimation();

        PlaySound(_hitSound);
        FlashRed();
    }

    private void Die()
    {
        _state = EnemyState.Dead;
        StopMoving();

        // El agente se apaga para que no siga moviendo ni rotando el cuerpo
        _agent.enabled = false;

        foreach (Collider col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        if (_animator != null)
        {
            if (_hasMovingBool)
                _animator.SetBool(MovingHash, false);

            PlayDeathAnimation();
        }

        PlaySound(_deathSound);
        StartCoroutine(DeathSequence());
    }

    // Se ve la animacion de muerte, se queda en el piso y despues se hunde y desaparece
    private IEnumerator DeathSequence()
    {
        // El cuerpo queda en el piso para siempre
        if (!_removeBodyAfterDeath)
            yield break;

        yield return new WaitForSeconds(_deathLingerTime);

        // Sin hundimiento: el cuerpo queda en la pose final de la animacion y desaparece
        if (_sinkDepth <= 0f || _sinkTime <= 0f)
        {
            Destroy(gameObject);
            yield break;
        }

        Vector3 start = transform.position;
        Vector3 end = start + Vector3.down * _sinkDepth;

        for (float t = 0f; t < _sinkTime; t += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(start, end, t / _sinkTime);
            yield return null;
        }

        Destroy(gameObject);
    }

    private void FlashRed()
    {
        if (_flashRoutine != null)
            StopCoroutine(_flashRoutine);

        _flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        var block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", _hitFlashColor);
        block.SetColor("_Color", _hitFlashColor);

        foreach (Renderer rend in _renderers)
            rend.SetPropertyBlock(block);

        yield return new WaitForSeconds(_hitFlashTime);

        foreach (Renderer rend in _renderers)
            rend.SetPropertyBlock(null);

        _flashRoutine = null;
    }

    private void PlaySound(AudioClip clip)
    {
        // 2D: con la camara cenital a ~10 unidades, un sonido 3D se oiria muy bajo
        Sfx.Play2D(clip);
    }
}