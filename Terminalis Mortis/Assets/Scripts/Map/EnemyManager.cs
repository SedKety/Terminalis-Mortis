using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [SerializeField] private List<EnemyCombatProfile> enemyProfiles = new List<EnemyCombatProfile>();

    private readonly Dictionary<int, EnemyCombatProfile> _profileLookup = new Dictionary<int, EnemyCombatProfile>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate EnemyManager detected. Destroying duplicate instance on " + gameObject.name + ".");
            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
            else
            {
                DestroyImmediate(gameObject);
            }

            return;
        }

        Instance = this;
        RebuildLookup();
    }

    private void OnValidate()
    {
        RebuildLookup();
    }

    private void RebuildLookup()
    {
        _profileLookup.Clear();

        for (int i = 0; i < enemyProfiles.Count; i++)
        {
            EnemyCombatProfile profile = enemyProfiles[i];
            if (profile != null)
            {
                _profileLookup[(int)profile.enemyType] = profile;
            }
        }
    }

    public EnemyCombatProfile GetCombatProfile(int enemyType)
    {
        if (_profileLookup.TryGetValue(enemyType, out EnemyCombatProfile profile) && profile != null)
        {
            return profile;
        }

        return EnemyCombatProfile.CreateDefault();
    }

    public IEnumerable<AsciiCharacters> GetConfiguredEnemyTypes()
    {
        foreach (KeyValuePair<int, EnemyCombatProfile> entry in _profileLookup)
        {
            yield return (AsciiCharacters)entry.Key;
        }
    }
}
