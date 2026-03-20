using System.Collections.Generic;


// PlayScene의 Enemy 관련 공통 로직을 분리한 서비스 클래스
public static class PlaySceneEnemyService
{
    
    private const float k_EnemyAttackInterval = 0.5f;    // 적 공격 시도 간격 (초)
    private const double k_EnemyAttackChance = 0.2;      // 공격 시도 시 실제 발사 확률
    private const float k_EnemyStepInterval = 1f;        // 적이 한 칸 이동하는 간격 (초)

    public static bool UpdateEnemyAttack(
        EnemyAttackController enemyAttackController,
        float deltaTime,
        List<Enemy> enemies,
        Galaga player,
        int formationOffsetX,
        int stage)
    {
        return enemyAttackController.Update(deltaTime, enemies, player.X, player.Y, formationOffsetX, stage);
    }

    public static void ResetEnemiesForLifeLose(
        EnemyAttackController enemyAttackController,
        List<Enemy> enemies)
    {
        enemyAttackController.Clear();

        for (int i = 0; i < enemies.Count; i++)
        {
            Enemy enemy = enemies[i];
            if (!enemy.IsActive || enemy.IsShowingEffect)
            {
                continue;
            }

            enemy.ResetToSpawnPosition();   // 적을 초기 위치로 리셋
        }
    }
}
