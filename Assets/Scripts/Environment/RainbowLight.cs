
using UnityEngine;

public class RainbowLight : MonoBehaviour
{
    [SerializeField] private Light _light;
    [SerializeField] private float _speed = 0.25f;

    private void Update()
    {
        float hue = Mathf.Repeat(Time.time * _speed, 1f);
        _light.color = Color.HSVToRGB(hue, 0.65f, 1f);
    }
}
