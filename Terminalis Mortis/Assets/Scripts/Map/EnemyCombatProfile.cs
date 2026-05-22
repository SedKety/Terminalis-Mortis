using UnityEngine;

[CreateAssetMenu(fileName = "EnemyCombatProfile", menuName = "Terminalis Mortis/Enemy Combat Profile")]
public class EnemyCombatProfile : ScriptableObject
{
    public AsciiCharacters enemyType = AsciiCharacters.Malware;
    public Vector2Int health = new Vector2Int(1, 1);
    public Vector2Int damage = new Vector2Int(1, 1);
    public int turnsBetweenMoves = 1;
    public bool attackOnTouch = true;
    public bool hasThorns;
    public int thornsDamage = 1;

    private static EnemyCombatProfile _defaultProfile;

    public static EnemyCombatProfile CreateDefault()
    {
        if (_defaultProfile == null)
        {
            _defaultProfile = CreateInstance<EnemyCombatProfile>();
            _defaultProfile.enemyType = AsciiCharacters.Malware;
            _defaultProfile.health = new Vector2Int(1, 1);
            _defaultProfile.damage = new Vector2Int(1, 1);
            _defaultProfile.turnsBetweenMoves = 1;
            _defaultProfile.attackOnTouch = true;
            _defaultProfile.hasThorns = false;
            _defaultProfile.thornsDamage = 0;
            _defaultProfile.hideFlags = HideFlags.HideAndDontSave;
        }

        return _defaultProfile;
    }

    public int RollHealth()
    {
        int min = Mathf.Max(1, Mathf.Min(health.x, health.y));
        int max = Mathf.Max(min, Mathf.Max(health.x, health.y));
        return Random.Range(min, max + 1);
    }

    public int RollDamage()
    {
        int min = Mathf.Max(0, Mathf.Min(damage.x, damage.y));
        int max = Mathf.Max(min, Mathf.Max(damage.x, damage.y));
        return Random.Range(min, max + 1);
    }
}
