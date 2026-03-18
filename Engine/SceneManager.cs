namespace Framework.Engine
{
    public class SceneManager<TScene> where TScene : Scene
    {
        private TScene _currentScene;

        public event GameAction<TScene> SceneChanged;   // 신이 변경될 때마다 발생하는 이벤트, 구독자에게 새로운 신을 전달

        public TScene CurrentScene => _currentScene;

        public void ChangeScene(TScene scene)
        {
            if (_currentScene != null)
            {
                _currentScene.Unload();
            }
            _currentScene = scene;
            SceneChanged?.Invoke(_currentScene);
            _currentScene.Load();
        }
    }
}
