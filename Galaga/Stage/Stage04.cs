using System.Collections.Generic;

public static class Stage04
{
    public static StageDefinition Create()
    {
        LinkedList<EnemySpawn> enemies = StageGridBuilder.BuildStandardGrid(4);
        return new StageDefinition(4, enemies);
    }
}
