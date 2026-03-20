using System;
using System.Collections.Generic;

// 게임의 플레이 씬을 나타내는 클래스, 적의 생성과 이동, 충돌 처리, 게임 종료 조건 등을 관리하는 게임플로우 관련 메서드들을 포함
public partial class PlayScene
{
    private void SpawnEnemies() // 스테이지 데이터에 따라 적을 생성하는 메서드, 각 적의 위치와 유형을 스테이지 정의에서 가져와 적 객체를 생성하고 게임 오브젝트로 추가
    {
        StageDefinition stageDefinition = StageData.Get(_stage);

        var rowOrderByY = new Dictionary<int, int>();
        for (LinkedListNode<EnemySpawn> node = stageDefinition.Enemies.First; node != null; node = node.Next)
        {
            int y = node.Value.Y;
            if (!rowOrderByY.ContainsKey(y))
            {
                rowOrderByY[y] = 0;
            }
        }

        foreach (EnemySpawn spawn in stageDefinition.Enemies)
        {
            Enemy enemy = new Enemy(this, spawn.X, spawn.Y, spawn.Type);
            _enemyInput.ApplySpawnPatternByPattern(enemy, _enemyFormationOffsetX, spawn.Pattern);

            int orderInRow = rowOrderByY[spawn.Y];
            rowOrderByY[spawn.Y] = orderInRow + 1;

            // 좌측 하단에서 기차처럼 줄줄이 나오는 지연값(행 단위 + 열 단위)
            enemy.SpawnDelaySeconds = (orderInRow * 0.09f) + ((spawn.Y - 4) * 0.12f);
            enemy.SpawnCurvePhase = orderInRow * 0.45f;

            _enemies.Add(enemy);
            AddGameObject(enemy);
        }
    }

    private void HandleEnemyAttackCollision()   // 적이 돌진 중일 때 플레이어와 충돌하는지 확인하여 게임 오버 처리
    {
        for (int i = 0; i < _enemies.Count; i++)
        {
            Enemy enemy = _enemies[i];
            if (enemy.IsSpawning)
            {
                continue;
            }

            if (_enemyAttackController.IsCharging(enemy))
            {
                continue;
            }

            if (EnemyAttack.CrushEnemy(enemy, _player.X, _player.Y))
            {
                LoseLife();
                return;
            }
        }
    }

    private void MoveEnemies(float deltaTime)   // X축 5~35 범위 내에서 좌우 왕복 이동 처리
    {
        UpdateSpawningEnemies(deltaTime);

        _enemyStepTimer += deltaTime;
        if (_enemyStepTimer < k_EnemyStepInterval)
        {
            return;
        }

        _enemyStepTimer = 0f;
        int dx = _enemyDirection;

        if (!CanMoveEnemiesBy(dx))
        {
            _enemyDirection = -_enemyDirection;
            dx = _enemyDirection;

            if (!CanMoveEnemiesBy(dx))
            {
                return;
            }
        }

        for (int i = 0; i < _enemies.Count; i++)
        {
            Enemy enemy = _enemies[i];
            if (enemy.IsActive && !_enemyAttackController.IsCharging(enemy))
            {
                if (enemy.IsSpawning)
                {
                    continue;
                }

                enemy.MoveBy(dx, 0);
            }
        }

        _enemyFormationOffsetX += dx;

        CheckEndConditions();
    }

    private bool CanMoveEnemiesBy(int dx)   // 적들이 주어진 방향으로 이동할 수 있는지 확인하는 메서드, 각 적이 이동 후 벽에 부딪히는지 확인하여 이동 가능 여부를 반환
    {
        const int k_EnemyMinX = 4;
        const int k_EnemyMaxX = 35;

        for (int i = 0; i < _enemies.Count; i++)
        {
            Enemy enemy = _enemies[i];
            if (enemy.IsActive && !_enemyAttackController.IsCharging(enemy))
            {
                // 스폰 진입 중인 적은 경계 체크에서 제외
                if (enemy.IsSpawning)
                {
                    continue;
                }

                int newX = enemy.X + dx;
                if (newX < k_EnemyMinX || newX > k_EnemyMaxX)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void UpdateSpawningEnemies(float deltaTime)
    {
        for (int i = 0; i < _enemies.Count; i++)
        {
            Enemy enemy = _enemies[i];
            if (!enemy.IsActive || _enemyAttackController.IsCharging(enemy) || !enemy.IsSpawning)
            {
                continue;
            }

            MoveEnemyIntoFormation(enemy, deltaTime);
        }
    }

    private void MoveEnemyIntoFormation(Enemy enemy, float deltaTime)
    {
        if (enemy.SpawnDelaySeconds > 0f)
        {
            enemy.SpawnDelaySeconds -= deltaTime;
            return;
        }

        const float k_SpawnSpeed = 0.9f;
        enemy.SpawnProgress += deltaTime * k_SpawnSpeed;
        if (enemy.SpawnProgress > 1f)
        {
            enemy.SpawnProgress = 1f;
        }

        float t = enemy.SpawnProgress;
        int targetX = enemy.SpawnX + _enemyFormationOffsetX;
        int targetY = enemy.SpawnY;

        float centerX = (Wall.Left + Wall.Right) * 0.5f;
        float centerY = (Wall.Top + Wall.Bottom) * 0.5f - 1f;

        float approachX = enemy.ChargeStartX + ((centerX - enemy.ChargeStartX) * t);
        float approachY = enemy.ChargeStartY + ((centerY - enemy.ChargeStartY) * t);

        float phaseBase;
        switch (enemy.SpawnPattern)
        {
            case EnemySpawnPattern.Left:
                phaseBase = (float)(Math.PI * 0.95);
                break;
            case EnemySpawnPattern.Top:
                phaseBase = (float)(-Math.PI * 0.5);
                break;
            case EnemySpawnPattern.Right:
                phaseBase = (float)(Math.PI * 0.05);
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

    private void CheckEndConditions()    // 각 적이 플레이어와 겹치는지 확인하여 게임 오버 처리, 돌진 중인 적은 충돌 처리를 하지 않도록 예외 처리
    {
        if (_enemies.Count == 0)
        {
            if (_stage >= StageData.MaxStage)
            {
                _isAllClear = true;
            }
            else
            {
                _isClear = true;
            }

            return;
        }

        for (int i = 0; i < _enemies.Count; i++)
        {
            Enemy enemy = _enemies[i];
            if (enemy.IsSpawning)
            {
                continue;
            }

            if (_enemyAttackController.IsCharging(enemy))
            {
                continue;
            }

            if (enemy.Y >= _player.Y)
            {
                LoseLife();
                return;
            }
        }
    }
}
