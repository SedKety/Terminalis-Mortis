using System.Collections.Generic;
using System;
using UnityEngine;

public class DungeonEnemyController
{
    private sealed class EnemyState
    {
        public AsciiCharacters Type;
        public int CurrentHealth;
        public int Damage;
        public int TurnsBetweenMoves;
        public bool AttackOnTouch;
        public bool HasThorns;
        public int ThornsDamage;
        public int MoveCooldown;
    }

    private readonly Dictionary<Vector2Int, EnemyState> _enemies = new Dictionary<Vector2Int, EnemyState>();

    public void InitializeFromMap(AsciiCharacters[,] map, EnemyManager enemyManager, Func<AsciiCharacters, bool> isEnemyTile)
    {
        _enemies.Clear();

        if (map == null)
        {
            return;
        }

        int width = map.GetLength(0);
        int height = map.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                AsciiCharacters tile = map[x, y];
                if (isEnemyTile != null && isEnemyTile(tile))
                {
                    EnemyCombatProfile profile = enemyManager != null
                        ? enemyManager.GetCombatProfile((int)tile)
                        : EnemyCombatProfile.CreateDefault();

                    _enemies[new Vector2Int(x, y)] = new EnemyState
                    {
                        Type = tile,
                        CurrentHealth = profile.RollHealth(),
                        Damage = profile.RollDamage(),
                        TurnsBetweenMoves = Mathf.Max(1, profile.turnsBetweenMoves),
                        AttackOnTouch = profile.attackOnTouch,
                        HasThorns = profile.hasThorns,
                        ThornsDamage = Mathf.Max(0, profile.thornsDamage),
                        MoveCooldown = 0
                    };
                }
            }
        }
    }

    public IEnumerable<Vector2Int> GetAliveEnemyPositions()
    {
        return _enemies.Keys;
    }

    public bool IsEnemyAlive(Vector2Int position)
    {
        return _enemies.ContainsKey(position);
    }

    public bool MoveEnemy(Vector2Int from, Vector2Int to)
    {
        if (!_enemies.TryGetValue(from, out EnemyState state))
        {
            return false;
        }

        _enemies.Remove(from);
        _enemies[to] = state;
        return true;
    }

    public void KillEnemy(Vector2Int at)
    {
        _enemies.Remove(at);
    }

    public bool DealDamageToEnemy(Vector2Int at, int damage)
    {
        if (damage <= 0)
        {
            return false;
        }

        if (!_enemies.TryGetValue(at, out EnemyState state))
        {
            return false;
        }

        state.CurrentHealth -= damage;
        if (state.CurrentHealth <= 0)
        {
            _enemies.Remove(at);
            return true;
        }

        return false;
    }

    public int GetEnemyDamage(Vector2Int at)
    {
        if (_enemies.TryGetValue(at, out EnemyState state))
        {
            return Mathf.Max(0, state.Damage);
        }

        return 0;
    }

    public bool CanAttackOnTouch(Vector2Int at)
    {
        return _enemies.TryGetValue(at, out EnemyState state) && state.AttackOnTouch;
    }

    public bool HasThorns(Vector2Int at)
    {
        return _enemies.TryGetValue(at, out EnemyState state) && state.HasThorns;
    }

    public int GetThornsDamage(Vector2Int at)
    {
        if (_enemies.TryGetValue(at, out EnemyState state) && state.HasThorns)
        {
            return Mathf.Max(0, state.ThornsDamage);
        }

        return 0;
    }

    public bool CanMoveThisTurn(Vector2Int at)
    {
        if (!_enemies.TryGetValue(at, out EnemyState state))
        {
            return false;
        }

        if (state.TurnsBetweenMoves <= 1)
        {
            return true;
        }

        if (state.MoveCooldown > 0)
        {
            state.MoveCooldown--;
            return false;
        }

        state.MoveCooldown = state.TurnsBetweenMoves - 1;
        return true;
    }
}
