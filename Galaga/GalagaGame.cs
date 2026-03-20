using System;
using Framework.Engine;

// 게임의 메인 클래스, 게임의 초기화, 업데이트, 그리기, 씬 전환 등을 관리하는 역할을 담당
public class GalagaGame : GameApp
{
    private readonly SceneManager<Scene> _scenes = new SceneManager<Scene>();
    public GalagaGame() : base(60, 20)
    {

    }
    public GalagaGame(int width, int height) : base(60, 20)
    {

    }

    protected override void Draw()
    {
        _scenes.CurrentScene?.Draw(Buffer); // 현재 신이 존재하면 그리기 호출
    }

    protected override void Initialize()
    {
        ChangeToTitle(); // 게임 시작 시 타이틀 신으로 전환
    }

    protected override void Update(float deltaTime)
    {
        if (Input.IsKeyDown(ConsoleKey.Escape)) // ESC 키가 눌렸는지 확인하여 게임 종료
        {
            Quit();
            return;
        }
        _scenes.CurrentScene?.Update(deltaTime); // 현재 신이 존재하면 업데이트 호출
    }

    private void ChangeToTitle()
    {
        var title = new TitleScene();
        title.StartRequested += () => ChangeToStartSequence(1, 0); // 타이틀 신에서 시작 연출로 전환
        _scenes.ChangeScene(title);

    }

    private void ChangeToStartSequence(int stage, int score)
    {
        var start = new StartTitle(stage);
        start.StartCompleted += () => ChangeToPlay(score, stage);
        _scenes.ChangeScene(start);
    }

    private void ChangeToPlay(int initialScore, int initialStage)
    {
        var play = new PlayScene(initialScore, initialStage);
        play.PlayAgainRequested += ChangeToTitle; // 플레이 신에서 게임 재시작 요청 이벤트에 ChangeToTitle 메서드 구독
        play.ClearRequested += () => ChangeToTitleClear(play); // 클리어 요청 이벤트에 ChangeToTitleClear 메서드 구독
        _scenes.ChangeScene(play); // 플레이 신으로 전환
    }

    private void ChangeToTitleClear(PlayScene playScene)
    {
        var clear = new ClearTitle(playScene.Score, playScene.Stage, StageData.MaxStage, playScene.Lifes, playScene.IsAllClear);
        clear.ContinueRequested += () => HandleClearContinue(playScene);
        _scenes.ChangeScene(clear);
    }

    private void HandleClearContinue(PlayScene playScene)
    {
        if (playScene.IsAllClear)
        {
            ChangeToTitle();
        }
        else
        {
            // 다음 스테이지를 위해 새로운 PlayScene 생성 (현재 점수와 다음 스테이지 전달)
            var nextPlay = new PlayScene(playScene.Score, playScene.Stage + 1, playScene.Lifes);
            nextPlay.PlayAgainRequested += ChangeToTitle;
            nextPlay.ClearRequested += () => ChangeToTitleClear(nextPlay);
            _scenes.ChangeScene(nextPlay);
        }
    }
}