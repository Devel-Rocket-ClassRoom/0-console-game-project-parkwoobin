using System;
using System.Collections.Generic;
using Framework.Engine;

public class Bullet : GameObject
{
    private const float k_MoveInterval = 0.05f; // 총알이 한 칸 이동하는 간격 (초)

    private readonly int _directionY;
    private readonly LinkedList<(int X, int Y)> _path = new LinkedList<(int X, int Y)>();
    private LinkedListNode<(int X, int Y)> _headNode;
    private float _moveTimer;

    public int X => _headNode.Value.X;  // 총알의 현재 X 좌표
    public int Y => _headNode.Value.Y;  // 총알의 현재 Y 좌표
    public bool IsEnemyBullet { get; }

    public Bullet(Scene scene, int x, int y, bool isEnemyBullet) : base(scene)
    {
        Name = "Bullet";
        IsEnemyBullet = isEnemyBullet;
        _directionY = IsEnemyBullet ? 1 : -1;

        _headNode = _path.AddFirst((x, y));
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

        char bulletChar = IsEnemyBullet ? 'v' : '^';
        ConsoleColor color = IsEnemyBullet ? ConsoleColor.Red : ConsoleColor.Green;
        buffer.SetCell(X, Y, bulletChar, color);
    }
}