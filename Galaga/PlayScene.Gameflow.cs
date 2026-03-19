

// 게임의 플레이 씬을 나타내는 클래스, 적의 생성과 이동, 충돌 처리, 게임 종료 조건 등을 관리하는 게임플로우 관련 메서드들을 포함
public partial class PlayScene
{
    private void SpawnEnemies()
    {
        StageDefinition stageDefinition = StageData.Get(_stage);
        foreach (EnemySpawn spawn in stageDefinition.Enemies)
        {
            Enemy enemy = new Enemy(this, spawn.X, spawn.Y, spawn.Type);
            _enemies.Add(enemy);
            AddGameObject(enemy);
        }
    }

    private void HandleEnemyChargeCollision()
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
                _isGameOver = true;
                return;
            }
        }
    }

    private void MoveEnemies(float deltaTime)
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

        for (int i = 0; i < _enemies.Count; i++)
        {
            Enemy enemy = _enemies[i];
            if (enemy.IsActive && !_enemyChargeController.IsCharging(enemy))
            {
                enemy.MoveBy(direction);
            }
        }
    }

    private bool CanMoveEnemiesBy(int dx)
    {
        if (dx == 0)
        {
            return false;
        }

        for (int i = 0; i < _enemies.Count; i++)
        {
            Enemy enemy = _enemies[i];
            if (!enemy.IsActive)
            {
                continue;
            }

            if (_enemyChargeController.IsCharging(enemy))
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

    private void CheckEndConditions()
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
                _isGameOver = true;
                return;
            }
        }
    }
}
