using System;
using System.Collections.Generic;
using Framework.Engine;
using System.IO;
using NAudio.Wave;

// 게임의 적 캐릭터를 나타내는 클래스, 적의 위치, 유형, 격추 이펙트, 돌진 상태 등을 관리하며, 적의 이동과 그리기 로직도 포함
public class Enemy : GameObject
{
    private const float k_EffectDuration = 0.25f;   // 격추 이펙트 표시 시간 (초)

    public enum EnemyType
    {
        Goei,
        Zako,
        Boss1,
        Boss2,
        Goei_Rush,
        Zako_Rush,
        Boss1_Rush,
        Boss2_Rush,
    }

    public int X { get; set; }
    public int Y { get; set; }
    public EnemyType Type { get; set; }
    private float _effectTimer;  // 격추 이펙트 타이머
    public bool IsShowingEffect { get; private set; }  // 격추 이펙트 표시 중 여부
    public int ChargeTargetX { get; set; }  // 돌진 목표 X 좌표
    public int ChargeTargetY { get; set; }  // 돌진 목표 Y 좌표
    public int ChargeStartX { get; set; }  // 돌진 시작 시 원래 X 좌표
    public int ChargeStartY { get; set; }  // 돌진 시작 시 원래 Y 좌표
    public bool ChargeReachedTarget { get; set; }  // 플레이어 Y 좌표에 도달했는가
    public bool ChargePassedWall { get; set; }  // 벽을 통과했는가
    private static readonly object s_AudioLock = new object();
    private static readonly string Zako_HitSoundPath = Path.Combine(AppContext.BaseDirectory, "BGM", "006 Hit on Zako.mp3");   // 적 격추 사운드 파일 경로
    private static readonly string Goei_HitSoundPath = Path.Combine(AppContext.BaseDirectory, "BGM", "007 Hit on Goei.mp3");   // 적 격추 사운드 파일 경로
    private static readonly string Boss1_HitSoundPath = Path.Combine(AppContext.BaseDirectory, "BGM", "008 Hit on Boss (1).mp3");   // 적 격추 사운드 파일 경로
    private static readonly string Boss2_HitSoundPath = Path.Combine(AppContext.BaseDirectory, "BGM", "009 Hit on Boss (2).mp3");   // 적 격추 사운드 파일 경로
    private static readonly List<(WaveOutEvent Output, AudioFileReader Audio)> s_ActiveSounds = new List<(WaveOutEvent Output, AudioFileReader Audio)>();

    public Enemy(Scene scene, int x, int y, EnemyType type) : base(scene)
    {
        Name = $"Enemy-{type}";
        X = x;
        Y = y;
        Type = type;
        _effectTimer = 0f;
        IsShowingEffect = false;
    }

    public void MoveBy(int dx)  // 적을 dx만큼 이동시키는 메서드, X 좌표를 dx만큼 증가시키도록 업데이트
    {
        X += dx;
    }

    public void MoveBy(int dx, int dy) // 적을 (dx, dy)만큼 이동시키는 메서드
    {
        X += dx;
        Y += dy;
    }

    public void SetChargeType() // 돌진 상태로 타입을 변경하는 메서드
    {
        switch (Type)
        {
            case EnemyType.Goei:
                Type = EnemyType.Goei_Rush;
                break;
            case EnemyType.Zako:
                Type = EnemyType.Zako_Rush;
                break;
            case EnemyType.Boss1:
                Type = EnemyType.Boss1_Rush;
                break;
            case EnemyType.Boss2:
                Type = EnemyType.Boss2_Rush;
                break;
        }
    }

    public void ResetChargeType() // 돌진 상태를 해제하고 원래 타입으로 돌리는 메서드
    {
        switch (Type)
        {
            case EnemyType.Goei_Rush:
                Type = EnemyType.Goei;
                break;
            case EnemyType.Zako_Rush:
                Type = EnemyType.Zako;
                break;
            case EnemyType.Boss1_Rush:
                Type = EnemyType.Boss1;
                break;
            case EnemyType.Boss2_Rush:
                Type = EnemyType.Boss2;
                break;
        }
    }

    public bool IsHitAt(int x, int y)   // 주어진 좌표가 적의 위치와 겹치는지 확인하는 메서드, 적의 유형에 따라 폭이 다르므로 해당 유형에 맞게 범위를 계산하여 판단
    {
        if (y != Y)
        {
            return false;
        }

        int halfWidth = (Type == EnemyType.Boss1 || Type == EnemyType.Boss2 ||
                         Type == EnemyType.Boss1_Rush || Type == EnemyType.Boss2_Rush) ? 2 : 1;
        return x >= X - halfWidth && x <= X + halfWidth;
    }

    // true를 반환하면 적이 완전히 격추되어 사라져야 함
    public bool ApplyHit()
    {
        if (Type == EnemyType.Boss1)
        {
            PlayHitSound(Type);
            Type = EnemyType.Boss2;
            return false;
        }

        if (Type == EnemyType.Boss1_Rush)
        {
            PlayHitSound(Type);
            Type = EnemyType.Boss2_Rush;
            return false;
        }

        // 격추 이펙트 시작
        IsShowingEffect = true;
        _effectTimer = k_EffectDuration;
        PlayHitSound(Type);
        return true;
    }

