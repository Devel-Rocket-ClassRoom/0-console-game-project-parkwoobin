using System;

namespace Framework.Engine
{
    public abstract class GameObject
    {
        public string Name { get; set; } = "";
        public bool IsActive { get; set; } = true;  // 업데이트 draw 여부 결정하는 플래그
        public Scene Scene { get; } // 게임 오브젝트가 속한 신 참조

        protected GameObject(Scene scene)
        {
            Scene = scene;
        }

        public abstract void Update(float deltaTime);
        public abstract void Draw(ScreenBuffer buffer);
    }
}
