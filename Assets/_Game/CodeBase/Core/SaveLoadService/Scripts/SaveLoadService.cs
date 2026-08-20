using Cysharp.Threading.Tasks;

namespace _Game.CodeBase.Core.SaveLoadService.Scripts
{
    public class SaveLoadService : ISaveLoadService
    {
        private readonly LocalSaveProvider _localProvider;
        private readonly CloudSaveProvider _cloudProvider;

        public SaveLoadService(LocalSaveProvider localProvider, CloudSaveProvider cloudProvider)
        {
            _localProvider = localProvider;
            _cloudProvider = cloudProvider;
        }

        public async UniTask Save<T>(string key, T data)
        {
            await _localProvider.SaveAsync(key, data);

            if (_cloudProvider.IsAvailable)
                await _cloudProvider.SaveAsync(key, data);
        }

        public async UniTask<T> Load<T>(string key, T defaultValue = default)
        {
            if (_cloudProvider.IsAvailable && await _cloudProvider.HasAsync(key))
            {
                var cloudData = await _cloudProvider.LoadAsync(key, defaultValue);
                await _localProvider.SaveAsync(key, cloudData); // синхронізуємо локальний кеш
                return cloudData;
            }

            return await _localProvider.LoadAsync(key, defaultValue);
        }
    }
}