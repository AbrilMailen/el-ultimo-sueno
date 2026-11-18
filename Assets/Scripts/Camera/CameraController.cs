using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Tooltip("Jugador al que la camara sigue y apunta.")]
    [SerializeField] private Transform _target;

    [Header("Seguimiento")]
    [Tooltip("Activa un retraso suave. Desactivado mantiene el offset exacto al moverse.")]
    [SerializeField] private bool _smoothFollow;
    [SerializeField, Min(0f)] private float _followSpeed = 8f;

    [Header("Posicion relativa al jugador")]
    [Tooltip("Desplazamiento en ejes globales. No gira con el jugador.")]
    [SerializeField] private Vector3 _offset = new Vector3(0f, 11.47153f, -16.38304f);

    public void SetTarget(Transform target)
    {
        _target = target;
        SnapToTarget();
    }

    private void OnEnable()
    {
        SnapToTarget();
    }

    private void SnapToTarget()
    {
        if (_target == null)
            return;

        transform.position = _target.position + _offset;
        LookAtTarget();
    }

    private void LateUpdate()
    {
        if (_target == null)
            return;

        // Leer el Transform interpolado, no Rigidbody.position (pose de fisica).
        Vector3 desiredPosition = _target.position + _offset;

        transform.position = _smoothFollow
            ? Vector3.Lerp(
                transform.position,
                desiredPosition,
                1f - Mathf.Exp(-_followSpeed * Time.deltaTime))
            : desiredPosition;

        LookAtTarget();
    }

    [ContextMenu("Capturar offset desde la posicion actual")]
    private void CaptureOffsetFromCurrentPosition()
    {
        if (_target == null)
            return;

        _offset = transform.position - _target.position;
        LookAtTarget();
    }

    private void LookAtTarget()
    {
        Vector3 direction = _target.position - transform.position;
        if (direction.sqrMagnitude < 0.000001f)
            return;

        // Mantiene una orientacion estable incluso en una vista cenital.
        Vector3 up = Mathf.Abs(Vector3.Dot(direction.normalized, Vector3.up)) > 0.999f
            ? Vector3.forward
            : Vector3.up;

        transform.rotation = Quaternion.LookRotation(direction, up);
    }
}
