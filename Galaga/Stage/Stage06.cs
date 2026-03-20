using System.Collections.Generic;

public static class Stage06
{
    public static StageDefinition Create()
    {
        LinkedList<EnemySpawn> enemies = StageGridBuilder.BuildStandardGrid(6);
        return new StageDefinition(6, enemies);
    }
}
