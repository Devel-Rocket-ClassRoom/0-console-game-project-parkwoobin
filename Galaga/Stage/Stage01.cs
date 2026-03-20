using System.Collections.Generic;

public static class Stage01
{
    public static StageDefinition Create()
    {
        LinkedList<EnemySpawn> enemies = StageGridBuilder.BuildStandardGrid(1);
        return new StageDefinition(1, enemies);
    }
}
