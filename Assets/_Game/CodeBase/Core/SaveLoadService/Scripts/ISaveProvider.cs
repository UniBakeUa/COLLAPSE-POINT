using Cysharp.Threading.Tasks;

namespace _Game.CodeBase.Core.SaveLoadService.Scripts
{
    public interface ISaveProvider
    {
        UniTask SaveAsync<T>(string key, T data);
        UniTask<T> LoadAsync<T>(string key, T defaultValue = default);
        UniTask<bool> HasAsync(string key);
        UniTask DeleteAsync(string key);
    }
}