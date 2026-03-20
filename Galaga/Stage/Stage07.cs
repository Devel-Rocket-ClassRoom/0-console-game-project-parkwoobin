using System.Collections.Generic;

public static class Stage07
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
                if (slotIndex < 6)
                {
                    type = Enemy.EnemyType.Boss1;
                }
                else if (col % 2 == 0)
                {
                    type = Enemy.EnemyType.Goei;
                }
                else
                {
                    type = Enemy.EnemyType.Zako;
                }

                EnemySpawnPattern pattern = (row == 0) ? EnemySpawnPattern.Left : EnemySpawnPattern.Right;  //  위쪽 행은 왼쪽으로, 아래쪽 행은 오른쪽으로 이동하는 패턴
                enemies.AddLast(new EnemySpawn(x, y, type, pattern));
            }
        }

        return new StageDefinition(7, enemies);
    }
}
