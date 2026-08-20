using Cysharp.Threading.Tasks;

namespace _Game.CodeBase.Core.SaveLoadService.Scripts
{
    public interface ISaveLoadService
    {
        UniTask Save<T>(string key, T data);
        UniTask<T> Load<T>(string key, T defaultValue = default);
    }
}