using Cysharp.Threading.Tasks;

namespace _Game.CodeBase.Core.SaveLoadService.Scripts
{
    public class FileSystemRepository
    {
        private readonly ISaveLoadService _saveLoadService;
        private const string SaveKey = "FileSystemState";

        public FileSystemRepository(ISaveLoadService saveLoadService)
        {
            _saveLoadService = saveLoadService;
        }

        public async UniTask Save(SaveDataBase data)
        {
            await _saveLoadService.Save(SaveKey, data);
        }

        public async UniTask<SaveDataBase> Load()
        {
            return await _saveLoadService.Load(SaveKey, new SaveDataBase());
        }
    }
}