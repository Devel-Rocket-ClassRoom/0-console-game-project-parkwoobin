using System;
using System.Collections.Generic;

// 게임의 적 총알 발사와 플레이어와의 충돌 처리 등을 담당하는 클래스, 동시에 존재할 수 있는 적 총알의 최대 개수를 제한하고, 적이 플레이어를 향해 돌진하는 로직을 포함
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

        List<Enemy> spawnCandidates = new List<Enemy>();
        List<Enemy> normalCandidates = new List<Enemy>();
        for (int i = 0; i < enemies.Count; i++) // 활성화된 적 중에서 총알을 발사할 후보를 선정하는 루프, 동시에 존재할 수 있는 적 총알의 개수를 제한하기 위해 현재 활성화된 적 총알의 개수를 세고, 랜덤 확률에 따라 총알 발사 시도를 결정한 후, 후보 리스트에 추가
        {
            Enemy enemy = enemies[i];
            if (!enemy.IsActive)
            {
                continue;
            }

            // 화면 밖에서 쏜 탄환은 바로 사라져 체감이 낮으므로, 화면 안 후보만 사격 대상으로 선택
            bool isInScreen = enemy.X >= Wall.Left && enemy.X <= Wall.Right &&
                              enemy.Y >= Wall.Top && enemy.Y <= Wall.Bottom;
            if (!isInScreen)
            {
                continue;
            }

            if (enemy.IsSpawning)
            {
                spawnCandidates.Add(enemy);
            }
            else
            {
                normalCandidates.Add(enemy);
            }
        }

        List<Enemy> candidates = spawnCandidates.Count > 0 ? spawnCandidates : normalCandidates;
        if (candidates.Count == 0)  // 총알을 발사할 후보가 없는 경우 null을 반환하여 총알 발사를 하지 않도록 함
        {
            return null;
        }

        return candidates[random.Next(candidates.Count)];
    }

    public static bool CrushEnemy(Enemy enemy, int playerX, int playerY)    // 적과 플레이어가 겹치는지 확인하는 메서드, 적이 플레이어와 같은 Y 좌표에 있고, X 좌표가 겹치는 범위에 있는지 판단하여 겹침 여부를 반환
    {
        if (enemy == null || !enemy.IsActive)
        {
            return false;
        }

        if (enemy.Y != playerY)
        {
            return false;
        }

        int enemyHalfWidth = (enemy.Type == Enemy.EnemyType.Boss1 || enemy.Type == Enemy.EnemyType.Boss2 ||
                              enemy.Type == Enemy.EnemyType.Boss1_Rush || enemy.Type == Enemy.EnemyType.Boss2_Rush) ? 2 : 1;
        const int playerHalfWidth = 1; // '/A\\' 3칸 폭

        int enemyLeft = enemy.X - enemyHalfWidth;
        int enemyRight = enemy.X + enemyHalfWidth;
        int playerLeft = playerX - playerHalfWidth;
        int playerRight = playerX + playerHalfWidth;

        return enemyLeft <= playerRight && enemyRight >= playerLeft;
    }

    public static bool ChargeTowardsPlayer(Enemy enemy, int playerX, int playerY)
    {
        if (enemy == null || !enemy.IsActive)
        {
            return false;
        }

        // 돌진 시작 시점의 플레이어 위치를 향해 한 칸씩 이동(가로 1칸 + 세로 1칸)해서 돌진이 보이도록 처리
        int dx = 0;
        if (enemy.X < enemy.ChargeTargetX)
        {
            dx = 1;
        }
        else if (enemy.X > enemy.ChargeTargetX)
        {
            dx = -1;
        }

        // 적의 유형에 따라 폭이 다르므로 해당 유형에 맞게 범위를 계산하여 이동 후 충돌 감지
        int halfWidth = (enemy.Type == Enemy.EnemyType.Boss1 || enemy.Type == Enemy.EnemyType.Boss2 ||
                         enemy.Type == Enemy.EnemyType.Boss1_Rush || enemy.Type == Enemy.EnemyType.Boss2_Rush) ? 2 : 1;
        int nextX = enemy.X + dx;

        if (nextX - halfWidth < Wall.Left)
        {
            nextX = Wall.Left + halfWidth;
        }
        else if (nextX + halfWidth > Wall.Right)
        {
            nextX = Wall.Right - halfWidth;
        }

        // 실제 이동 처리 (가로 + 세로 1칸)
        enemy.MoveBy(nextX - enemy.X, 1);

        // 현재 플레이어 위치와의 충돌 감지
        return CrushEnemy(enemy, playerX, playerY);
    }
}
