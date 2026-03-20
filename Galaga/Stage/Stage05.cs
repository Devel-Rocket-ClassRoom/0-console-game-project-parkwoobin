using System.Collections.Generic;

public static class Stage05
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
                if (col == 3 || col == 4)
                {
                    type = Enemy.EnemyType.Boss1;
                }
                else
                {
                    type = (row == 0) ? Enemy.EnemyType.Goei : Enemy.EnemyType.Zako;
                }

                EnemySpawnPattern pattern = (col % 2 == 0) ? EnemySpawnPattern.Top : EnemySpawnPattern.RightBottom;
                enemies.AddLast(new EnemySpawn(x, y, type, pattern));
            }
        }

        return new StageDefinition(5, enemies);
    }
}
