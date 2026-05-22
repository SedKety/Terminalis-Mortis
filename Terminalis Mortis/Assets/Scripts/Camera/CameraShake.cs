using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    Vector3 originalPosition;

    [SerializeField] private Transform cameraShakePivot;

    [Header("Durations")]
    [SerializeField] private float lightDuration, mediumDuration, heavyDuration;

    [Header("Magnitudes")]
    [SerializeField] private float lightMagnitude, mediumMagnitude, heavyMagnitude;

    [Header("Animator Triggers (bools)")]
    public bool shakeLight;
    public bool shakeMedium;
    public bool shakeHeavy;

    private Coroutine currentShake;

    void Awake()
    {
        originalPosition = cameraShakePivot.localPosition;
    }

    void Update()
    {
        if (shakeLight)
        {
            shakeLight = false;
            StartShake(lightDuration, lightMagnitude);
        }

        if (shakeMedium)
        {
            shakeMedium = false;
            StartShake(mediumDuration, mediumMagnitude);
        }

        if (shakeHeavy)
        {
            shakeHeavy = false;
            StartShake(heavyDuration, heavyMagnitude);
        }
    }

    private void StartShake(float duration, float magnitude)
    {
        if (currentShake != null)
            StopCoroutine(currentShake);

        currentShake = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float dampened = magnitude * (1f - t);

            float x = Mathf.PerlinNoise(Time.time * 25f, 0f) * 2f - 1f;
            float y = Mathf.PerlinNoise(0f, Time.time * 25f) * 2f - 1f;

            cameraShakePivot.localPosition = originalPosition + new Vector3(x, y, 0f) * dampened;

            elapsed += Time.deltaTime;
            yield return null;
        }

        cameraShakePivot.localPosition = originalPosition;
        currentShake = null;
    }
}