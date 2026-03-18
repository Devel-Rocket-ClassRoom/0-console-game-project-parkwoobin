using System;
using Framework.Engine;

public class Enemy : GameObject
{
    public int X { get; private set; }
    public int Y { get; private set; }

    public Enemy(Scene scene, int x, int y) : base(scene)
    {
        Name = "Enemy";
        X = x;
        Y = y;
    }

    public void MoveBy(int dx)
    {
        X += dx;
    }

    public override void Update(float deltaTime)
    {
    }

    public override void Draw(ScreenBuffer buffer)
    {
        if (!IsActive)
        {
            return;
        }

        buffer.SetCell(X - 1, Y, '[', ConsoleColor.Yellow);
        buffer.SetCell(X, Y, 'W', ConsoleColor.Red);
        buffer.SetCell(X + 1, Y, ']', ConsoleColor.Yellow);
    }
}
