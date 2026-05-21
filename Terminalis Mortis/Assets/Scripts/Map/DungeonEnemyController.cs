using System.Collections.Generic;
using System;
using UnityEngine;

public class DungeonEnemyController
{
    private readonly HashSet<Vector2Int> _aliveEnemies = new HashSet<Vector2Int>();

    public void InitializeFromMap(int width, int height, Func<int, int, bool> isEnemyAt)
    {
        _aliveEnemies.Clear();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (isEnemyAt != null && isEnemyAt(x, y))
                {
                    _aliveEnemies.Add(new Vector2Int(x, y));
                }
            }
        }
    }

    public IEnumerable<Vector2Int> GetAliveEnemyPositions()
    {
        return _aliveEnemies;
    }

    public bool IsEnemyAlive(Vector2Int position)
    {
        return _aliveEnemies.Contains(position);
    }

    public void MoveEnemy(Vector2Int from, Vector2Int to)
    {
        if (_aliveEnemies.Remove(from))
        {
            _aliveEnemies.Add(to);
        }
    }

    public void KillEnemy(Vector2Int at)
    {
        _aliveEnemies.Remove(at);
    }

}
