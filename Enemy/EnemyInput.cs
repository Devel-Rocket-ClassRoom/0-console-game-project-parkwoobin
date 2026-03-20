using System;
using System.Collections.Generic;

// 처음 시작시 적이 위 아래 좌 우측에서 나오는 패턴을 관리하는 클래스, 적이 스폰될 때마다 해당 패턴에 따라 적의 초기 위치와 이동 방향을 설정하여 게임 내에서 사용

public class EnemyInput
{
    private readonly Random _random;
    const float k_SpawnSpeed = 0.6f;    // 스폰 진입 애니메이션 속도, 클수록 빠르게 포메이션으로 이동

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
                enemy.Y = Wall.Top - 3;
                break;
            case EnemySpawnPattern.LeftTop:    // 적이 왼쪽 위에서 진입
                enemy.X = Wall.Left - 12;
                enemy.Y = Wall.Top - 7;
                break;
            case EnemySpawnPattern.LeftBottom:  // 적이 왼쪽 아래에서 진입
                enemy.X = Wall.Left - 12;
                enemy.Y = 10;
                break;
            case EnemySpawnPattern.RightTop:   // 적이 오른쪽 위에서 진입
                enemy.X = Wall.Right + 12;
                enemy.Y = Wall.Top - 7;
                break;
            case EnemySpawnPattern.RightBottom:    // 적이 오른쪽 아래에서 진입
                enemy.X = Wall.Right + 12;
                enemy.Y = 10;
                break;
        }

        enemy.ChargeStartX = enemy.X;
        enemy.ChargeStartY = enemy.Y;
        enemy.ChargeTargetX = formationOffsetX + enemy.SpawnX;  // 적이 스폰된 후 이동할 목표 X 좌표를 설정 (적의 원래 스폰 위치 + 포메이션 오프셋)
        enemy.ChargeTargetY = enemy.SpawnY;
    }

    // 스테이지 번호에 따라 스폰 패턴을 결정하는 함수
    public EnemySpawnPattern GetSpawnPatternByStage(int stage)
    {
        int normalizedStage = Math.Max(1, stage);
        return (EnemySpawnPattern)((normalizedStage - 1) % 5);
    }

    // 스폰 진입 중인 적을 이동시키는 애니메이션 처리
    public void MoveEnemyIntoFormation(Enemy enemy, float deltaTime, int formationOffsetX)
    {
        if (enemy.SpawnDelaySeconds > 0f)   // 스폰 진입 대기 시간 처리, 스폰이 시작되기 전에 잠시 멈춰있도록 함
        {
            enemy.SpawnDelaySeconds -= deltaTime;
            return;
        }

        enemy.SpawnProgress += deltaTime * k_SpawnSpeed;    // 스폰 진입 애니메이션 진행, deltaTime과 k_SpawnSpeed를 곱하여 스폰 진행 속도를 조절
        // SpawnProgress가 1이 되면 스폰이 완료되어 포메이션 위치에 도달
        if (enemy.SpawnProgress > 1f)
        {
            enemy.SpawnProgress = 1f;
        }

        float t = enemy.SpawnProgress;
        int targetX = enemy.SpawnX + formationOffsetX;
        int targetY = enemy.SpawnY;

        float centerX = (Wall.Left + Wall.Right) * 0.8f;    // 화면 중앙 X 좌표 계산, 적이 스폰될 때 중앙을 기준으로 회전하는 애니메이션을 만들기 위해 사용
        float centerY = (Wall.Top + Wall.Bottom) * 0.5f - 1f;   // 화면 중앙 Y 좌표 계산, 플레이어 위치보다 약간 위로 설정하여 스폰 애니메이션이 플레이어와 겹치지 않도록 함

        float approachX = enemy.ChargeStartX + ((centerX - enemy.ChargeStartX) * t);
        float approachY = enemy.ChargeStartY + ((centerY - enemy.ChargeStartY) * t);

        float phaseBase;
        switch (enemy.SpawnPattern) // 스폰 패턴에 따라 회전 애니메이션의 초기 위상을 다르게 설정하여 적이 각기 다른 방향에서 회전하며 등장하도록 함
        {
            case EnemySpawnPattern.LeftTop:
                phaseBase = (float)(Math.PI * 0.95);    // 왼쪽에서 나오는 적은 시계 반대 방향으로 회전하며 등장
                break;
            case EnemySpawnPattern.LeftBottom:
                phaseBase = (float)(Math.PI * 1.15);
                break;
            case EnemySpawnPattern.Top:
                phaseBase = (float)(-Math.PI * 0.7);    // 위에서 나오는 적은 시계 방향으로 회전하며 등장
                break;
            case EnemySpawnPattern.RightTop:
                phaseBase = (float)(Math.PI * 0.2);    // 오른쪽에서 나오는 적은 시계 방향으로 회전하며 등장
                break;
            case EnemySpawnPattern.RightBottom:
                phaseBase = (float)(-Math.PI * 0.2);
                break;
            default:
                phaseBase = 0f;
                break;
        }

        float theta = phaseBase + (t * (float)(Math.PI * 2.2)) + enemy.SpawnCurvePhase;
        float radius = (1f - t) * 6.5f;
        float sinOffset = (float)Math.Sin(theta) * radius;
        float tanOffset = (float)Math.Tan((t - 0.5f) * 0.7f);
        tanOffset = Math.Clamp(tanOffset, -1.6f, 1.6f) * (1f - t) * 1.8f;

        float orbitX = approachX + sinOffset + tanOffset;
        float orbitY = approachY + ((float)Math.Cos(theta) * radius * 0.65f);

        float blend = t * t;
        float finalX = orbitX + ((targetX - orbitX) * blend);
        float finalY = orbitY + ((targetY - orbitY) * blend);

        enemy.X = (int)Math.Round(finalX);
        enemy.Y = (int)Math.Round(finalY);

        if (t >= 1f)
        {
            enemy.X = targetX;
            enemy.Y = targetY;
            enemy.IsSpawning = false;
        }
    }



}