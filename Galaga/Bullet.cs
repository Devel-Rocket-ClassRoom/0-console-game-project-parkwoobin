using System;
using System.Collections.Generic;
using Framework.Engine;
using System.IO;
using NAudio.Wave;

// 게임의 총알을 나타내는 클래스, 총알의 위치와 이동, 그리기 로직을 담당하며, 적과 플레이어의 총알을 구분하여 처리
public class Bullet : GameObject
{
    private const float k_MoveInterval = 0.025f; // 총알이 한 칸 이동하는 간격 (초)

    private readonly int _directionY;
    private readonly LinkedList<(int X, int Y)> _path = new LinkedList<(int X, int Y)>();
    private LinkedListNode<(int X, int Y)> _headNode;
    private float _moveTimer;

    public int X => _headNode.Value.X;  // 총알의 현재 X 좌표
    public int Y => _headNode.Value.Y;  // 총알의 현재 Y 좌표
    public bool IsEnemyBullet { get; }
    private static readonly object s_AudioLock = new object();
    private static readonly string AttackSoundPath = Path.Combine(AppContext.BaseDirectory, "BGM", "005 Shot.mp3");   // 적 격추 사운드 파일 경로
    private static readonly List<(WaveOutEvent Output, AudioFileReader Audio)> s_ActiveSounds = new List<(WaveOutEvent Output, AudioFileReader Audio)>();

    public Bullet(Scene scene, int x, int y, bool isEnemyBullet) : base(scene)
    {
        Name = "Bullet";
        IsEnemyBullet = isEnemyBullet;
        _directionY = IsEnemyBullet ? 1 : -1;

        _headNode = _path.AddFirst((x, y));
        if (!IsEnemyBullet)
        {
            PlayHitSound();
        }
    }

    public override void Update(float deltaTime)    // 총알의 이동을 처리하는 업데이트 메서드, 이동 간격이 지나면 다음 위치로 이동하고, 벽의 범위를 벗어나면 비활성화
    {
        _moveTimer += deltaTime;
        if (_moveTimer < k_MoveInterval)
        {
            return;
        }

        _moveTimer = 0f;    // 이동 간격이 지났으므로 다음 위치로 이동
        MoveToNextNode();

        if (Y < Wall.Top || Y > Wall.Bottom)
        {
            IsActive = false;
        }
    }

    private static void PlayHitSound()  // 총알 발사 사운드를 1회 재생
    {
        AudioFileReader audio = new AudioFileReader(AttackSoundPath);
        WaveOutEvent output = new WaveOutEvent();

        output.PlaybackStopped += (sender, e) =>  // 사운드 재생이 끝나면 활성 사운드 목록에서 제거하고 리소스 해제  
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

        lock (s_AudioLock)  // 사운드 재생이 시작되면 활성 사운드 목록에 추가하여 리소스 관리를 용이하게 함
        {
            s_ActiveSounds.Add((output, audio));
        }

        output.Play();
    }

    private void MoveToNextNode()   // 총알이 이동할 다음 위치를 계산하여 헤드 노드로 추가하고, 기존 노드는 제거하여 총알의 위치를 업데이트
    {
        (int X, int Y) current = _headNode.Value;
        _headNode = _path.AddFirst((current.X, current.Y + _directionY));

        // 헤드 노드만 남겨서 node 기반 위치 관리만 유지
        while (_path.Count > 1)
        {
            _path.RemoveLast();
        }
    }

    public override void Draw(ScreenBuffer buffer)  // 총알의 종류에 따라 다른 문자와 색상으로 표시
    {
        if (!IsActive)
        {
            return;
        }

        char bulletChar = IsEnemyBullet ? '│' : '^';
        ConsoleColor color = IsEnemyBullet ? ConsoleColor.White : ConsoleColor.Green;
        buffer.SetCell(X, Y, bulletChar, color);
    }
}