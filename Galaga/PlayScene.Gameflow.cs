

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

    private void MoveEnemies(float deltaTime)   // 적의 이동을 처리하는 메서드, 일정 간격마다 적들이 좌우로 이동하며, 이동 방향을 바꿀 때 벽에 부딪히는지 확인하여 방향을 반대로 바꾸거나 이동하지 않도록 처리
    {
        _enemyStepTimer += deltaTime;
        if (_enemyStepTimer < k_EnemyStepInterval)
        {
            return;
        }

        _enemyStepTimer = 0f;
        int direction = _random.Next(0, 2) == 0 ? -1 : 1;

        if (!CanMoveEnemiesBy(direction))
        {
            if (CanMoveEnemiesBy(-direction))
            {
                direction *= -1;
            }
            else
            {
                return;
            }
        }

        _enemyDirection = direction;

        for (int i = 0; i < _enemies.Count; i++)    // 적들을 이동시키는 루프, 각 적이 활성화되어 있고 돌진 중이 아닌 경우에만 이동하도록 처리
        {
            Enemy enemy = _enemies[i];
            if (enemy.IsActive && !_enemyChargeController.IsCharging(enemy))
            {
                enemy.MoveBy(direction);
            }
        }
    }

    private bool CanMoveEnemiesBy(int dx)   // 적들이 주어진 방향으로 이동할 수 있는지 확인하는 메서드, 각 적이 이동 후 벽에 부딪히는지 확인하여 이동 가능 여부를 반환
    {
        if (dx == 0)
        {
            return false;
        }

        for (int i = 0; i < _enemies.Count; i++)    // 각 적이 이동 후 벽에 부딪히는지 확인하는 루프
        {
            Enemy enemy = _enemies[i];
            if (!enemy.IsActive)
            {
                continue;
            }

            if (_enemyChargeController.IsCharging(enemy))   // 돌진 중인 적은 이동 체크에서 제외하여 돌진이 보이도록 처리
            {
                continue;
            }

            int halfWidth = enemy.Type == Enemy.EnemyType.Boss1 || enemy.Type == Enemy.EnemyType.Boss2 ? 2 : 1;
            int nextLeft = (enemy.X + dx) - halfWidth;
            int nextRight = (enemy.X + dx) + halfWidth;

            if (nextLeft < Wall.Left || nextRight > Wall.Right)
            {
                return false;
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
