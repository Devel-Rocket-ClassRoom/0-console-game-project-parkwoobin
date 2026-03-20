

// 게임의 플레이 씬을 나타내는 클래스, 적의 생성과 이동, 충돌 처리, 게임 종료 조건 등을 관리하는 게임플로우 관련 메서드들을 포함
public partial class PlayScene
{
    private void SpawnEnemies() // 스테이지 데이터에 따라 적을 생성하는 메서드, 각 적의 위치와 유형을 스테이지 정의에서 가져와 적 객체를 생성하고 게임 오브젝트로 추가
    {
        StageDefinition stageDefinition = StageData.Get(_stage);
        foreach (EnemySpawn spawn in stageDefinition.Enemies)
        {
            Enemy enemy = new Enemy(this, spawn.X, spawn.Y, spawn.Type);
            _enemies.Add(enemy);
            AddGameObject(enemy);
        }
    }

    private void HandleEnemyChargeCollision()   // 적이 돌진 중일 때 플레이어와 충돌하는지 확인하여 게임 오버 처리
    {
        for (int i = 0; i < _enemies.Count; i++)
        {
            Enemy enemy = _enemies[i];
            if (_enemyChargeController.IsCharging(enemy))
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
            if (enemy.IsActive && !_enemyChargeController.IsCharging(enemy))
            {
                enemy.MoveBy(dx, 0);
            }
        }

        CheckEndConditions();
    }

    private bool CanMoveEnemiesBy(int dx)   // 적들이 주어진 방향으로 이동할 수 있는지 확인하는 메서드, 각 적이 이동 후 벽에 부딪히는지 확인하여 이동 가능 여부를 반환
    {
        const int k_EnemyMinX = 4;
        const int k_EnemyMaxX = 35;

        for (int i = 0; i < _enemies.Count; i++)
        {
            Enemy enemy = _enemies[i];
            if (enemy.IsActive && !_enemyChargeController.IsCharging(enemy))
            {
                int newX = enemy.X + dx;
                if (newX < k_EnemyMinX || newX > k_EnemyMaxX)
                {
                    return false;
                }
            }
        }
        return true;
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
            if (_enemyChargeController.IsCharging(enemy))
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
