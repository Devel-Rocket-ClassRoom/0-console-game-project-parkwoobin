using System;
using Framework.Engine;

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

    public override void Draw(ScreenBuffer buffer)
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