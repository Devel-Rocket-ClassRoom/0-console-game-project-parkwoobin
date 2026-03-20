using System;
using System.Collections.Generic;

// 처음 시작시 적이 위 아래 좌 우측에서 나오는 패턴을 관리하는 클래스, 적이 스폰될 때마다 해당 패턴에 따라 적의 초기 위치와 이동 방향을 설정하여 게임 내에서 사용

public class EnemyInput
{
    private readonly Random _random;

    public EnemyInput(Random random) // 적 입력 컨트롤러의 생성자, 랜덤 객체를 받아서 스폰 패턴 결정에 사용
    {
        _random = random;
    }

    public void ApplySpawnPattern(Enemy enemy, int formationOffsetX, int stage)
    // 적이 스폰될 때마다 호출되는 메서드, 스테이지 기반 패턴 함수로 초기 위치를 설정
    {
        EnemySpawnPattern pattern = GetSpawnPatternByStage(stage);

        ApplySpawnPatternByPattern(enemy, formationOffsetX, pattern);
    }

    // 스폰 패턴을 직접 지정해서 적용하는 함수
    public void ApplySpawnPatternByPattern(Enemy enemy, int formationOffsetX, EnemySpawnPattern pattern)
    {
        enemy.SpawnPattern = pattern;
        enemy.IsSpawning = true;
        enemy.SpawnProgress = 0f;

        switch (pattern)
        {
            case EnemySpawnPattern.Top: // 적이 위에서 내려오는 패턴
                enemy.X = (Wall.Left + Wall.Right) / 2;
                enemy.Y = Wall.Top - 8;
                break;
            case EnemySpawnPattern.Bottom:  // 적이 아래에서 올라오는 패턴
                enemy.X = Wall.Left - 12;
                enemy.Y = Wall.Bottom + 3;
                break;
            case EnemySpawnPattern.Left:    // 적이 왼쪽에서 오른쪽으로 이동하는 패턴
                enemy.X = Wall.Left - 12;
                enemy.Y = Wall.Bottom - 1;

                break;
            case EnemySpawnPattern.Right:   // 적이 오른쪽에서 왼쪽으로 이동하는 패턴
                enemy.X = Wall.Right + 12;
                enemy.Y = Wall.Bottom - 1;
                break;
        }

        enemy.ChargeStartX = enemy.X;
        enemy.ChargeStartY = enemy.Y;
        enemy.ChargeTargetX = formationOffsetX + enemy.SpawnX;
        enemy.ChargeTargetY = enemy.SpawnY;
    }

    // 스테이지 번호에 따라 스폰 패턴을 결정하는 함수
    public EnemySpawnPattern GetSpawnPatternByStage(int stage)
    {
        int normalizedStage = Math.Max(1, stage);
        return (EnemySpawnPattern)((normalizedStage - 1) % 4);
    }



}