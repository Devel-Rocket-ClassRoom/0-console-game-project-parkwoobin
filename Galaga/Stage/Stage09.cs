using System.Collections.Generic;

public static class Stage09
{
    public static StageDefinition Create()
    {
        LinkedList<EnemySpawn> enemies = StageGridBuilder.BuildStandardGrid(9);
        return new StageDefinition(9, enemies);
    }
}
