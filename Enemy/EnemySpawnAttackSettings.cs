using System;

// 적 발사 확률/탄속 보정 설정
public static class EnemySpawnAttackSettings
{
    public const float SpawnBulletMoveInterval = 0.05f; // 스폰 중 적 탄환 이동 간격(작을수록 빠름)
    public const double StageBaseAttackChanceBonusPerLevel = 0.002; // 스테이지마다 초기 공격 확률 +0.2%

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
    {
        if (shooter != null && shooter.IsSpawning)
        {
            return SpawnBulletMoveInterval;
        }

        return Bullet.DefaultMoveInterval;
    }
}
