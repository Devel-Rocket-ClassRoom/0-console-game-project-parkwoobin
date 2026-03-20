using System;
using System.Collections.Generic;
using Framework.Engine;

// 게임의 플레이 씬을 나타내는 클래스, 적의 생성과 이동, 충돌 처리, 게임 종료 조건 등을 관리하는 게임플로우 관련 메서드들을 포함
public partial class PlayScene : Scene
{
    private const float k_PlayerHitCooldown = 0.8f; // 피격 후 무적 시간
    private const float k_EnemyStepInterval = 1f;    // 적이 한 칸 이동하는 간격 (초)
    private const int k_EnemyInitialLeftMoveCount = 2; // 시작 시 좌측 이동 횟수(최초 1회)
    private const int k_EnemyRegularMoveCount = 4; // 이후 왕복 시 방향당 이동 횟수
    private const float k_EnemyAttackInterval = 0.5f;    // 적 공격 시도 간격 (초)
    private const double k_EnemyAttackChance = 0.2;      // 공격 시도 시 실제 발사 확률

    private Wall _wall;
    private Galaga _player;
    private readonly List<Enemy> _enemies = new List<Enemy>();  // 적 리스트
    private readonly List<Bullet> _bullets = new List<Bullet>(); // 총알 리스트
    private readonly Random _random = new Random();
    private EnemyChargeController _enemyChargeController;
    private Lifelose _lifelose;
    private float _enemyStepTimer;  // 적 이동 타이머
    private int _enemyDirection = -1;    // 적 이동 방향 (1: 오른쪽, -1: 왼쪽)
    private int _enemyMovesInCurrentPhase; // 현재 방향에서 이동한 횟수
    private bool _hasUsedInitialLeftPhase; // 시작 좌2 패턴을 이미 사용했는지 여부
    private int _blinkCounter;  // 게임 오버/클리어 메시지 깜빡임 카운터
    private int _score; // 플레이어 점수
    private int _stage; // 현재 스테이지 번호
    private bool _isGameOver;   // 게임 오버 상태 여부
    private bool _isClear;  // 게임 클리어 상태 여부
    private bool _isAllClear;   // 전체 스테이지 클리어 여부
    private float _enemyAttackTimer; // 적 공격 시도 타이머
    private float _playerHitCooldownTimer; // 플레이어 피격 쿨다운

    public event GameAction PlayAgainRequested;
    public event GameAction ClearRequested;

    public int Score => _score;
    public int Stage => _stage;
    public bool IsAllClear => _isAllClear;
    public int Lifes => _lifes;

    private int _initialScore;  // 초기 점수, 게임 재시작 시 이 점수로 초기화하여 유지할 수 있도록 함
    private int _initialStage;  // 초기 스테이지 번호, 게임 재시작 시 이 번호로 초기화하여 유지할 수 있도록 함
    private int _initialLifes;  // 초기 목숨 수, 다음 스테이지 전환 시 값을 유지하기 위해 사용

    private int _lifes;  // 플레이어 목숨 수, 게임 재시작 시 초기화하여 유지할 수 있도록 함

    public PlayScene() : this(0, 1, 3)
    {
    }

    public PlayScene(int initialScore, int initialStage)
        : this(initialScore, initialStage, 3)
    {
    }

    public PlayScene(int initialScore, int initialStage, int initialLifes)
    {
        _initialScore = initialScore;
        _initialStage = initialStage;
        _initialLifes = initialLifes < 0 ? 0 : initialLifes;
        _enemyChargeController = new EnemyChargeController(_random);
        _lifelose = new Lifelose();
    }

