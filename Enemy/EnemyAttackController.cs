using System;
using System.Collections.Generic;
using System.IO;
using NAudio.Wave;

// 게임의 적 돌격 설정을 관리하는 클래스, 돌격 시도 간격, 돌격 시작 확률, 동시에 돌격할 수 있는 적의 최대 수 등의 상수를 정의하여 게임 내에서 사용
public class EnemyAttackController
{
    private readonly HashSet<Enemy> _chargingEnemies = new HashSet<Enemy>();
    private readonly Random _random;
    private float _chargeTimer;
    private float _moveTimer;
    private static readonly object s_AudioLock = new object();
    private static readonly string FallingSoundPath = Path.Combine(AppContext.BaseDirectory, "BGM", "004 Alien Flying.mp3");
    private static readonly List<(WaveOutEvent Output, AudioFileReader Audio)> s_ActiveSounds = new List<(WaveOutEvent Output, AudioFileReader Audio)>();

    public EnemyAttackController(Random random) // 적 돌격 컨트롤러의 생성자, 랜덤 객체를 받아서 돌격 시작 확률 계산에 사용
    {
        _random = random;
    }

    public bool IsCharging(Enemy enemy)
    // 특정 적이 현재 돌격 중인지 확인하는 메서드
    // 돌격 중인 적들을 저장하는 HashSet에서 해당 적이 존재하는지 여부를 반환
    {
        return _chargingEnemies.Contains(enemy);
    }

    public void Clear()
    {
        _chargingEnemies.Clear();
        _chargeTimer = 0f;
        _moveTimer = 0f;
    }

    public void Remove(Enemy enemy)
    {
        _chargingEnemies.Remove(enemy);
    }

    public bool Update(float deltaTime, List<Enemy> enemies, int playerX, int playerY, int formationOffsetX, int stage)
    // 적 돌격을 업데이트하는 메서드, 돌격 시도 간격마다 새로운 돌격을 시작할 적들을 선정하여 돌격 상태로 설정하고, 
    // 돌격 중인 적들을 플레이어 방향으로 이동시키며 충돌 체크와 화면 밖으로 나가는 경우 복귀 처리 등을 수행
    {
        _chargeTimer += deltaTime;
        _moveTimer += deltaTime;

        if (_chargeTimer >= EnemyAttackSettings.IntervalSeconds)
        {
            _chargeTimer = 0f;
            TryStartCharges(enemies, playerX, playerY, stage);
        }

        if (_moveTimer < EnemyAttackSettings.MoveIntervalSeconds)
        {
            return false;
        }

        _moveTimer = 0f;

        List<Enemy> finished = new List<Enemy>();

        foreach (var e in _chargingEnemies)
        {
            if (e == null || !e.IsActive || e.IsShowingEffect)
            {
                finished.Add(e);
                continue;
            }

            MoveEnemy(e, playerX, playerY, formationOffsetX);

            // 충돌 체크
            if (e.ChargePattern != 3 && EnemyAttack.CrushEnemy(e, playerX, playerY))
            {
                return true;
            }


            // 화면 밖 → 복귀
            if (e.ChargePattern != 3 && e.Y > Wall.Bottom)
            {
                e.ChargeTargetX = e.SpawnX + formationOffsetX;
                e.ChargeTargetY = e.SpawnY;
                e.ChargeStartX = e.X;
                e.ChargeStartY = e.Y;
                e.X = e.ChargeTargetX;
                e.Y = Wall.Top;
                e.ChargePattern = 3; // 복귀 패턴
                e.ChargePassedWall = true;
                continue;
            }

            if (e.ChargePattern == 3 && e.Y >= e.ChargeTargetY && e.X == e.ChargeTargetX)   // 복귀 완료
            {
                EnemyReturnOverlapResolver.Apply(e, enemies);
                e.ResetChargeType();
                e.ChargePattern = 0;
                e.ChargePassedWall = false;
                finished.Add(e);
            }
        }
        foreach (var e in finished)
            _chargingEnemies.Remove(e);

        return false;
    }

