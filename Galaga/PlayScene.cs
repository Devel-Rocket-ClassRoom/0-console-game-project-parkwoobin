using System;
using System.Collections.Generic;
using Framework.Engine;

public class PlayScene : Scene
{
    private const float k_EnemyStepInterval = 1f;    // 적이 한 칸 이동하는 간격 (초)
    private const float k_SecondShotDelay = 0.3f;   // 첫 발 이후 두 번째 발 발사까지 필요한 최소 시간 (초)
    private const float k_EnemyAttackInterval = 0.5f;    // 적 공격 시도 간격 (초)
    private const double k_EnemyAttackChance = 0.2;      // 공격 시도 시 실제 발사 확률

    private Wall _wall;
    private Galaga _player;
    private readonly List<Enemy> _enemies = new List<Enemy>();  // 적 리스트
    private readonly List<Bullet> _bullets = new List<Bullet>(); // 총알 리스트
    private readonly Random _random = new Random();
    private float _enemyStepTimer;  // 적 이동 타이머
    private int _enemyDirection = 1;    // 적 이동 방향 (1: 오른쪽, -1: 왼쪽)
    private int _blinkCounter;  // 게임 오버/클리어 메시지 깜빡임 카운터
    private int _score; // 플레이어 점수
    private int _stage; // 현재 스테이지 번호
    private bool _isGameOver;   // 게임 오버 상태 여부
    private bool _isClear;  // 게임 클리어 상태 여부
    private bool _isAllClear;   // 전체 스테이지 클리어 여부
    private float _secondShotTimer; // 두 번째 발사 가능 타이머
    private float _enemyAttackTimer; // 적 공격 시도 타이머

    public event GameAction PlayAgainRequested;
    public event GameAction ClearRequested;

    public int Score => _score;
    public int Stage => _stage;
    public bool IsAllClear => _isAllClear;

    private int _initialScore;
    private int _initialStage;

    public PlayScene() : this(0, 1)
    {
    }

    public PlayScene(int initialScore, int initialStage)
    {
        _initialScore = initialScore;
        _initialStage = initialStage;
    }

    public override void Load()
    {
        _score = _initialScore;
        _stage = _initialStage;
        _isGameOver = false;
        _isClear = false;
        _isAllClear = false;
        _blinkCounter = 0;
        _enemyDirection = 1;
        _enemyStepTimer = 0f;
        _secondShotTimer = 0f;
        _enemyAttackTimer = 0f;

        _enemies.Clear();
        _bullets.Clear();
        ClearGameObjects();

        _wall = new Wall(this); // 벽 생성
        AddGameObject(_wall);

        _player = new Galaga(this, (Wall.Left + Wall.Right) / 2, Wall.Bottom - 1, Wall.Left + 1, Wall.Right - 1);   // 플레이어 생성, 시작 위치는 벽의 중앙 아래, 이동 범위는 벽 내부로 제한
        AddGameObject(_player);

        SpawnEnemies(); // 스테이지 데이터 기준으로 적 생성
    }

    public override void Unload()   // 게임 종료 시 리소스 정리, 게임 오브젝트 리스트와 적, 총알 리스트를 모두 초기화하여 메모리 해제
    {
        ClearGameObjects(); // 게임 오브젝트 리스트 초기화
        _enemies.Clear();   // 적 리스트 초기화
        _bullets.Clear();   // 총알 리스트 초기화
    }

    public override void Update(float deltaTime)    // 게임 로직 업데이트, 게임 오버 또는 클리어 상태에 따라 입력 처리 및 게임 오브젝트 업데이트를 수행
    {
        if (_isGameOver)
        {
            _blinkCounter++;
            if (Input.IsKeyDown(ConsoleKey.Enter))
            {
                PlayAgainRequested?.Invoke();
            }
            return;
        }

        if (_isClear || _isAllClear)
        {
            ClearRequested?.Invoke();
            return;
        }

        UpdateGameObjects(deltaTime);
        HandleShootInput(deltaTime); // 플레이어의 단발 입력 처리(동시 최대 2발)
        MoveEnemies(deltaTime); // 적 이동 처리
        HandleEnemyChargeCollision(); // 적 돌진(접촉) 충돌 처리
        HandleEnemyAttack(deltaTime); // 적의 간헐적 공격 처리
        ResolveCollisions();    // 총알과 적의 충돌 처리
        CleanupInactiveObjects();  // 비활성화된 게임 오브젝트 정리
        CheckEndConditions();   // 게임 종료 조건 확인
    }

