using System.Collections.Generic;

public static class Stage07
{
    public static StageDefinition Create()
    {
        LinkedList<EnemySpawn> enemies = StageGridBuilder.BuildStandardGrid(7);
        return new StageDefinition(7, enemies);
    }
}
