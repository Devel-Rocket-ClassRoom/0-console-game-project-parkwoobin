using System.Collections.Generic;

public static class Stage08
{
    public static StageDefinition Create()
    {
        LinkedList<EnemySpawn> enemies = StageGridBuilder.BuildStandardGrid(8);
        return new StageDefinition(8, enemies);
    }
}
