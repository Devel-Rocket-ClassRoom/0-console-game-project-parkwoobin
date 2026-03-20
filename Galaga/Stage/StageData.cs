using System;
using System.Collections.Generic;
// 게임의 각 스테이지에 대한 적 배치 정보를 관리하는 클래스, 최대 스테이지 수와 각 스테이지마다 적의 위치와 유형을 정의하여 반환하는 메서드를 포함
//적은 각 스테이지마다 다르게 설정 Stage01~Stage10 클래스에서 스폰 패턴과 적 유형이 정의되어 있으며, StageData 클래스에서 해당 정보를 관리하여 게임 내에서 사용
public readonly struct EnemySpawn
{
    public EnemySpawn(int x, int y, Enemy.EnemyType type, EnemySpawnPattern pattern = EnemySpawnPattern.Top)
    {
        X = x;
        Y = y;
        Type = type;
        Pattern = pattern;
    }

    public int X { get; }
    public int Y { get; }
    public Enemy.EnemyType Type { get; }
    public EnemySpawnPattern Pattern { get; }
}

public enum EnemySpawnPattern
{
    Top = 0,
    Bottom = 1,
    Left = 2,
    Right = 3,
}

public sealed class StageDefinition
{
    public StageDefinition(int stageNumber, LinkedList<EnemySpawn> enemies)
    {
        StageNumber = stageNumber;
        Enemies = enemies;
    }

    public int StageNumber { get; }
    public LinkedList<EnemySpawn> Enemies { get; }
}

public static class StageData   // 게임의 각 스테이지에 대한 적 배치 정보를 관리하는 클래스, 최대 스테이지 수와 각 스테이지마다 적의 위치와 유형을 정의하여 반환하는 메서드를 포함
{
    public const int MaxStage = 10;

    private static readonly Dictionary<int, Func<StageDefinition>> s_stageLoaders = new Dictionary<int, Func<StageDefinition>>
    {
        { 1, Stage01.Create },
        { 2, Stage02.Create },
        { 3, Stage03.Create },
        { 4, Stage04.Create },
        { 5, Stage05.Create },
        { 6, Stage06.Create },
        { 7, Stage07.Create },
        { 8, Stage08.Create },
        { 9, Stage09.Create },
        { 10, Stage10.Create },
    };

    private static readonly Dictionary<int, StageDefinition> s_stages = BuildStages();

    public static StageDefinition Get(int stageNumber)  // 지정된 스테이지 번호에 해당하는 StageDefinition을 반환하는 메서드, 스테이지 번호가 유효하지 않은 경우 예외를 발생시킴
    {
        if (!s_stages.TryGetValue(stageNumber, out StageDefinition stage))
        {
            throw new ArgumentOutOfRangeException(nameof(stageNumber), "Stage number must be between 1 and 10.");
        }

        return stage;
    }

    private static Dictionary<int, StageDefinition> BuildStages()   // 각 스테이지에 대한 적 배치 정보를 생성하는 메서드, 최대 스테이지 수까지 반복하여 각 스테이지마다 적의 위치와 유형을 정의하여 반환
    {
        var stages = new Dictionary<int, StageDefinition>();

        foreach (KeyValuePair<int, Func<StageDefinition>> stageLoader in s_stageLoaders)
        {
            stages[stageLoader.Key] = stageLoader.Value();
        }

        return stages;
    }
}
