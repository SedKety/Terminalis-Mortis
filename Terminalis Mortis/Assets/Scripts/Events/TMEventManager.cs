using UnityEngine;
using UnityEngine.Events;

namespace TerminalisMortis.Events
{
    [System.Serializable]
    public class Vector2IntEvent : UnityEvent<Vector2Int>
    {
    }

    [System.Serializable]
    public class StringEvent : UnityEvent<string>
    {
    }

    public class TMEventManager : MonoBehaviour
    {
        public static TMEventManager Instance { get; private set; }

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
}
