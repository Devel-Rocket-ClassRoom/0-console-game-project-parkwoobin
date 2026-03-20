using System;
using System.Collections.Generic;
using Framework.Engine;

// 벽을 나타내는 클래스, 게임의 경계 역할을 하며 플레이어와 적이 이 영역을 벗어나지 못하도록 함
public class Wall : GameObject
{
    public const int Left = 1;
    public const int Top = 1;
    public const int Right = 38;
    public const int Bottom = 18;

    // 별 도트
    private static readonly char[] s_StarDots = new[] { '⠐', '⠨', '⢐', '⠐', '⡠', '⠁', '⠅', '⠐', '⠡', '⠐', };
    private static readonly Random s_Random = new Random();
    private static readonly object s_StarLock = new object();
    private static readonly List<Star> s_SharedStars = new List<Star>();
    private static bool s_IsSharedStarFieldReady;
    private readonly List<Star> _stars = new List<Star>();

    private readonly struct Star
    {
        public Star(int x, int y, char glyph, ConsoleColor color)
        {
            X = x;
            Y = y;
            Glyph = glyph;
            Color = color;
        }

        public int X { get; }
        public int Y { get; }
        public char Glyph { get; }
        public ConsoleColor Color { get; }
    }

    public Wall(Scene scene) : base(scene)
    {
        Name = "Wall";
        BuildStarField();
    }

    public override void Update(float deltaTime)
    {
        // 벽은 고정 배경만 그리므로 업데이트 로직이 필요 없음
    }

    public override void Draw(ScreenBuffer buffer)  // 벽을 그리는 메서드, 사각형 형태로 벽을 그리기 위해 DrawBox 메서드 사용
    {
        buffer.DrawBox(Left - 1, Top - 1, Right - Left + 3, Bottom - Top + 3, ConsoleColor.White);

        for (int i = 0; i < _stars.Count; i++)
        {
            Star star = _stars[i];
            buffer.SetCell(star.X, star.Y, star.Glyph, star.Color);
        }
    }

    private void BuildStarField()   // 벽 내부에서 랜덤으로 별을 생성하는 메소드
    {
        _stars.Clear();

        lock (s_StarLock)
        {
            if (!s_IsSharedStarFieldReady)
            {
                // 벽 내부의 전체 영역을 계산하여 별의 개수를 결정, 각 별은 랜덤한 위치와 모양, 색상을 가지도록 생성
                int width = (Right - Left) + 1;
                int height = (Bottom - Top) + 1;
                int starCount = (width * height) / 11;

                // 별의 개수는 벽 내부의 전체 영역을 11로 나눈 값으로 설정, 이 값은 적절한 밀도를 유지하기 위한 경험적 수치
                for (int i = 0; i < starCount; i++)
                {
                    int x = s_Random.Next(Left, Right + 1);
                    int y = s_Random.Next(Top, Bottom + 1);
                    char glyph = s_StarDots[s_Random.Next(s_StarDots.Length)];
                    ConsoleColor color = (s_Random.NextDouble() < 0.2) ? ConsoleColor.White : ConsoleColor.Gray;
                    s_SharedStars.Add(new Star(x, y, glyph, color));
                }

                s_IsSharedStarFieldReady = true;
            }

            _stars.AddRange(s_SharedStars);
        }
    }

    public bool IsInBounds(int x, int y)    // 주어진 좌표가 벽의 내부에 있는지 확인하는 메서드, 벽의 경계 좌표와 비교하여 판단
    {
        return x >= Left && x <= Right && y >= Top && y <= Bottom;
    }

    public bool IsInBounds((int x, int y) position) // 좌표를 튜플 형태로 받아서 벽의 내부에 있는지 확인하는 오버로드된 메서드, 기존 IsInBounds 메서드를 호출하여 판단
    {
        return IsInBounds(position.x, position.y);
    }
}