using System.Collections.Generic;

public static class Stage10
{
    public static StageDefinition Create()
    {
        LinkedList<EnemySpawn> enemies = StageGridBuilder.BuildStandardGrid(10);
        return new StageDefinition(10, enemies);
    }
}
