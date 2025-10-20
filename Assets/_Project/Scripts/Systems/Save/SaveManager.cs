using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

namespace _Project.Scripts.Systems.Save
{
    public class SaveManager : ISaveManager
    {
        public string LogTag => "[SaveManager]";

        #region Base

        public UniTask AsyncInitialize()
        {
            return UniTask.CompletedTask;
        }

        public void Initialize()
        {
            // Initialization logic if needed
        }

        public void Dispose() { }

        #endregion
        
       
        public void Save<T>(string key, T data)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                Debug.LogError($"{LogTag} Invalid key.");
                return;
            }

            var json = JsonConvert.SerializeObject(data);
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
        }
        
        public T Load<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                Debug.LogError($"{LogTag} Invalid key.");
                return default;
            }

            if (!PlayerPrefs.HasKey(key))
            {
                return default;
            }

            var json = PlayerPrefs.GetString(key);
            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"{LogTag} Load failed for key '{key}': {e}");
                return default;
            }
        }
        
        public void Delete(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }
            
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }
    }
}
