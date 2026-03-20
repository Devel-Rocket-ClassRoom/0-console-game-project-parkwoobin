
using System;

// 게임의 적 돌격 설정을 관리하는 클래스, 돌격 시도 간격, 돌격 시작 확률, 동시에 돌격할 수 있는 적의 최대 수 등의 상수를 정의하여 게임 내에서 사용
public static class EnemyAttackSettings
{
    public const float IntervalSeconds = 3f;    // 돌격 시도 간격 (초)
    public const double StartChance = 0.25; // 스테이지 1의 초기 돌진 확률(기본값)
    public const double StageBaseStartChanceBonusPerLevel = 0.05; // 스테이지마다 초기 돌진 확률 +5%
    public const double AliveEnemyCountStartChanceBonusMax = 0.20; // 적 수가 줄었을 때 추가 보너스 최대치
    public const int EnemyCountForBaseChance = 20; // 이 수 이상이면 적 수 보정 0
    public const int EnemyCountForMaxBonus = 2;    // 이 수 이하면 적 수 보정 최대
    public const int MaxSimultaneousCharges = 2;    // 동시에 돌격할 수 있는 적의 최대 수
    public const float MoveIntervalSeconds = 0.15f; // 돌격 중 이동 주기(초). 값이 작을수록 빨라짐
    public const int MoveStep = 1; // 한 번 이동할 때 전진하는 칸 수

    // 돌진 확률 = 스테이지 기본값 + (적 수 감소 보정)
    // - 스테이지가 오를수록 초기 돌진 확률이 증가
    // - 살아있는 적 수가 줄어들수록 추가 보정으로 더 증가
    public static double GetStartChanceByEnemyCount(int aliveEnemyCount, int stage)
    {
        int normalizedStage = stage < 1 ? 1 : stage;
        double stageBaseChance = StartChance + ((normalizedStage - 1) * StageBaseStartChanceBonusPerLevel);

        int clampedAlive = Math.Clamp(aliveEnemyCount, EnemyCountForMaxBonus, EnemyCountForBaseChance);
        double enemyRatio = (double)(EnemyCountForBaseChance - clampedAlive) /
                            (EnemyCountForBaseChance - EnemyCountForMaxBonus);
        double aliveEnemyBonus = enemyRatio * AliveEnemyCountStartChanceBonusMax;

        return ClampChance(stageBaseChance + aliveEnemyBonus);
    }
    // 돌진 확률 계산 공식:
    // 기본값에서 스테이지 1 오를때마다 StageBaseStartChanceBonusPerLevel 만큼 증가
    // 살아있는 적 수가 EnemyCountForBaseChance에서 EnemyCountForMaxBonus로 줄어들 때마다 
    // 최대 AliveEnemyCountStartChanceBonusMax 만큼 추가 보정이 선형으로 증가, 최종 확률은 0과 1 사이로 제한

    private static double ClampChance(double chance)
    {
        return Math.Clamp(chance, 0.0, 1.0);
    }
}
