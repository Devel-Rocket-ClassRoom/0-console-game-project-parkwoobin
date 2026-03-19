using System;
using Framework.Engine;

// 게임의 클리어 타이틀 화면을 나타내는 클래스, 스테이지 클리어 후 점수와 스테이지 정보를 표시하고, 다음 단계로 진행하거나 타이틀 화면으로 돌아가는 입력 처리를 담당
public class ClearTitle : Scene
{
    private int _blinkCounter;
    private int _score;
    private int _stage;
    private int _maxStage;
    private bool _isAllClear;

    public event GameAction ContinueRequested;

    public bool IsAllClear => _isAllClear;

    public ClearTitle(int score, int stage, int maxStage, bool isAllClear)
    {
        _score = score;
        _stage = stage;
        _maxStage = maxStage;
        _isAllClear = isAllClear;
    }

    public override void Load()
    {
        _blinkCounter = 0;
    }

    public override void Unload()
    {
    }

    public override void Update(float deltaTime)
    {
        _blinkCounter++;
        if (Input.IsKeyDown(ConsoleKey.Enter))
        {
            ContinueRequested?.Invoke();
        }
    }

    public override void Draw(ScreenBuffer buffer)  // 클리어 타이틀 화면을 그리는 메서드, 스테이지 클리어 메시지와 점수, 다음 단계로 진행 안내 메시지를 화면 중앙에 표시
    {
        if (_isAllClear)
        {
            if (_blinkCounter % 2 == 0)
            {
                buffer.WriteTextCentered(12, "ALL STAGES CLEAR", ConsoleColor.Yellow);
            }

            buffer.WriteTextCentered(14, $"Final Score: {_score}", ConsoleColor.White);
            buffer.WriteTextCentered(16, "Press ENTER to title", ConsoleColor.White);
        }
        else
        {
            if (_blinkCounter % 2 == 0)
            {
                buffer.WriteTextCentered(12, "STAGE CLEAR", ConsoleColor.Green);
            }

            buffer.WriteTextCentered(14, $"Score: {_score}", ConsoleColor.White);
            buffer.WriteTextCentered(16, "Press ENTER for next stage", ConsoleColor.White);
        }
    }
}