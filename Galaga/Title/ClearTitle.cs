using System;
using Framework.Engine;
using System.IO;
using NAudio.Wave;


// 게임의 클리어 타이틀 화면을 나타내는 클래스, 스테이지 클리어 후 점수와 스테이지 정보를 표시하고, 다음 단계로 진행하거나 타이틀 화면으로 돌아가는 입력 처리를 담당
public class ClearTitle : Scene
{
    private int _blinkCounter;
    private const float Readycount = 2f;
    private float _readycount;
    private int _score;
    private int _stage;
    private int _maxStage;
    private int _lifes;
    private bool _isAllClear;
    private Wall _wall;
    private Galaga _player;

    public event GameAction ContinueRequested;

    public bool IsAllClear => _isAllClear;

    private static readonly string AllClearBGM = Path.Combine(AppContext.BaseDirectory, "BGM", "019 Name Entry Music (High Score).mp3");

    private WaveOutEvent _allClearOutput;  // 스테이지 all clear 사운드 재생을 위한 WaveOutEvent과 AudioFileReader
    private AudioFileReader _allClearAudio;    // 스테이지 all clear 사운드 재생을 위한 WaveOutEvent과 AudioFileReader
    public ClearTitle(int score, int stage, int maxStage, int lifes, bool isAllClear)
    {
        _score = score;
        _stage = stage;
        _maxStage = maxStage;
        _lifes = lifes;
        _isAllClear = isAllClear;
    }

    public override void Load()
    {
        _blinkCounter = 0;
        _readycount = 0f;
        _wall = new Wall(this);

        if (!_isAllClear)
        {
            _player = new Galaga(this, (Wall.Left + Wall.Right) / 2, Wall.Bottom - 1, Wall.Left + 1, Wall.Right - 1);
        }
        else if (_isAllClear && File.Exists(AllClearBGM))
        {
            _allClearAudio = new AudioFileReader(AllClearBGM); // all clear 음악 파일을 로드하여 재생 준비
            _allClearOutput = new WaveOutEvent();   // WaveOutEvent 객체를 생성하여 오디오 출력 설정
            _allClearOutput.Init(_allClearAudio);   // 오디오 파일을 WaveOutEvent에 연결하여 재생 준비
            _allClearOutput.Play(); // all clear 음악 재생
        }


    }

    public override void Unload()
    {
        _allClearOutput?.Dispose();
        _allClearAudio?.Dispose();
    }

    public override void Update(float deltaTime)
    {
        _blinkCounter++;

        if (_isAllClear)
        {
            if (Input.IsKeyDown(ConsoleKey.Enter))
            {
                ContinueRequested?.Invoke();
            }

            return;
        }

        _player?.Update(deltaTime);

        if (_readycount < Readycount)
        {
            _readycount += deltaTime;
            return;
        }

        ContinueRequested?.Invoke();
    }

    public override void Draw(ScreenBuffer buffer)  // 클리어 타이틀 화면을 그리는 메서드, 스테이지 클리어 메시지와 점수, 다음 단계로 진행 안내 메시지를 화면 중앙에 표시
    {
        if (_isAllClear)
        {
            if (_blinkCounter % 2 == 0)
            {
                buffer.WriteTextCentered(8, "ALL STAGES CLEAR", ConsoleColor.Yellow);
            }

            buffer.WriteTextCentered(10, $"Final Score: {_score}", ConsoleColor.White);
            buffer.WriteTextCentered(14, "Press ENTER to title", ConsoleColor.White);
        }
        else if (_readycount <= Readycount)
        {
            _wall?.Draw(buffer);
            _player?.Draw(buffer);

            buffer.WriteText(45, 5, "Score", ConsoleColor.Red);
            buffer.WriteText(48, 6, $"{_score}", ConsoleColor.White);
            buffer.WriteText(43, 15, $"Stage: {_stage}/{_maxStage}", ConsoleColor.White);
            buffer.WriteText(43, 0, "Move: Left/Right", ConsoleColor.DarkGray);
            buffer.WriteText(48, 1, "Fire: Space", ConsoleColor.DarkGray);

            // 플레이어 목숨 수를 클리어 화면에도 동일하게 표시
            if (_lifes == 3)
            {
                buffer.WriteText(43, 10, "/A\\ /A\\", ConsoleColor.White);
            }
            else if (_lifes == 2)
            {
                buffer.WriteText(43, 10, "/A\\  ", ConsoleColor.White);
            }
            else if (_lifes == 1)
            {
                buffer.WriteText(43, 10, "  ", ConsoleColor.White);
            }

            buffer.WriteText(12, 10, $"{_stage} STAGE CLEAR", ConsoleColor.Cyan);

        }
    }
}