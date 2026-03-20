using System.Collections.Generic;

public static class Stage03
{
    public static StageDefinition Create()
    {
        LinkedList<EnemySpawn> enemies = StageGridBuilder.BuildStandardGrid(3);
        return new StageDefinition(3, enemies);
    }
}
