using System;
using Framework.Engine;


public class GalagaGame : GameApp
{
    private readonly SceneManager<Scene> _scenes = new SceneManager<Scene>();
    public GalagaGame() : base(40, 20)
    {

    }
    public GalagaGame(int width, int height) : base(40, 20)
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
        title.StartRequested += ChangeToPlay; // 타이틀 신에서 게임 시작 요청 이벤트에 ChangeToPlay 메서드 구독
        _scenes.ChangeScene(title);

    }
    private void ChangeToPlay()
    {
        var play = new PlayScene();
        play.PlayAgainRequested += ChangeToTitle; // 플레이 신에서 게임 재시작 요청 이벤트에 ChangeToTitle 메서드 구독
        _scenes.ChangeScene(play); // 플레이 신으로 전환
    }
}