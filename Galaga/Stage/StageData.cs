using System;
using System.Collections.Generic;
// 게임의 각 스테이지에 대한 적 배치 정보를 관리하는 클래스, 최대 스테이지 수와 각 스테이지마다 적의 위치와 유형을 정의하여 반환하는 메서드를 포함
public readonly struct EnemySpawn
{
    public EnemySpawn(int x, int y, Enemy.EnemyType type)
    {
        X = x;
        Y = y;
        Type = type;
    }

    public int X { get; }
    public int Y { get; }
    public Enemy.EnemyType Type { get; }
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

    public static StageDefinition Get(int stageNumber)  //  
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

internal static class StageGridBuilder
{
    internal static LinkedList<EnemySpawn> BuildStandardGrid(int stage)
    {
        const int rows = 2;
        const int cols = 8;
        const int startX = 5;
        const int startY = 4;
        const int stepX = 4;
        const int stepY = 2;

        var spawns = new LinkedList<EnemySpawn>();

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                int x = startX + (col * stepX);
                int y = startY + (row * stepY);
                Enemy.EnemyType type = PickType(stage, row, col);
                spawns.AddLast(new EnemySpawn(x, y, type));
            }
        }

        return spawns;
    }

    private static Enemy.EnemyType PickType(int stage, int row, int col)
    {
        int pattern = (stage + row + col) % 6;

        if (pattern == 0)
        {
            return Enemy.EnemyType.Boss1;
        }
        if ((row + col + stage) % 2 == 0)
        {
            return Enemy.EnemyType.Goei;
        }

        return Enemy.EnemyType.Zako;
    }
}
