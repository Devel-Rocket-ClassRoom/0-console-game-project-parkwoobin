using System.Collections.Generic;

public static class Stage05
{
    public static StageDefinition Create()
    {
        LinkedList<EnemySpawn> enemies = StageGridBuilder.BuildStandardGrid(5);
        return new StageDefinition(5, enemies);
    }
}
