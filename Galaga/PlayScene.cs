using System;
using System.Collections.Generic;
using Framework.Engine;

public class PlayScene : Scene
{
    private const int k_EnemyRows = 2;  // 적의 행 수
    private const int k_EnemyCols = 4;  // 적의 열 수
    private const float k_EnemyStepInterval = 0.5f;    // 적이 한 칸 이동하는 간격 (초)

    private Wall _wall;
    private Galaga _player;
    private readonly List<Enemy> _enemies = new List<Enemy>();  // 적 리스트
    private readonly List<Bullet> _bullets = new List<Bullet>(); // 총알 리스트
    private float _enemyStepTimer;  // 적 이동 타이머
    private int _enemyDirection = 1;    // 적 이동 방향 (1: 오른쪽, -1: 왼쪽)
    private int _blinkCounter;  // 게임 오버/클리어 메시지 깜빡임 카운터
    private int _score; // 플레이어 점수
    private bool _isGameOver;   // 게임 오버 상태 여부
    private bool _isClear;  // 게임 클리어 상태 여부
    private int _currentPosition = 0;

    public event GameAction PlayAgainRequested;

    public override void Load()
    {
        _score = 0;
        _isGameOver = false;
        _isClear = false;
        _blinkCounter = 0;
        _enemyDirection = 1;
        _enemyStepTimer = 0f;

        _enemies.Clear();
        _bullets.Clear();
        ClearGameObjects();

        _wall = new Wall(this); // 벽 생성
        AddGameObject(_wall);

        _player = new Galaga(this, (Wall.Left + Wall.Right) / 2, Wall.Bottom - 1, Wall.Left + 1, Wall.Right - 1);   // 플레이어 생성, 시작 위치는 벽의 중앙 아래, 이동 범위는 벽 내부로 제한
        AddGameObject(_player);

        SpawnEnemies(); // 적 생성, 벽 내부의 상단에 적들을 배치
    }

    public override void Unload()   // 게임 종료 시 리소스 정리, 게임 오브젝트 리스트와 적, 총알 리스트를 모두 초기화하여 메모리 해제
    {
        ClearGameObjects(); // 게임 오브젝트 리스트 초기화
        _enemies.Clear();   // 적 리스트 초기화
        _bullets.Clear();   // 총알 리스트 초기화
    }

    public override void Update(float deltaTime)    // 게임 로직 업데이트, 게임 오버 또는 클리어 상태에 따라 입력 처리 및 게임 오브젝트 업데이트를 수행
    {
        if (_isGameOver || _isClear)    // 게임 오버 또는 클리어 상태인 경우, 메시지 깜빡임 처리 및 재시작 입력 대기
        {
            _blinkCounter++;
            if (Input.IsKeyDown(ConsoleKey.Enter))
            {
                PlayAgainRequested?.Invoke();
            }
            return;
        }

        UpdateGameObjects(deltaTime);
        HandleShootInput(); // 플레이어의 총알 발사 입력 처리
        MoveEnemies(deltaTime); // 적 이동 처리
        ResolveCollisions();    // 총알과 적의 충돌 처리
        CleanupInactiveObjects();  // 비활성화된 게임 오브젝트 정리
        CheckEndConditions();   // 게임 종료 조건 확인
    }

    private void SpawnEnemies() // 적 생성 메서드, 벽 내부의 상단에 적들을 배치하기 위해 시작 좌표를 계산하고, 이중 루프를 통해 적들을 생성하여 게임 오브젝트로 추가
    {
        int startX = Wall.Left + 3; // 적 시작 X 좌표, 벽의 왼쪽에서 3칸 떨어진 위치
        int startY = Wall.Top + 2;  // 적 시작 Y 좌표, 벽의 위쪽에서 2칸 떨어진 위치

        for (int row = 0; row < k_EnemyRows; row++)
        {
            for (int col = 0; col < k_EnemyCols; col++)
            {
                Enemy enemy = new Enemy(this, startX + (col * 4), startY + (row * 2));  // 적 생성, 각 적은 열마다 4칸, 행마다 2칸 간격으로 배치
                _enemies.Add(enemy);
                AddGameObject(enemy);
            }
        }
    }

    private void HandleShootInput() // 플레이어의 총알 발사 입력 처리 메서드, 스페이스바가 눌렸고 플레이어가 총알을 발사할 수 있는 상태인 경우, 새로운 총알을 생성하여 게임 오브젝트로 추가하고, 플레이어의 발사 쿨다운을 초기화
    {
        if (Input.IsKeyDown(ConsoleKey.Spacebar) && _player.CanShoot())
        {
            Bullet bullet = new Bullet(this, _player.X, _player.Y - 1, false);
            _bullets.Add(bullet);
            AddGameObject(bullet);
            _player.MarkShotFired();
        }
    }

