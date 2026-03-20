using System.Collections.Generic;

public static class Stage02
{
    public static StageDefinition Create()
    {
        LinkedList<EnemySpawn> enemies = StageGridBuilder.BuildStandardGrid(2);
        return new StageDefinition(2, enemies);
    }
}
