using System;
using Framework.Engine;

public partial class PlayScene
{
    private void HandleShootInput(float deltaTime)
    {
        if (_secondShotTimer > 0f)
        {
            _secondShotTimer -= deltaTime;
        }

        if (!Input.IsKeyDown(ConsoleKey.Spacebar))
        {
            return;
        }

        int activePlayerBullets = CountActivePlayerBullets();
        if (activePlayerBullets >= 2)
        {
            return;
        }

        if (activePlayerBullets == 1 && _secondShotTimer > 0f)
        {
            return;
        }

        FirePlayerBullet();

        if (activePlayerBullets == 0)
        {
            _secondShotTimer = k_SecondShotDelay;
        }
    }

    private int CountActivePlayerBullets()
    {
        int count = 0;

        for (int i = 0; i < _bullets.Count; i++)
        {
            Bullet bullet = _bullets[i];
            if (bullet.IsActive && !bullet.IsEnemyBullet)
            {
                count++;
            }
        }

        return count;
    }

    private void FirePlayerBullet()
    {
        Bullet bullet = new Bullet(this, _player.X, _player.Y - 1, false);
        _bullets.Add(bullet);
        AddGameObject(bullet);
    }

    private void HandleEnemyAttack(float deltaTime)
    {
        _enemyAttackTimer += deltaTime;
        if (_enemyAttackTimer < k_EnemyAttackInterval)
        {
            return;
        }

        _enemyAttackTimer = 0f;
        Enemy shooter = EnemyAttack.PickShooter(_enemies, _bullets, _random, k_EnemyAttackChance);
        if (shooter == null)
        {
            return;
        }

        Bullet bullet = new Bullet(this, shooter.X, shooter.Y + 1, true);
        _bullets.Add(bullet);
        AddGameObject(bullet);
    }

    private void ResolveCollisions()
    {
        for (int i = 0; i < _bullets.Count; i++)
        {
            Bullet bullet = _bullets[i];
            if (!bullet.IsActive)
            {
                continue;
            }

            if (bullet.IsEnemyBullet)
            {
                if (bullet.Y == _player.Y && bullet.X >= _player.X - 1 && bullet.X <= _player.X + 1)
                {
                    bullet.IsActive = false;
                    _isGameOver = true;
                    return;
                }

                continue;
            }

            for (int j = 0; j < _enemies.Count; j++)
            {
                Enemy enemy = _enemies[j];
                if (!enemy.IsActive)
                {
                    continue;
                }

                if (enemy.IsHitAt(bullet.X, bullet.Y))
                {
                    bullet.IsActive = false;

                    bool isDestroyed = enemy.ApplyHit();
                    if (isDestroyed)
                    {
                        _score += 10;
                    }

                    break;
                }
            }
        }
    }

    private void CleanupInactiveObjects()
    {
        for (int i = _bullets.Count - 1; i >= 0; i--)
        {
            if (_bullets[i].IsActive)
            {
                continue;
            }

            RemoveGameObject(_bullets[i]);
            _bullets.RemoveAt(i);
        }

        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            if (_enemies[i].IsActive)
            {
                continue;
            }

            _enemyChargeController.Remove(_enemies[i]);
            RemoveGameObject(_enemies[i]);
            _enemies.RemoveAt(i);
        }
    }
}
