using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Unity event that takes a Vector2Int parameter, used for handling combat events such as player and enemy hits, 
/// where the parameter represents the location of the hit.
/// </summary>
[System.Serializable]
public class Vector2IntEvent : UnityEvent<Vector2Int>{}

/// <summary>
/// Unity event that takes a string parameter, used for handling player input events such as commands entered in the monitor.
/// </summary>
[System.Serializable]
public class StringEvent : UnityEvent<string>{}

/// <summary>
/// Handles global events for the game, such as combat interactions and player input.
/// This allows for a decoupled architecture where different systems can subscribe to events without needing direct references to each other.
/// </summary>
public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    [Header("Combat Events")]
    [SerializeField] private Vector2IntEvent onPlayerHit = new Vector2IntEvent();
    [SerializeField] private Vector2IntEvent onEnemyHit = new Vector2IntEvent();

    [Header("Input Events")]
    [SerializeField] private StringEvent onEnter = new StringEvent();

    public Vector2IntEvent OnPlayerHit => onPlayerHit;
    public Vector2IntEvent OnEnemyHit => onEnemyHit;
    public StringEvent OnEnter => onEnter;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RaisePlayerHit(Vector2Int location)
    {
        onPlayerHit?.Invoke(location);
    }

    public void RaiseEnemyHit(Vector2Int location)
    {
        onEnemyHit?.Invoke(location);
    }

    public void RaiseEnter(string input)
    {
        onEnter?.Invoke(input);
    }
}
