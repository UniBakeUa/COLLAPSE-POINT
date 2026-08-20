using UnityEngine;

namespace _Game.CodeBase.Core.SingletonModule.Scripts
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        public static T Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this as T)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this as T;
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this as T)
                Instance = null;
        }
    }
}