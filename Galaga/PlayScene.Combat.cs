using System;
using Framework.Engine;


// 게임의 플레이 씬을 나타내는 클래스, 적의 생성과 이동, 충돌 처리, 게임 종료 조건 등을 관리하는 게임플로우 관련 메서드들을 포함
public partial class PlayScene
{
    // 플레이어 발사 입력과 2발 제한, 두 번째 발 지연을 처리
    private void HandleShootInput(float deltaTime)
    {
        _player.HandleShootInput(deltaTime, _bullets, this);
    }

    // 일정 주기마다 확률적으로 적 탄환 발사
    private void HandleEnemyAttack(float deltaTime)
    {
        _enemyAttackTimer += deltaTime;
        if (_enemyAttackTimer < k_EnemyAttackInterval)
        {
            return;
        }

        _enemyAttackTimer = 0f;
        double shotChance = EnemySpawnAttackSettings.GetShotChance(k_EnemyAttackChance, _stage);
        Enemy shooter = EnemyAttack.PickShooter(_enemies, _bullets, _random, shotChance);
        if (shooter == null)
        {
            return;
        }

        float bulletMoveInterval = EnemySpawnAttackSettings.GetBulletMoveInterval(shooter);
        Bullet bullet = new Bullet(this, shooter.X, shooter.Y + 1, true, bulletMoveInterval);
        _bullets.Add(bullet);
        AddGameObject(bullet);
    }

    // 탄환-플레이어/탄환-적 충돌을 처리하고 점수를 반영
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
                    LoseLife();
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

    // 비활성 오브젝트를 역순으로 정리하여 리스트 인덱스 꼬임 방지
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

            _enemyAttackController.Remove(_enemies[i]);
            RemoveGameObject(_enemies[i]);
            _enemies.RemoveAt(i);
        }
    }

    // 목숨 손실 연출(READY) 시작 시 화면의 모든 탄환을 즉시 제거
    private void ClearAllBulletsForLifeLose()
    {
        for (int i = _bullets.Count - 1; i >= 0; i--)
        {
            RemoveGameObject(_bullets[i]);
        }

        _bullets.Clear();
        _player.ResetShootState();
    }
}
