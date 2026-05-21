using UnityEngine;

public class DungeonCombatController
{
    public void DealDamageToEnemy(Vector2Int position, int damage, DungeonEnemyController enemyController)
    {
        if (damage <= 0 || enemyController == null)
        {
            return;
        }

        enemyController.KillEnemy(position);
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
