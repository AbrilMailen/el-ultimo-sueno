using UnityEngine;

// Mueve a la niña con WASD y la gira hacia el mouse (cámara cenital fija)
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private Camera _camera;

    [Header("Pasos")]
    [Tooltip("Uno o varios clips; se elige uno al azar en cada paso.")]
    [SerializeField] private AudioClip[] _footstepSounds;
    [SerializeField] private float _stepInterval = 0.4f;
    [SerializeField, Range(0f, 1f)] private float _footstepVolume = 0.6f;

    private float _nextStepTime;
    private Rigidbody _rigidbody;
    private Vector3 _moveDirection;
    private Vector3 _aimDirection;

    private Animator _animator;

    private PlayerCombat _playerCombat;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _playerCombat = GetComponent<PlayerCombat>();
        // El render y la camara leen una pose interpolada entre pasos de fisica.
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _aimDirection = transform.forward;

        if (_camera == null)
            _camera = Camera.main;
    }

    private void Update()
    {
        if (_playerCombat != null && _playerCombat.IsAttacking)
        {
            _moveDirection = Vector3.zero;
            if(_animator != null) _animator.speed = 1f;
            return;
        }
        ReadMovementInput();
        ReadAimInput();
        PlayFootsteps();

        if(_animator != null)
        {
            bool isMoving = _moveDirection.sqrMagnitude > 0.01f;
            _animator.speed = isMoving ? 1f : 0f;
        }
    }

    private void FixedUpdate()
    {
        _rigidbody.MovePosition(_rigidbody.position + _moveDirection * _moveSpeed * Time.fixedDeltaTime);
        _rigidbody.MoveRotation(Quaternion.LookRotation(_aimDirection));
    }

    private void PlayFootsteps()
    {
        if (_footstepSounds == null || _footstepSounds.Length == 0)
            return;

        if (_moveDirection.sqrMagnitude < 0.01f || Time.time < _nextStepTime)
            return;

        _nextStepTime = Time.time + _stepInterval;
        Sfx.Play2D(_footstepSounds[Random.Range(0, _footstepSounds.Length)], _footstepVolume);
    }

    private void ReadMovementInput()
    {
        float x = Input.GetAxisRaw("Horizontal"); // A / D
        float z = Input.GetAxisRaw("Vertical");   // W / S

        // normalized para que la diagonal no sea más rápida
        _moveDirection = new Vector3(x, 0f, z).normalized;
    }

    private void ReadAimInput()
    {
        // Rayo desde la cámara hasta un plano a la altura de la jugadora
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 direction = ray.GetPoint(distance) - transform.position;
            direction.y = 0f;

            // evita girar de golpe si el mouse está justo encima de ella
            if (direction.sqrMagnitude > 0.01f)
                _aimDirection = direction.normalized;
        }
    }
}