    public override void Load()
    {
        _score = _initialScore;
        _stage = _initialStage;
        _isGameOver = false;
        _isClear = false;
        _isAllClear = false;
        _blinkCounter = 0;
        _enemyDirection = -1;
        _enemyMovesInCurrentPhase = 0;
        _hasUsedInitialLeftPhase = false;
        _enemyStepTimer = 0f;
        _enemyAttackTimer = 0f;
        _playerHitCooldownTimer = 0f;
        _lifes = _initialLifes; // 플레이어 목숨 초기화 (스테이지 전환 시 전달된 값 유지)

        _enemies.Clear();
        _bullets.Clear();
        _enemyChargeController.Clear();
        _lifelose.Reset();
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
        _enemyChargeController.Clear();
        _lifelose.Reset();
    }

    public override void Update(float deltaTime)    // 게임 로직 업데이트, 게임 오버 또는 클리어 상태에 따라 입력 처리 및 게임 오브젝트 업데이트를 수행
    {
        if (_playerHitCooldownTimer > 0f)
        {
            _playerHitCooldownTimer -= deltaTime;
            if (_playerHitCooldownTimer < 0f)
            {
                _playerHitCooldownTimer = 0f;
            }
        }

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

        if (_lifelose.IsActive)
        {
            _player.UpdateExplosion(deltaTime);
            if (_lifelose.Update(deltaTime))
            {
                _player.ResetPosition();
            }
            return;
        }

        UpdateGameObjects(deltaTime);
        HandleShootInput(deltaTime); // 플레이어의 단발 입력 처리(동시 최대 2발)
        MoveEnemies(deltaTime); // 적 이동 처리
        if (PlaySceneEnemyService.UpdateEnemyCharge(_enemyChargeController, deltaTime, _enemies, _player)) // 적 돌진 처리
        {
            LoseLife();
        }
        HandleEnemyChargeCollision(); // 적 돌진(접촉) 충돌 처리
        HandleEnemyAttack(deltaTime); // 적의 간헐적 공격 처리
        ResolveCollisions();    // 총알과 적의 충돌 처리
        CleanupInactiveObjects();  // 비활성화된 게임 오브젝트 정리
        CheckEndConditions();   // 게임 종료 조건 확인
    }

    private void LoseLife()
    {
        if (_isGameOver)
        {
            return;
        }

        if (_playerHitCooldownTimer > 0f)
        {
            return;
        }

        _lifes--;
        _playerHitCooldownTimer = k_PlayerHitCooldown;
        _player.StartExplosion();

        if (_lifes <= 0)
        {
            _lifes = 0;
            _isGameOver = true;
            return;
        }

        PlaySceneEnemyService.ResetEnemiesForLifeLose(_enemyChargeController, _enemies);
        ClearAllBulletsForLifeLose();
        // 이동 패턴 카운터(_enemyDirection/_enemyMovesInCurrentPhase/_hasUsedInitialLeftPhase)는
        // 리셋하지 않아 READY 이후에도 직전 진행 상태를 이어서 사용한다.
        _lifelose.Begin();
    }

    public override void Draw(ScreenBuffer buffer)  // 게임 화면을 그리는 메서드, 게임 오브젝트를 그린 후 점수와 조작법을 표시하고, 게임 오버 또는 클리어 상태인 경우 메시지를 깜빡이도록 처리
    {
        DrawGameObjects(buffer);

        buffer.WriteText(45, 5, $"Score", ConsoleColor.Red);
        buffer.WriteText(48, 6, $"{_score}", ConsoleColor.White);
        buffer.WriteText(43, 15, $"Stage: {_stage}/{StageData.MaxStage}", ConsoleColor.White);
        buffer.WriteText(43, 0, "Move: Left/Right", ConsoleColor.DarkGray);
        buffer.WriteText(48, 1, "Fire: Space", ConsoleColor.DarkGray);

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


        if (_isGameOver)
        {
            if (_blinkCounter % 2 == 0)
            {
                buffer.WriteTextCentered(12, "GAME OVER", ConsoleColor.Red);
            }

            buffer.WriteTextCentered(14, $"Score: {_score}", ConsoleColor.White);
            buffer.WriteTextCentered(16, "Press ENTER to retry", ConsoleColor.White);
        }

        _lifelose.Draw(buffer);
    }
}