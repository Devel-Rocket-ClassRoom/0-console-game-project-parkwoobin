using System;
using System.Threading;

namespace Framework.Engine
{
    public abstract class GameApp
    {
        private const int k_TargetFrameTime = 33;   //33ms ~ 30 FPS 업데이트
        private bool _isRunning;    // 게임 실행 여부 확인

        protected ScreenBuffer Buffer { get; private set; }     // 그리기 역할 ScreenBuffer 인스턴스

        public event GameAction GameStarted;    // 게임 시작 이벤트
        public event GameAction GameStopped;    // 게임 종료 이벤트

        protected GameApp(int width, int height)    // 콘솔 창 크기 설정 및 ScreenBuffer 초기화
        {
            Buffer = new ScreenBuffer(width, height);
        }

        public void Run()
        {
            Console.CursorVisible = false;  // 깜빡 거리는 콘솔 커서 숨김
            Console.Clear();    // 콘솔 초기화

            _isRunning = true;  // 게임 실행 상태 설정
            Initialize();   // 게임 초기화 메서드 호출
            GameStarted?.Invoke();  // 게임 시작 이벤트 발생

            int previousTime = Environment.TickCount;   // 이전 프레임 시간 초기화

            while (_isRunning)  // 게임 루프 시작
            {
                // 프레임 시간 계산
                // 각 프레임마다의 시간을 계산하여 게임 업데이트 및 렌더링에 사용
                int currentTime = Environment.TickCount;
                float deltaTime = (currentTime - previousTime) / 1000f;
                previousTime = currentTime;

                // 입력 처리
                Input.Poll();
                Update(deltaTime);

                // 화면 그리기
                Buffer.Clear();
                Draw();
                Buffer.Present();

                int elapsed = Environment.TickCount - currentTime;
                int sleepTime = k_TargetFrameTime - elapsed;
                if (sleepTime > 0)  // 설정해둔 프레임에 맞추려고 남은 시간만큼 대기
                {
                    Thread.Sleep(sleepTime);
                }
            }

            // 게임 종료 이벤트 발생 및 콘솔 상태 초기화
            GameStopped?.Invoke();
            Console.CursorVisible = true;
            Console.ResetColor();
            Console.Clear();
        }

        protected void Quit()
        {
            _isRunning = false; // 게임 루프 종료를 위한 플래그 설정
        }

        /// <summary>
        /// 게임 초기화, 업데이트, 그리기 메서드는 게임마다 다르게 구현되어야 하므로 추상 메서드로 정의
        /// </summary>
        protected abstract void Initialize();
        protected abstract void Update(float deltaTime);
        protected abstract void Draw();
    }
}
