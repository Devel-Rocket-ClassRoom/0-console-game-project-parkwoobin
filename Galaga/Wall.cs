using System;
using Framework.Engine;

public class Wall : GameObject
{
    public const int Left = 1;
    public const int Top = 1;
    public const int Right = 38;
    public const int Bottom = 18;
    public Wall(Scene scene) : base(scene)
    {
        Name = "Wall";

    }

    public override void Update(float deltaTime)
    {
        // 벽은 움직이지 않으므로 업데이트 로직이 필요 없음
    }

    public override void Draw(ScreenBuffer buffer)  // 벽을 그리는 메서드, 사각형 형태로 벽을 그리기 위해 DrawBox 메서드 사용
    {
        buffer.DrawBox(Left - 1, Top - 1, Right - Left + 3, Bottom - Top + 3, ConsoleColor.White);
    }

    public bool IsInBounds(int x, int y)    // 주어진 좌표가 벽의 내부에 있는지 확인하는 메서드, 벽의 경계 좌표와 비교하여 판단
    {
        return x >= Left && x <= Right && y >= Top && y <= Bottom;
    }

    public bool IsInBounds((int x, int y) position) // 좌표를 튜플 형태로 받아서 벽의 내부에 있는지 확인하는 오버로드된 메서드, 기존 IsInBounds 메서드를 호출하여 판단
    {
        return IsInBounds(position.x, position.y);
    }
}