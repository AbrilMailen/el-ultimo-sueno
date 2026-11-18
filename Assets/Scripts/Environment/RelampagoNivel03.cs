
using System.Collections;
using UnityEngine;

public class RelampagoNivel03 : MonoBehaviour
{
    [Header("Configuracion del relampago")]
    [SerializeField] private Light luzPrincipal;

    [SerializeField] private float multiplicadorIntensidad = 3.5f;
    [SerializeField] private float duracionDestello = 0.08f;

    [Header("Intervalo entre relampagos")]
    [SerializeField] private float intervaloMinimo = 5f;
    [SerializeField] private float intervaloMaximo = 12f;

    [Header("Sonido de trueno")]
    [SerializeField] private AudioClip sonidoTrueno;
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private float retrasoTrueno = 0.8f;

    private float intensidadOriginal;

    private void Start()
    {
        if (luzPrincipal == null)
            luzPrincipal = GetComponent<Light>();

        intensidadOriginal = luzPrincipal.intensity;

        StartCoroutine(CicloRelampagos());
    }

    private IEnumerator CicloRelampagos()
    {
        while (true)
        {
            float espera = Random.Range(
                intervaloMinimo,
                intervaloMaximo
            );

            yield return new WaitForSeconds(espera);

            yield return StartCoroutine(GenerarRelampago());
        }
    }

    private IEnumerator GenerarRelampago()
    {
        // Primer destello
        luzPrincipal.intensity =
            intensidadOriginal * multiplicadorIntensidad;

        yield return new WaitForSeconds(duracionDestello);

        luzPrincipal.intensity = intensidadOriginal;

        // Segundo destello mas breve
        yield return new WaitForSeconds(0.12f);

        luzPrincipal.intensity =
            intensidadOriginal * multiplicadorIntensidad * 0.7f;

        yield return new WaitForSeconds(duracionDestello);

        luzPrincipal.intensity = intensidadOriginal;

        // Reproducir trueno con retraso
        if (sonidoTrueno != null)
        {
            yield return new WaitForSeconds(retrasoTrueno);

            if (audioSource != null)
                audioSource.PlayOneShot(sonidoTrueno);
            else
                Sfx.Play2D(sonidoTrueno); // sin AudioSource asignado, suena igual
        }
    }
}