    private static string GetHitSoundPath(EnemyType type)   // 적 종류에 따른 격추 사운드 파일 경로를 반환하는 메서드, 각 적 유형에 맞는 사운드 파일 경로를 반환하도록 구현
    {
        switch (type)
        {
            case EnemyType.Zako:
            case EnemyType.Zako_Rush:
                return Zako_HitSoundPath;
            case EnemyType.Goei:
            case EnemyType.Goei_Rush:
                return Goei_HitSoundPath;
            case EnemyType.Boss1:
            case EnemyType.Boss1_Rush:
                return Boss1_HitSoundPath;
            case EnemyType.Boss2:
            case EnemyType.Boss2_Rush:
                return Boss2_HitSoundPath;
            default:
                return Zako_HitSoundPath;
        }
    }

    private static void PlayHitSound(EnemyType type)  // 적 종류에 맞는 격추 사운드를 재생
    {
        string path = GetHitSoundPath(type);
        if (!File.Exists(path))
        {
            return;
        }

        AudioFileReader audio = new AudioFileReader(path);
        WaveOutEvent output = new WaveOutEvent();

        output.PlaybackStopped += (sender, e) =>    // 사운드 재생이 끝나면 활성 사운드 목록에서 제거하고 리소스 해제
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

    public override void Update(float deltaTime)
    {
        if (IsShowingEffect)    // 격추 이펙트가 표시 중이면 타이머를 감소시키고, 시간이 다 되면 적을 비활성화
        {
            _effectTimer -= deltaTime;
            if (_effectTimer <= 0f)
            {
                IsActive = false;
            }
        }
    }

    public override void Draw(ScreenBuffer buffer)  // 적의 종류에 따라 다른 모양과 색상으로 그리는 메서드, 각 적 타입에 맞는 문자와 색상을 사용하여 버퍼에 그리기
    {
        if (IsShowingEffect)
        {
            buffer.SetCell(X, Y, '※', ConsoleColor.Yellow);
            return;
        }

        if (!IsActive)
        {
            return;
        }

        switch (Type)
        {
            case EnemyType.Goei:
                buffer.SetCell(X - 1, Y, '[', ConsoleColor.Red);
                buffer.SetCell(X, Y, 'W', ConsoleColor.Gray);
                buffer.SetCell(X + 1, Y, ']', ConsoleColor.Red);
                break;
            case EnemyType.Goei_Rush:
                buffer.SetCell(X - 1, Y, '[', ConsoleColor.Red);
                buffer.SetCell(X, Y, 'M', ConsoleColor.Gray);
                buffer.SetCell(X + 1, Y, ']', ConsoleColor.Red);
                break;
            case EnemyType.Zako:
                buffer.SetCell(X - 1, Y, '[', ConsoleColor.Blue);
                buffer.SetCell(X, Y, 'B', ConsoleColor.Yellow);
                buffer.SetCell(X + 1, Y, ']', ConsoleColor.Blue);
                break;
            case EnemyType.Zako_Rush:
                buffer.SetCell(X - 1, Y, '[', ConsoleColor.Blue);
                buffer.SetCell(X, Y, 'ꓭ', ConsoleColor.Yellow);
                buffer.SetCell(X + 1, Y, ']', ConsoleColor.Blue);
                break;
            case EnemyType.Boss1:
                buffer.SetCell(X - 2, Y, '┌', ConsoleColor.Green);
                buffer.SetCell(X - 1, Y, '[', ConsoleColor.Green);
                buffer.SetCell(X, Y, 'G', ConsoleColor.DarkYellow);
                buffer.SetCell(X + 1, Y, ']', ConsoleColor.Green);
                buffer.SetCell(X + 2, Y, '┐', ConsoleColor.Green);
                break;
            case EnemyType.Boss1_Rush:
                buffer.SetCell(X - 2, Y, '└', ConsoleColor.Green);
                buffer.SetCell(X - 1, Y, '[', ConsoleColor.Green);
                buffer.SetCell(X, Y, 'Ә', ConsoleColor.DarkYellow);
                buffer.SetCell(X + 1, Y, ']', ConsoleColor.Green);
                buffer.SetCell(X + 2, Y, '┘', ConsoleColor.Green);
                break;
            case EnemyType.Boss2:
                buffer.SetCell(X - 2, Y, '┌', ConsoleColor.Blue);
                buffer.SetCell(X - 1, Y, '[', ConsoleColor.Blue);
                buffer.SetCell(X, Y, 'G', ConsoleColor.DarkMagenta);
                buffer.SetCell(X + 1, Y, ']', ConsoleColor.Blue);
                buffer.SetCell(X + 2, Y, '┐', ConsoleColor.Blue);
                break;
            case EnemyType.Boss2_Rush:
                buffer.SetCell(X - 2, Y, '└', ConsoleColor.Blue);
                buffer.SetCell(X - 1, Y, '[', ConsoleColor.Blue);
                buffer.SetCell(X, Y, 'Ә', ConsoleColor.DarkMagenta);
                buffer.SetCell(X + 1, Y, ']', ConsoleColor.Blue);
                buffer.SetCell(X + 2, Y, '┘', ConsoleColor.Blue);
                break;
        }
    }
}
