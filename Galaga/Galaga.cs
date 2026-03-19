using System;
using Framework.Engine;

// 게임의 플레이어 캐릭터를 나타내는 클래스, 플레이어의 위치와 이동, 그리기 로직을 담당
public class Galaga : GameObject
{
    private const float k_MoveInterval = 0.05f; // 플레이어가 한 칸 이동하는 간격 (초)
    private const float k_EffectDuration = 2f; // 플레이어 격추 이펙트 시간 (초)

    private readonly int _startX;
    private readonly int _minX;
    private readonly int _maxX;
    private float _moveTimer;
    private float _effectTimer;

    public int X { get; private set; }
    public int Y { get; }
    public bool IsShowingEffect { get; private set; }

    public Galaga(Scene scene, int startX, int startY, int minX, int maxX) : base(scene)
    {
        Name = "Player";
        _startX = startX;
        X = startX;
        Y = startY;
        _minX = minX;
        _maxX = maxX;
        _effectTimer = 0f;
        IsShowingEffect = false;
    }

    public void ResetPosition()
    {
        X = _startX;
        _moveTimer = 0f;
        _effectTimer = 0f;
        IsShowingEffect = false;
    }

    public void StartExplosion()
    {
        _effectTimer = k_EffectDuration;
        IsShowingEffect = true;
    }

    public void UpdateExplosion(float deltaTime)
    {
        if (!IsShowingEffect)
        {
            return;
        }

        _effectTimer -= deltaTime;
        if (_effectTimer <= 0f)
        {
            _effectTimer = 0f;
            IsShowingEffect = false;
        }
    }

    public override void Update(float deltaTime)
    {
        if (IsShowingEffect)
        {
            UpdateExplosion(deltaTime);
            return;
        }

        _moveTimer += deltaTime;

        if (_moveTimer < k_MoveInterval)
        {
            return;
        }

        if (Input.IsKey(ConsoleKey.LeftArrow))
        {
            X = Math.Max(_minX, X - 1);
            _moveTimer = 0f;
            return;
        }

        if (Input.IsKey(ConsoleKey.RightArrow))
        {
            X = Math.Min(_maxX, X + 1);
            _moveTimer = 0f;
        }
    }

    public override void Draw(ScreenBuffer buffer)
    {
        if (IsShowingEffect)
        {
            buffer.SetCell(X, Y, '※', ConsoleColor.Red);
            return;
        }

        buffer.SetCell(X - 1, Y, '/', ConsoleColor.Cyan);
        buffer.SetCell(X, Y, 'A', ConsoleColor.White);
        buffer.SetCell(X + 1, Y, '\\', ConsoleColor.Cyan);
    }
}