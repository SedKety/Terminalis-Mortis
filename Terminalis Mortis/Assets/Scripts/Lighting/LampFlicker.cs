using UnityEngine;

public class LampFlicker : MonoBehaviour
{

    Light lampLight;

    [SerializeField] float baseIntensity;
    [SerializeField] float flickerStrength;
    [SerializeField] float flickerSpeed;

    void Start() => lampLight = GetComponent<Light>();

    void Update()
    {
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
        lampLight.intensity = baseIntensity + (noise - 0.5f) * flickerStrength;
    }
}
