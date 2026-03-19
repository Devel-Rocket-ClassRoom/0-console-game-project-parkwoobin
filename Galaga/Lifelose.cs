using System;
using Framework.Engine;

// 목숨을 잃었을 때 READY를 표시하고 재시작 타이밍을 관리하는 클래스
public class Lifelose
{
    private const float k_ReadyDuration = 2f;   // READY 메시지가 표시되는 시간 (초)

    private float _timer;
    public bool IsActive { get; private set; }

    public void Begin()
    {
        _timer = 0f;
        IsActive = true;
    }

    public void Reset()
    {
        _timer = 0f;
        IsActive = false;
    }

    // true를 반환하면 READY 대기가 끝나고 게임을 재개할 수 있음
    public bool Update(float deltaTime)
    {
        if (!IsActive)
        {
            return false;
        }

        _timer += deltaTime;
        if (_timer >= k_ReadyDuration)  // READY 대기가 끝나면 활성화 상태를 해제하고 true 반환
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
    }
}
