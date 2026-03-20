using System;
using Framework.Engine;

// 목숨을 잃었을 때 READY를 표시하고 재시작 타이밍을 관리하는 클래스
public class Lifelose
{

    public bool IsActive { get; private set; }

    public void Begin()
    {
        IsActive = true;
    }

    public void Reset()
    {
        IsActive = false;
    }

    // true를 반환하면 READY 대기가 끝나고 게임을 재개할 수 있음
    public bool Update(float deltaTime)
    {
        if (!IsActive)
        {
            return false;
        }

        if (Input.IsKeyDown(ConsoleKey.Enter)) // 엔터 키가 눌렸는지 확인하여 게임 시작
        {
            IsActive = false;
            return true;
        }

        return false;
    }

    public void Draw(ScreenBuffer buffer)
    {
        if (!IsActive)
        {
            return;
        }

        buffer.WriteText(18, 10, "READY", ConsoleColor.Red);
        buffer.WriteText(9, 11, "Press Enter to Continue", ConsoleColor.Red);
    }
}
