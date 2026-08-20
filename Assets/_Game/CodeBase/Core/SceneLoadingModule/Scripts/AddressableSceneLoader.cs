using System;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace _Game.Core.SceneLoadingModule.Scripts
{
    public class AddressableSceneLoader
    {
        private AsyncOperationHandle<SceneInstance> _currentHandle;
        private bool _isLoaded;

        public async UniTask LoadAsync(string addressableKey, IProgress<float> progress = null)
        {
            if (_isLoaded)
                throw new InvalidOperationException($"Scene already loaded, call {nameof(UnloadAsync)} first");

            _currentHandle = Addressables.LoadSceneAsync(addressableKey, LoadSceneMode.Additive);

            // Заділка під loading screen: якщо передати IProgress<float>,
            // прогрес буде оновлюватись під час завантаження.
            // Зараз просто чекаємо завершення без проміжних апдейтів.
            await _currentHandle.ToUniTask(progress: progress);

            _isLoaded = true;
        }

        public async UniTask UnloadAsync()
        {
            if (!_isLoaded) return;

            await Addressables.UnloadSceneAsync(_currentHandle).ToUniTask();
            _isLoaded = false;
        }
    }
}