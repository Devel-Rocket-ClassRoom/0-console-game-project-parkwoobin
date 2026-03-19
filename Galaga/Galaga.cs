using System;
using Framework.Engine;

// 게임의 플레이어 캐릭터를 나타내는 클래스, 플레이어의 위치와 이동, 그리기 로직을 담당
public class Galaga : GameObject
{
    private const float k_MoveInterval = 0.05f; // 플레이어가 한 칸 이동하는 간격 (초)

    private readonly int _minX;
    private readonly int _maxX;
    private float _moveTimer;

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
        buffer.SetCell(X - 1, Y, '/', ConsoleColor.Cyan);
        buffer.SetCell(X, Y, 'A', ConsoleColor.White);
        buffer.SetCell(X + 1, Y, '\\', ConsoleColor.Cyan);
    }
}