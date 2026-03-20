using System.Collections.Generic;

public static class Stage09
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

                int slotIndex = (row * 8) + col;

                Enemy.EnemyType type;
                if (slotIndex < 8)
                {
                    type = Enemy.EnemyType.Boss1;
                }
                else if (row == 0)
                {
                    type = Enemy.EnemyType.Goei;
                }
                else
                {
                    type = Enemy.EnemyType.Zako;
                }

                EnemySpawnPattern pattern = (slotIndex < 8)
                    ? ((col % 2 == 0) ? EnemySpawnPattern.LeftBottom : EnemySpawnPattern.RightBottom)
                    : EnemySpawnPattern.Top;
                enemies.AddLast(new EnemySpawn(x, y, type, pattern));
            }
        }

        return new StageDefinition(9, enemies);
    }
}