    public static void MoveEnemy(Enemy e, int playerX, int playerY, int formationOffsetX) // 적을 플레이어 방향으로 이동시키는 메서드
    {
        switch (e.ChargePattern)
        {
            case 0: // 💥 직선 다이빙
                e.X += Math.Sign(playerX - e.X);
                e.Y += EnemyAttackSettings.MoveStep;
                break;

            case 1: // 🌀 지그재그
                e.Y += EnemyAttackSettings.MoveStep;
                e.X += (int)Math.Round(Math.Sin(e.Y * 0.6) * 1.5);
                break;

            case 2: // 🌀 원형
                e.Y += EnemyAttackSettings.MoveStep;
                e.X += (int)Math.Round(Math.Cos(e.Y * 0.6) * 1.5);
                break;

            case 3: // ↩ 화면 위에서 재진입 후 편대 위치로 복귀
                e.ChargeTargetX = e.SpawnX + formationOffsetX;
                e.ChargeTargetY = e.SpawnY;

                // 편대가 계속 이동하므로 복귀 중 X는 매 틱 목표 오프셋에 즉시 동기화한다.
                e.X = e.ChargeTargetX;

                if (e.Y < e.ChargeTargetY)
                {
                    e.Y += EnemyAttackSettings.MoveStep;
                    if (e.Y > e.ChargeTargetY)
                    {
                        e.Y = e.ChargeTargetY;
                    }
                }
                break;

            default:
                e.Y += EnemyAttackSettings.MoveStep;
                break;
        }
    }

    private void TryStartCharges(List<Enemy> enemies, int playerX, int playerY, int stage)
    // 새로운 돌격을 시작할 적들을 선정하여 돌격 상태로 설정하는 메서드
    {   // 동시에 돌격할 수 있는 적의 최대 수를 초과하지 않도록 하면서, 랜덤 확률에 따라 돌격을 시작할 적들을 후보로 선정하여 돌격 상태로 설정 
        if (_chargingEnemies.Count >= EnemyAttackSettings.MaxSimultaneousCharges)
            return;

        int aliveEnemyCount = 0;    // 현재 화면에 나타나 있는 적 중에서 살아있는 적의 수를 계산하여 돌격 시작 확률 결정에 사용
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].IsActive && !enemies[i].IsShowingEffect)
            {
                aliveEnemyCount++;
            }
        }

        double startChance = EnemyAttackSettings.GetStartChanceByEnemyCount(aliveEnemyCount, stage);
        if (_random.NextDouble() > startChance)
            return;

        List<Enemy> candidates = new List<Enemy>();
        for (int i = 0; i < enemies.Count; i++)
        {
            Enemy e = enemies[i];
            if (!e.IsActive || e.IsShowingEffect || _chargingEnemies.Contains(e))
                continue;

            if (e.Y >= playerY)
                continue;

            candidates.Add(e);
        }

        if (candidates.Count == 0)
            return;

        int pickIndex = _random.Next(candidates.Count);
        Enemy charger = candidates[pickIndex];

        // 초기 설정
        charger.ChargeStartX = charger.X;
        charger.ChargeStartY = charger.Y;
        charger.ChargeTargetX = playerX;
        charger.ChargeTargetY = playerY;
        charger.ChargePattern = _random.Next(3); // 0: 직진, 1: 지그재그, 2: 원형
        charger.SetChargeType();

        _chargingEnemies.Add(charger);
        PlayChargeSound();
    }

    private static void PlayChargeSound()
    {
        if (!File.Exists(FallingSoundPath))
        {
            return;
        }

        AudioFileReader audio = new AudioFileReader(FallingSoundPath);
        WaveOutEvent output = new WaveOutEvent();

        output.PlaybackStopped += (sender, e) =>
        {
            lock (s_AudioLock)
            {
                for (int i = s_ActiveSounds.Count - 1; i >= 0; i--)
                {
                    if (ReferenceEquals(s_ActiveSounds[i].Output, output))
                    {
                        s_ActiveSounds.RemoveAt(i);
                        break;
                    }
                }
            }

            output.Dispose();
            audio.Dispose();
        };

        output.Init(audio);

        lock (s_AudioLock)
        {
            s_ActiveSounds.Add((output, audio));
        }

        output.Play();
    }
}
