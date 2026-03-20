using System;
using System.Collections.Generic;

// 스폰 연출 중인 적의 발사 확률/탄속 보정 설정
public static class EnemySpawnAttackSettings
{
    public const double SpawnShotChanceMultiplier = 1.1; // 스폰 중 적이 있으면 발사 확률 배수
    public const float SpawnBulletMoveInterval = 0.05f; // 스폰 중 적 탄환 이동 간격(작을수록 빠름)
    public const double StageBaseAttackChanceBonusPerLevel = 0.001; // 스테이지마다 초기 공격 확률 +0.1%

    // 공격 확률 = (스테이지 기본값) * 스폰 보정(선택)
    public static double GetShotChance(List<Enemy> enemies, double baseChance, int stage)
    // 스테이지와 스폰 상태에 따라 공격 확률을 계산하는 메서드
    {
        int normalizedStage = stage < 1 ? 1 : stage;
        bool hasSpawningEnemy = false;

        for (int i = 0; i < enemies.Count; i++)
        {
            Enemy enemy = enemies[i];
            if (!enemy.IsActive || enemy.IsShowingEffect)
            {
                continue;
            }

            if (enemy.IsSpawning)
            {
                hasSpawningEnemy = true;
            }
        }

        // 스테이지가 올라갈수록 기본 공격 확률이 증가한다.
        double stageBaseChance = baseChance + ((normalizedStage - 1) * StageBaseAttackChanceBonusPerLevel);

        double chance = stageBaseChance;
        if (hasSpawningEnemy)
        {
            chance *= SpawnShotChanceMultiplier;
        }

        return chance > 1.0 ? 1.0 : chance;
    }

    public static float GetBulletMoveInterval(Enemy shooter)
    {
        if (shooter != null && shooter.IsSpawning)
        {
            return SpawnBulletMoveInterval;
        }

        return Bullet.DefaultMoveInterval;
    }
}
