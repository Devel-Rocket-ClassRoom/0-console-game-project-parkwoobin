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
            if (enemy.IsSpawning || enemy.IsShowingEffect)
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

        for (int i = 0; i < _enemies.Count; i++)    // 각 적을 이동시키는 메서드, 
        // 돌진 중인 적은 이동에서 제외하여 플레이어와의 충돌 처리를 따로 하도록 예외 처리
        {
            Enemy enemy = _enemies[i];
            if (enemy.IsActive && !enemy.IsShowingEffect && !_enemyAttackController.IsCharging(enemy))
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
        const int k_EnemyMinX = 4;  // 적이 이동할 수 있는 최소 X 좌표, 
        const int k_EnemyMaxX = 35; // 적이 이동할 수 있는 최대 X 좌표, 벽의 내부 영역을 기준으로 설정

        for (int i = 0; i < _enemies.Count; i++)
        {
            Enemy enemy = _enemies[i];
            if (enemy.IsActive && !enemy.IsShowingEffect && !_enemyAttackController.IsCharging(enemy))
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

    private void UpdateSpawningEnemies(float deltaTime) // 스폰 진입 중인 적들을 이동시키는 메서드, 각 적의 스폰 지연 시간이 끝나면 스폰 패턴에 따라 이동을 시작하도록 EnemyInput의 MoveEnemyIntoFormation 메서드를 호출
    {
        for (int i = 0; i < _enemies.Count; i++)
        {
            Enemy enemy = _enemies[i];
            if (!enemy.IsActive || _enemyAttackController.IsCharging(enemy) || !enemy.IsSpawning)
            {
                continue;
            }

            _enemyInput.MoveEnemyIntoFormation(enemy, deltaTime, _enemyFormationOffsetX);
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
            if (enemy.IsSpawning || enemy.IsShowingEffect)
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
