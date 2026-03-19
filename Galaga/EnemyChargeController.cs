using System;
using System.Collections.Generic;

// 게임의 적 돌격 설정을 관리하는 클래스, 돌격 시도 간격, 돌격 시작 확률, 동시에 돌격할 수 있는 적의 최대 수 등의 상수를 정의하여 게임 내에서 사용
public class EnemyChargeController
{
    private readonly HashSet<Enemy> _chargingEnemies = new HashSet<Enemy>();
    private readonly Random _random;
    private float _chargeTimer;

    public EnemyChargeController(Random random)
    {
        _random = random;
    }

    public bool IsCharging(Enemy enemy)
    {
        return _chargingEnemies.Contains(enemy);
    }

    public void Clear()
    {
        _chargingEnemies.Clear();
        _chargeTimer = 0f;
    }

    public void Remove(Enemy enemy)
    {
        _chargingEnemies.Remove(enemy);
    }

    public bool Update(float deltaTime, List<Enemy> enemies, int playerX, int playerY)
    {
        _chargeTimer += deltaTime;

        if (_chargeTimer >= EnemyChargeSettings.IntervalSeconds)
        {
            _chargeTimer = 0f;
            TryStartCharges(enemies, playerX, playerY);
        }

        if (_chargingEnemies.Count == 0)
        {
            return false;
        }

        List<Enemy> finished = new List<Enemy>();

        foreach (Enemy charger in _chargingEnemies)
        {
            if (charger == null || !charger.IsActive)
            {
                finished.Add(charger);
                continue;
            }

            if (charger.ChargePassedWall)
            {
                charger.Y += 1;

                if (charger.X > charger.ChargeStartX)
                {
                    charger.X -= 1;
                }
                else if (charger.X < charger.ChargeStartX)
                {
                    charger.X += 1;
                }

                if (EnemyAttack.CrushEnemy(charger, playerX, playerY))
                {
                    return true;
                }
            }
            else
            {
                if (EnemyAttack.ChargeTowardsPlayer(charger, playerX, playerY))
                {
                    return true;
                }

                if (charger.Y >= charger.ChargeTargetY)
                {
                    charger.ChargeReachedTarget = true;
                }

                if (charger.Y > Wall.Bottom)
                {
                    // 벽 아래를 지나가면 화면 위로 래핑
                    charger.Y = 0;
                    charger.ChargePassedWall = true;
                }
            }

            if (charger.ChargePassedWall && charger.Y >= charger.ChargeStartY && charger.X == charger.ChargeStartX)
            {
                charger.ResetChargeType();
                charger.Y = charger.ChargeStartY;
                charger.ChargeReachedTarget = false;
                charger.ChargePassedWall = false;
                finished.Add(charger);
            }
        }

        for (int i = 0; i < finished.Count; i++)
        {
            _chargingEnemies.Remove(finished[i]);
        }

        return false;
    }

    private void TryStartCharges(List<Enemy> enemies, int playerX, int playerY)
    {
        if (_chargingEnemies.Count >= EnemyChargeSettings.MaxSimultaneousCharges)
        {
            return;
        }

        if (_random.NextDouble() > EnemyChargeSettings.StartChance)
        {
            return;
        }

        List<Enemy> candidates = new List<Enemy>();
        for (int i = 0; i < enemies.Count; i++)
        {
            Enemy enemy = enemies[i];
            if (enemy.IsActive && !enemy.IsShowingEffect && enemy.Y < playerY && !_chargingEnemies.Contains(enemy))
            {
                candidates.Add(enemy);
            }
        }

        if (candidates.Count == 0)
        {
            return;
        }

        int remainingSlots = EnemyChargeSettings.MaxSimultaneousCharges - _chargingEnemies.Count;
        int maxStartCount = Math.Min(remainingSlots, candidates.Count);
        int startCount = _random.Next(1, maxStartCount + 1);

        for (int i = 0; i < startCount; i++)
        {
            int index = _random.Next(candidates.Count);
            Enemy charger = candidates[index];
            candidates.RemoveAt(index);

            charger.ChargeStartX = charger.X;
            charger.ChargeStartY = charger.Y;
            charger.ChargeTargetX = playerX;
            charger.ChargeTargetY = playerY;
            charger.ChargeReachedTarget = false;
            charger.ChargePassedWall = false;
            charger.SetChargeType();

            _chargingEnemies.Add(charger);
        }
    }
}