    private void MoveEnemies(float deltaTime)   // 적 이동 메서드, 일정 간격마다 좌우로만 이동하며 벽에 닿으면 방향을 반전
    {
        _currentPosition++;
        _enemyStepTimer += deltaTime;
        if (_enemyStepTimer < k_EnemyStepInterval)
        {
            return;
        }

        _enemyStepTimer = 0f;


        for (int i = 0; i < _enemies.Count; i++)    // 적 리스트를 순회하여 각 적이 다음 이동 위치에서 벽에 닿는지 확인, 만약 닿는 경우 방향을 반전하도록 플래그 설정
        {
            Enemy enemy = _enemies[i];
            if (!enemy.IsActive)    // 비활성화된 적은 이동하지 않으므로 건너뜀
            {
                continue;
            }

            int nextX = enemy.X + _enemyDirection;  // 다음 이동 위치의 X 좌표 계산, 현재 적의 X 좌표에 이동 방향을 더하여 다음 위치를 예측

            if (_currentPosition % 2 == 0)   // 현재 위치에서 한칸씩만 이동
            {
                _enemyDirection *= -1;    // 이동 방향 반전, 다음 이동에서 반대 방향으로 이동하도록 설정
                break;  // 방향이 반전되면 나머지 적들은 아직 이동하지 않았으므로 루프를 종료하여 다음 업데이트에서 이동하도록 함
            }
            else if (_currentPosition % 2 == 1)   // 현재 위치에서 한칸씩만 이동
            {
                _enemyDirection *= +1;    // 이동 방향 반전, 다음 이동에서 반대 방향으로 이동하도록 설정
                break;  // 방향이 반전되면 나머지 적들은 아직 이동하지 않았으므로 루프를 종료하여 다음 업데이트에서 이동하도록 함
            }
        }

        for (int i = 0; i < _enemies.Count; i++)    // 적 리스트를 순회하여 각 적이 활성화된 경우, 현재 방향에 따라 이동하도록 처리
        {
            if (_enemies[i].IsActive)
            {
                _enemies[i].MoveBy(_enemyDirection);
            }
        }
    }

    private void ResolveCollisions()    // 총알과 적의 충돌을 처리하는 메서드, 이중 루프를 통해 활성화된 총알과 적을 비교하여 위치가 일치하는 경우, 해당 적과 총알을 비활성화하고 점수를 증가시킴
    {
        for (int i = 0; i < _bullets.Count; i++)    // 총알 리스트를 순회하여 활성화된 총알과 적의 충돌을 확인
        {
            Bullet bullet = _bullets[i];
            if (!bullet.IsActive)
            {
                continue;
            }

            for (int j = 0; j < _enemies.Count; j++)
            {
                Enemy enemy = _enemies[j];
                if (!enemy.IsActive)
                {
                    continue;
                }

                if (enemy.X == bullet.X && enemy.Y == bullet.Y) // 총알과 적의 위치가 일치하는 경우, 충돌로 간주하여 해당 적과 총알을 비활성화하고 점수를 증가시킴
                {
                    enemy.IsActive = false;
                    bullet.IsActive = false;
                    _score += 10;
                    break;
                }
            }
        }
    }

    private void CleanupInactiveObjects()    // 비활성화된 게임 오브젝트를 정리하는 메서드, 리스트에서 제거하고 메모리에서 해제
    {
        for (int i = _bullets.Count - 1; i >= 0; i--)   // 총알 리스트를 역순으로 순회하여 비활성화된 총알을 제거
        {
            if (_bullets[i].IsActive)
            {
                continue;
            }

            RemoveGameObject(_bullets[i]);
            _bullets.RemoveAt(i);
        }

        for (int i = _enemies.Count - 1; i >= 0; i--)   // 적 리스트를 역순으로 순회하여 비활성화된 적을 제거
        {
            if (_enemies[i].IsActive)
            {
                continue;
            }

            RemoveGameObject(_enemies[i]);
            _enemies.RemoveAt(i);
        }
    }

    private void CheckEndConditions()   // 게임 종료 조건을 확인하는 메서드, 적이 모두 사라졌는지 또는 플레이어가 적에게 닿았는지 확인
    {
        if (_enemies.Count == 0)
        {
            _isClear = true;
            return;
        }

        for (int i = 0; i < _enemies.Count; i++)
        {
            if (_enemies[i].Y >= _player.Y)
            {
                _isGameOver = true;
                return;
            }
        }
    }

    public override void Draw(ScreenBuffer buffer)  // 게임 화면을 그리는 메서드, 게임 오브젝트를 그린 후 점수와 조작법을 표시하고, 게임 오버 또는 클리어 상태인 경우 메시지를 깜빡이도록 처리
    {
        DrawGameObjects(buffer);
        buffer.WriteText(1, 0, $"Score: {_score}", ConsoleColor.White);
        buffer.WriteText(1, 1, "Move: Left/Right", ConsoleColor.DarkGray);
        buffer.WriteText(1, 2, "Fire: Space", ConsoleColor.DarkGray);

        if (_isGameOver)
        {
            if (_blinkCounter % 2 == 0)
            {
                buffer.WriteTextCentered(12, "GAME OVER", ConsoleColor.Red);
            }

            buffer.WriteTextCentered(14, $"Score: {_score}", ConsoleColor.White);
            buffer.WriteTextCentered(16, "Press ENTER to retry", ConsoleColor.White);
        }

        if (_isClear)
        {
            if (_blinkCounter % 2 == 0)
            {
                buffer.WriteTextCentered(12, "STAGE CLEAR", ConsoleColor.Green);
            }

            buffer.WriteTextCentered(14, $"Score: {_score}", ConsoleColor.White);
            buffer.WriteTextCentered(16, "Press ENTER to play again", ConsoleColor.White);
        }
    }
}