using System;
using Framework.Engine;

public class Galaga : GameObject
{
    private const float k_MoveInterval = 0.05f; // 플레이어가 한 칸 이동하는 간격 (초)
    private const float k_FireCooldown = 0.5f; // 플레이어가 총알을 발사한 후 다음 발사까지 필요한 최소 시간 (초)

    private readonly int _minX;
    private readonly int _maxX;
    private float _moveTimer;
    private float _fireTimer;

    public int X { get; private set; }
    public int Y { get; }

    public Galaga(Scene scene, int startX, int startY, int minX, int maxX) : base(scene)
    {
        Name = "Player";
        X = startX;
        Y = startY;
        _minX = minX;
        _maxX = maxX;
    }

    public override void Update(float deltaTime)
    {
        _moveTimer += deltaTime;
        _fireTimer -= deltaTime;

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

    public bool CanShoot()
    {
        return _fireTimer <= 0f;
    }

    public void MarkShotFired()
    {
        _fireTimer = k_FireCooldown;
    }

    public override void Draw(ScreenBuffer buffer)
    {
        buffer.SetCell(X - 1, Y, '/', ConsoleColor.Cyan);
        buffer.SetCell(X, Y, 'A', ConsoleColor.White);
        buffer.SetCell(X + 1, Y, '\\', ConsoleColor.Cyan);
    }
}