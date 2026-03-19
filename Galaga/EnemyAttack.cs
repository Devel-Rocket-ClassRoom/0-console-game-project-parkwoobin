using System;
using System.Collections.Generic;

public static class EnemyAttack
{
    public const int MaxEnemyBullets = 2;   // 동시에 존재할 수 있는 적 총알의 최대 개수, 게임의 난이도 조절을 위해 설정된 상수

    public static Enemy PickShooter(List<Enemy> enemies, List<Bullet> bullets, Random random, double spawnChance)
    {
        int activeEnemyBullets = 0;
        for (int i = 0; i < bullets.Count; i++)
        {
            if (bullets[i].IsActive && bullets[i].IsEnemyBullet)
            {
                activeEnemyBullets++;
            }
        }

        if (activeEnemyBullets >= MaxEnemyBullets)
        {
            return null;
        }

        if (random.NextDouble() > spawnChance)
        {
            return null;
        }

        List<Enemy> candidates = new List<Enemy>();
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].IsActive)
            {
                candidates.Add(enemies[i]);
            }
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        return candidates[random.Next(candidates.Count)];
    }

    public static bool CrushEnemy(Enemy enemy, int playerX, int playerY)
    {
        if (enemy == null || !enemy.IsActive)
        {
            return false;
        }

        if (enemy.Y != playerY)
        {
            return false;
        }

        int enemyHalfWidth = enemy.Type == Enemy.EnemyType.Boss1 || enemy.Type == Enemy.EnemyType.Boss2 ? 2 : 1;
        const int playerHalfWidth = 1; // '/A\\' 3칸 폭

        int enemyLeft = enemy.X - enemyHalfWidth;
        int enemyRight = enemy.X + enemyHalfWidth;
        int playerLeft = playerX - playerHalfWidth;
        int playerRight = playerX + playerHalfWidth;

        return enemyLeft <= playerRight && enemyRight >= playerLeft;
    }
}
