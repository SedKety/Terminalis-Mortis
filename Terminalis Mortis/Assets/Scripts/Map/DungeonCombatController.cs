using UnityEngine;

public class DungeonCombatController
{
    public bool DealDamageToEnemy(Vector2Int position, int damage, DungeonEnemyController enemyController)
    {
        if (damage <= 0 || enemyController == null)
        {
            return false;
        }

        return enemyController.DealDamageToEnemy(position, damage);
    }

    public void DealDamageToPlayer(ref int playerHealth, int damage)
    {
        if (damage <= 0)
        {
            return;
        }

        playerHealth -= damage;
        if (playerHealth < 0)
        {
            playerHealth = 0;
        }
    }
}
