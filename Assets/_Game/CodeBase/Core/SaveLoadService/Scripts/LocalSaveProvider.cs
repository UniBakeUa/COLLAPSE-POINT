using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Game.CodeBase.Core.SaveLoadService.Scripts
{
    public class LocalSaveProvider : ISaveProvider
    {
        private readonly string _rootPath;

        public LocalSaveProvider()
        {
            _rootPath = Path.Combine(Application.persistentDataPath, "SavedData");

            if (!Directory.Exists(_rootPath))
                Directory.CreateDirectory(_rootPath);
        }

        public async UniTask SaveAsync<T>(string key, T data)
        {
            var json = JsonUtility.ToJson(data);
            await File.WriteAllTextAsync(GetPath(key), json);
        }

        public async UniTask<T> LoadAsync<T>(string key, T defaultValue = default)
        {
            var path = GetPath(key);

            if (!File.Exists(path))
                return defaultValue;

            try
            {
                var json = await File.ReadAllTextAsync(path);
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalSaveProvider] Failed to load '{key}': {e}");
                return defaultValue;
            }
        }

        public UniTask<bool> HasAsync(string key) => UniTask.FromResult(File.Exists(GetPath(key)));

        public UniTask DeleteAsync(string key)
        {
            var path = GetPath(key);

            if (File.Exists(path))
                File.Delete(path);

            return UniTask.CompletedTask;
        }

        private string GetPath(string key) => Path.Combine(_rootPath, $"{key}.json");
    }
}