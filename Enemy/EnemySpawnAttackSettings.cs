using System;

// 스폰중에 적이 공격할 확률
public static class EnemySpawnAttackSettings
{
    public const double StageBaseAttackChanceBonusPerLevel = 0.002; // 스테이지마다 초기 공격 확률 +0.2%

    public const float SpawnBulletMoveInterval = 0.05f; // 스폰 중 적 탄환 이동 간격(클수록 느림)

    // 공격 확률 = 스테이지 기본값
    public static double GetShotChance(double baseChance, int stage)
    // 스테이지에 따라 공격 확률을 계산하는 메서드

    {
        int normalizedStage = stage < 1 ? 1 : stage;

        // 스테이지가 올라갈수록 기본 공격 확률이 증가한다.
        double stageBaseChance = baseChance + ((normalizedStage - 1) * StageBaseAttackChanceBonusPerLevel);

        return Math.Clamp(stageBaseChance, 0.0, 1.0);
    }

    public static float GetBulletMoveInterval(Enemy shooter)
    // 적의 상태에 따라 탄환 이동 간격을 계산하는 메서드, 스폰 중인 적이 발사하는 탄환은 더 느리게 이동한다.
    {
        if (shooter != null && shooter.IsSpawning)
        {
            return SpawnBulletMoveInterval;
        }

        return SpawnBulletMoveInterval;
    }

}
