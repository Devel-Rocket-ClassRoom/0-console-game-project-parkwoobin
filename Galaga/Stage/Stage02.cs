using System.Collections.Generic;

public static class Stage02
{
    public static StageDefinition Create()
    {
        LinkedList<EnemySpawn> enemies = new LinkedList<EnemySpawn>();

        const int startX = 5;
        const int startY = 4;
        const int stepX = 4;
        const int stepY = 2;

        for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                int x = startX + (col * stepX);
                int y = startY + (row * stepY);

                Enemy.EnemyType type;
                if (col % 2 == 0)
                {
                    type = Enemy.EnemyType.Goei;
                }
                else
                {
                    type = Enemy.EnemyType.Zako;
                }

                EnemySpawnPattern pattern = EnemySpawnPattern.LeftBottom;
                enemies.AddLast(new EnemySpawn(x, y, type, pattern));
            }
        }

        return new StageDefinition(2, enemies);
    }
}
