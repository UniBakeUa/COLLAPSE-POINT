using Cysharp.Threading.Tasks;

namespace _Game.CodeBase.Core.SaveLoadService.Scripts
{
    public class CloudSaveProvider : ISaveProvider
    {
        public bool IsAvailable { get; private set; } = false; // поки cloud не готовий — завжди false

        public UniTask SaveAsync<T>(string key, T data)
        {
            // TODO: реалізувати підключення до cloud save
            return UniTask.CompletedTask;
        }

        public UniTask<T> LoadAsync<T>(string key, T defaultValue = default)
        {
            // TODO: реалізувати завантаження з cloud
            return UniTask.FromResult(defaultValue);
        }

        public UniTask<bool> HasAsync(string key) => UniTask.FromResult(false);

        public UniTask DeleteAsync(string key) => UniTask.CompletedTask;
    }
}