    private void SpawnEnemies() // 스테이지 데이터 파일에 정의된 적 배치를 생성
    {
        StageDefinition stageDefinition = StageData.Get(_stage);
        foreach (EnemySpawn spawn in stageDefinition.Enemies)
        {
            Enemy enemy = new Enemy(this, spawn.X, spawn.Y, spawn.Type);
            _enemies.Add(enemy);
            AddGameObject(enemy);
        }
    }

    private void ClearEnemiesAndBullets()   // 적과 총알을 모두 제거하는 메서드, 게임 오브젝트 리스트에서 제거하고 메모리에서 해제
    {
        for (int i = _bullets.Count - 1; i >= 0; i--)
        {
            RemoveGameObject(_bullets[i]);
            _bullets.RemoveAt(i);
        }

        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            RemoveGameObject(_enemies[i]);
            _enemies.RemoveAt(i);
        }
    }

    private void HandleShootInput(float deltaTime)  // 플레이어의 총알 발사 입력을 처리하는 메서드, 스페이스바 입력과 동시에 최대 2발까지 발사 가능하며, 첫 발 이후 두 번째 발은 일정 시간 지연 후에만 발사 가능하도록 처리
    {
        if (_secondShotTimer > 0f)
        {
            _secondShotTimer -= deltaTime;
        }

        if (!Input.IsKeyDown(ConsoleKey.Spacebar))
        {
            return;
        }

        int activePlayerBullets = CountActivePlayerBullets();
        if (activePlayerBullets >= 2)
        {
            return;
        }

        // 첫 발 이후 두 번째 발은 설정된 지연 시간이 지나야 발사 가능
        if (activePlayerBullets == 1 && _secondShotTimer > 0f)
        {
            return;
        }

        FirePlayerBullet();

        if (activePlayerBullets == 0)
        {
            _secondShotTimer = k_SecondShotDelay;
        }
    }

    private int CountActivePlayerBullets()  // 플레이어가 발사한 활성화된 총알의 수를 세는 메서드, 총알 리스트를 순회하여 플레이어 총알 중 활성화된 총알의 개수를 반환
    {
        int count = 0;

        for (int i = 0; i < _bullets.Count; i++)
        {
            Bullet bullet = _bullets[i];
            if (bullet.IsActive && !bullet.IsEnemyBullet)
            {
                count++;
            }
        }

        return count;
    }

    private void FirePlayerBullet() // 플레이어가 총알을 발사하는 메서드, 플레이어의 현재 위치에서 총알을 생성하여 게임 오브젝트 리스트에 추가
    {
        Bullet bullet = new Bullet(this, _player.X, _player.Y - 1, false);
        _bullets.Add(bullet);
        AddGameObject(bullet);
    }

    private void HandleEnemyAttack(float deltaTime) // 적의 공격을 처리하는 메서드, 일정 간격마다 공격 시도를 하며, 동시에 존재할 수 있는 적 총알의 최대 개수를 제한하고, 공격 시도 시 실제 발사 확률을 적용하여 총알을 생성
    {
        _enemyAttackTimer += deltaTime;
        if (_enemyAttackTimer < k_EnemyAttackInterval)
        {
            return;
        }

        _enemyAttackTimer = 0f;
        Enemy shooter = EnemyAttack.PickShooter(_enemies, _bullets, _random, k_EnemyAttackChance);
        if (shooter == null)
        {
            return;
        }

        Bullet bullet = new Bullet(this, shooter.X, shooter.Y + 1, true);
        _bullets.Add(bullet);
        AddGameObject(bullet);
    }

    private void HandleEnemyChargeCollision()   // 적이 유저 좌표와 겹치는지 판정하여 충돌 시 게임 오버 처리
    {
        for (int i = 0; i < _enemies.Count; i++)
        {
            Enemy enemy = _enemies[i];
            if (EnemyAttack.CrushEnemy(enemy, _player.X, _player.Y))
            {
                _isGameOver = true;
                return;
            }
        }
    }

    private void MoveEnemies(float deltaTime)   // 적 이동 메서드, 매 스텝마다 좌/우를 랜덤 선택해 1칸만 이동
    {
        _enemyStepTimer += deltaTime;
        if (_enemyStepTimer < k_EnemyStepInterval)
        {
            return;
        }

        _enemyStepTimer = 0f;
        int direction = _random.Next(0, 2) == 0 ? -1 : 1;

        // 랜덤으로 고른 방향이 막혀 있으면 반대 방향으로 1칸 이동 시도
        if (!CanMoveEnemiesBy(direction))
        {
            if (CanMoveEnemiesBy(-direction))
            {
                direction *= -1;
            }
            else
            {
                return;
            }
        }

        _enemyDirection = direction;

        for (int i = 0; i < _enemies.Count; i++)
        {
            if (_enemies[i].IsActive)
            {
                _enemies[i].MoveBy(direction);
            }
        }
    }

    private bool CanMoveEnemiesBy(int dx)   // 적이 dx만큼 이동할 수 있는지 확인하는 메서드, 적 리스트를 순회하여 각 적이 이동하려는 방향으로 벽의 범위를 벗어나지 않는지 확인
    {
        if (dx == 0)
        {
            return false;
        }

        for (int i = 0; i < _enemies.Count; i++)
        {
            Enemy enemy = _enemies[i];
            if (!enemy.IsActive)
            {
                continue;
            }

            int halfWidth = enemy.Type == Enemy.EnemyType.Boss1 || enemy.Type == Enemy.EnemyType.Boss2 ? 2 : 1;
            int nextLeft = (enemy.X + dx) - halfWidth;
            int nextRight = (enemy.X + dx) + halfWidth;

            if (nextLeft < Wall.Left || nextRight > Wall.Right)
            {
                return false;
            }
        }

        return true;
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

            if (bullet.IsEnemyBullet)
            {
                if (bullet.Y == _player.Y && bullet.X >= _player.X - 1 && bullet.X <= _player.X + 1)
                {
                    bullet.IsActive = false;
                    _isGameOver = true;
                    return;
                }

                continue;
            }

            for (int j = 0; j < _enemies.Count; j++)
            {
                Enemy enemy = _enemies[j];
                if (!enemy.IsActive)
                {
                    continue;
                }

                if (enemy.IsHitAt(bullet.X, bullet.Y)) // 날개를 포함한 적의 전체 폭과 총알 위치를 비교
                {
                    bullet.IsActive = false;

                    bool isDestroyed = enemy.ApplyHit();
                    if (isDestroyed)
                    {
                        _score += 10;
                    }

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
            if (_stage >= StageData.MaxStage)
            {
                _isAllClear = true;
            }
            else
            {
                _isClear = true;
            }

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

        buffer.WriteText(45, 5, $"Score", ConsoleColor.Red);
        buffer.WriteText(48, 6, $"{_score}", ConsoleColor.White);
        buffer.WriteText(43, 15, $"Stage: {_stage}/{StageData.MaxStage}", ConsoleColor.White);
        buffer.WriteText(43, 0, "Move: Left/Right", ConsoleColor.DarkGray);
        buffer.WriteText(48, 1, "Fire: Space", ConsoleColor.DarkGray);



        if (_isGameOver)
        {
            if (_blinkCounter % 2 == 0)
            {
                buffer.WriteTextCentered(12, "GAME OVER", ConsoleColor.Red);
            }

            buffer.WriteTextCentered(14, $"Score: {_score}", ConsoleColor.White);
            buffer.WriteTextCentered(16, "Press ENTER to retry", ConsoleColor.White);
        }
    }
}