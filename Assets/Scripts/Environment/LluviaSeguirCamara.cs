
using UnityEngine;

public class LluviaSeguirCamara : MonoBehaviour
{
    [SerializeField] private Transform _camara;
    [SerializeField] private float _altura = 15f;

    private void LateUpdate()
    {
        if (_camara == null) return;

        Vector3 posicion = _camara.position;
        posicion.y = _camara.position.y + _altura;

        transform.position = posicion;
    }
}
