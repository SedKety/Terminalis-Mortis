using UnityEngine;

public class Bobbing : MonoBehaviour
{
    [Header("Bob Settings")]
    [SerializeField] private float amplitude;
    [SerializeField] private float frequency;      

    [Header("Axis")]
    [SerializeField] private bool bobX = false;
    [SerializeField] private bool bobY = true;
    [SerializeField] private bool bobZ = false;

    private Vector3 startPosition;
    private float timeOffset;

    void Start()
    {
        startPosition = transform.position;

       // timeOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        float time = Time.time * frequency;
        float offset = Mathf.Sin(time) * amplitude;

        Vector3 newPos = startPosition;

        if (bobX) newPos.x += offset;
        if (bobY) newPos.y += offset;
        if (bobZ) newPos.z += offset;

        transform.position = newPos;
    }
}