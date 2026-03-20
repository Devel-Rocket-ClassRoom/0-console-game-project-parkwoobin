
// 게임의 적 돌격 설정을 관리하는 클래스, 돌격 시도 간격, 돌격 시작 확률, 동시에 돌격할 수 있는 적의 최대 수 등의 상수를 정의하여 게임 내에서 사용
public static class EnemyChargeSettings
{
    public const float IntervalSeconds = 3f;    // 돌격 시도 간격 (초)
    public const double StartChance = 0.25; // 돌격 시도 간격마다 돌격이 시작될 확률
    public const int MaxSimultaneousCharges = 2;    // 동시에 돌격할 수 있는 적의 최대 수
    public const float MoveIntervalSeconds = 0.15f; // 돌격 중 이동 주기(초). 값이 작을수록 빨라짐
    public const int MoveStep = 1; // 한 번 이동할 때 전진하는 칸 수
}
