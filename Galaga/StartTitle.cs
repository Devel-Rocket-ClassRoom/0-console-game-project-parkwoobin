using System;
using Framework.Engine;
using System.IO;
using NAudio.Wave;


// 게임의 시작 연출을 담당하는 클래스, 스테이지 번호와 READY 메시지를 순차적으로 표시하며, 시작 음악 재생과 타이밍 관리를 수행
public class StartTitle : Scene
{
    private const float k_StartTextDuration = 5f; // START 텍스트 최대 표시 시간 (초)
    private const float k_StageTextDuration = 1f; // 스테이지 텍스트 표시 시간 (초)
    private const float k_ReadyTextDuration = 1f; // READY 텍스트 표시 시간 (초)

    private static readonly string s_StartMusicPath = Path.Combine(AppContext.BaseDirectory, "BGM", "002 Game Start Music.mp3");

    private enum Phase
    {
        Start,
        Stage,
        Ready,
        Done
    }

    private readonly int _stage;
    private WaveOutEvent _output;
    private AudioFileReader _audio;
    private float _phaseTimer;  // 현재 단계에서 경과된 시간 (초)
    private Phase _phase;
    private bool _eventRaised;

    public event GameAction StartCompleted;

    public StartTitle(int stage)
    {
        _stage = stage;
    }

    public override void Load()
    {
        _phase = Phase.Start;
        _phaseTimer = 0f;
        _eventRaised = false;

        if (!File.Exists(s_StartMusicPath))
        {
            return;
        }

        _audio = new AudioFileReader(s_StartMusicPath);
        _output = new WaveOutEvent();
        _output.Init(_audio);
        _output.Play();
    }

    public override void Unload()
    {
        if (_output != null)
        {
            _output.Stop();
            _output.Dispose();
            _output = null;
        }

        if (_audio != null)
        {
            _audio.Dispose();
            _audio = null;
        }
    }

    public override void Update(float deltaTime)    // 단계별로 타이머를 업데이트하고, 각 단계가 끝나면 다음 단계로 전환하며, 모든 단계가 완료되면 StartCompleted 이벤트를 발생시키는 업데이트 메서드
    {
        switch (_phase)
        {
            case Phase.Start:
                _phaseTimer += deltaTime;
                if (_phaseTimer >= k_StartTextDuration)
                {
                    _phase = Phase.Stage;
                    _phaseTimer = 0f;
                }
                break;
            case Phase.Stage:
                _phaseTimer += deltaTime;
                if (_phaseTimer >= k_StageTextDuration)
                {
                    _phase = Phase.Ready;
                    _phaseTimer = 0f;
                }
                break;
            case Phase.Ready:
                _phaseTimer += deltaTime;
                if (_phaseTimer >= k_ReadyTextDuration)
                {
                    _phase = Phase.Done;
                }
                break;
            case Phase.Done:
                if (!_eventRaised)
                {
                    _eventRaised = true;
                    StartCompleted?.Invoke();
                }
                break;
        }
    }

    public override void Draw(ScreenBuffer buffer)
    {
        // 플레이 화면과 동일한 벽/우측 안내 UI를 시작 연출에서도 유지
        buffer.DrawBox(Wall.Left - 1, Wall.Top - 1, Wall.Right - Wall.Left + 3, Wall.Bottom - Wall.Top + 3, ConsoleColor.White);
        buffer.WriteText(45, 5, "Score", ConsoleColor.Red);
        buffer.WriteText(48, 6, "0", ConsoleColor.White);
        buffer.WriteText(43, 15, $"Stage: {_stage}/{StageData.MaxStage}", ConsoleColor.White);
        buffer.WriteText(43, 0, "Move: Left/Right", ConsoleColor.DarkGray);
        buffer.WriteText(48, 1, "Fire: Space", ConsoleColor.DarkGray);
        buffer.WriteText(43, 10, "/A\\ /A\\ /A\\", ConsoleColor.White);


        switch (_phase)
        {
            case Phase.Start:
                buffer.WriteText(18, 10, "START", ConsoleColor.Red);
                break;
            case Phase.Stage:
                buffer.WriteText(15, 10, $"STAGE     {_stage}", ConsoleColor.Cyan);
                break;
            case Phase.Ready:
                buffer.WriteText(18, 10, "READY", ConsoleColor.Red);
                break;
        }
    }

}