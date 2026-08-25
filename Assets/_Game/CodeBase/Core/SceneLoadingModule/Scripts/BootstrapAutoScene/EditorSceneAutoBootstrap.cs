using UnityEngine.SceneManagement;
using Zenject;

namespace _Game.CodeBase.Core.SceneLoadingModule.Scripts.BootstrapAutoScene
{
    public class EditorSceneAutoBootstrap : IInitializable
    {
        public const string BootstrapSceneName = "Bootstrap";
        public const string MenuSceneName = "MainMenu";
        public const string GameSceneName = "Game";
        private static string lastSceneName;
        
        public static string GetLastScene()
        {
            return lastSceneName;
        }
        public void Initialize()
        {
#if UNITY_EDITOR
            var currentScene = SceneManager.GetActiveScene().name;

            if (currentScene == BootstrapSceneName) return;

            lastSceneName = currentScene;
            
            SceneManager.LoadScene(BootstrapSceneName);
#endif
        }
    }
}