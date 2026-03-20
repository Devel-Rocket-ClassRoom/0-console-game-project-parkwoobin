using System.Collections.Generic;

// PlayScene의 Enemy 관련 공통 로직을 분리한 서비스 클래스
public static class PlaySceneEnemyService
{
    private const float k_EnemyAttackInterval = 0.5f;    // 적 공격 시도 간격 (초)
    private const double k_EnemyAttackChance = 0.2;      // 공격 시도 시 실제 발사 확률
    private const float k_EnemyStepInterval = 1f;        // 적이 한 칸 이동하는 간격 (초)

    public static bool UpdateEnemyCharge(
        EnemyChargeController enemyChargeController,
        float deltaTime,
        List<Enemy> enemies,
        Galaga player)
    {
        return enemyChargeController.Update(deltaTime, enemies, player.X, player.Y);
    }

    public static void ResetEnemiesForLifeLose(
        EnemyChargeController enemyChargeController,
        List<Enemy> enemies)
    {
        enemyChargeController.Clear();

        for (int i = 0; i < enemies.Count; i++)
        {
            Enemy enemy = enemies[i];
            if (!enemy.IsActive || enemy.IsShowingEffect)
            {
                continue;
            }

            enemy.ResetToSpawnPosition();
        }
    }
}
