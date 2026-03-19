using Framework.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
// 게임의 타이틀 화면을 나타내는 클래스, 게임 시작을 위한 안내 메시지와 입력 처리를 담당
public class TitleScene : Scene
{

    public event GameAction StartRequested; // 게임 시작 요청 이벤트, 구독자에게 게임 시작을 알리는 역할
    public override void Load()
    {

    }

    public override void Unload()
    {

    }

    public override void Update(float deltaTime)
    {
        if (Input.IsKeyDown(ConsoleKey.Enter)) // 엔터 키가 눌렸는지 확인하여 게임 시작
        {
            StartRequested?.Invoke();
        }
    }

    public override void Draw(ScreenBuffer buffer)
    {
        buffer.WriteTextCentered(6, "Galaga ", ConsoleColor.Green);
        buffer.WriteTextCentered(10, "Arrow Keys: Move");
        buffer.WriteTextCentered(11, "Space: Fire");
        buffer.WriteTextCentered(13, "ESC: Quit");
        buffer.WriteTextCentered(15, "Press Enter to Start", ConsoleColor.Yellow);
    }
}