using System.Collections.Generic;

public static class Stage08
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
                if (slotIndex < 7)
                {
                    type = Enemy.EnemyType.Boss1;
                }
                else
                {
                    type = Enemy.EnemyType.Goei;
                }

                EnemySpawnPattern pattern;
                switch ((row + col) % 5)
                {
                    case 0:
                        pattern = EnemySpawnPattern.Top;
                        break;
                    case 1:
                        pattern = EnemySpawnPattern.LeftTop;
                        break;
                    case 2:
                        pattern = EnemySpawnPattern.LeftBottom;
                        break;
                    case 3:
                        pattern = EnemySpawnPattern.RightTop;
                        break;
                    default:
                        pattern = EnemySpawnPattern.RightBottom;
                        break;
                }
                enemies.AddLast(new EnemySpawn(x, y, type, pattern));
            }
        }

        return new StageDefinition(8, enemies);
    }
}
