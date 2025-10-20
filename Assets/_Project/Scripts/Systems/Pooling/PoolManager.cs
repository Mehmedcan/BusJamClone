using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Project.Scripts.Systems.Pooling
{
    public class PoolManager : IPoolManager
    {
        public string LogTag => "[PoolManager]";

        private readonly Dictionary<string, Stack<GameObject>> _pools = new();
        private readonly Dictionary<string, GameObject> _prefabs = new();
        
        private Transform _root;

        #region Base

        public UniTask AsyncInitialize()
        {
            return UniTask.CompletedTask;
        }

        public void Initialize()
        {
            var go = new GameObject("Pools");
            Object.DontDestroyOnLoad(go);
            _root = go.transform;
        }

        public void Dispose()
        {
            foreach (var pool in _pools.Values)
            {
                while (pool.Count > 0)
                {
                    var obj = pool.Pop();
                    Object.Destroy(obj);
                }
            }
            _pools.Clear();
            _prefabs.Clear();
            
            if (_root != null)
            {
                Object.Destroy(_root.gameObject);
                _root = null;
            }
        }


        #endregion
        
        
        public void CreatePool(string poolKey, GameObject prefab, int initialSize = 5)
        {
            if (!prefab)
            {
                Debug.LogError($"{LogTag} Cannot create pool for null prefab.");
                return;
            }

            EnsurePool(prefab, poolKey, initialSize);
            
            for (var i = _pools[poolKey].Count; i < initialSize; i++)
            {
                var inst = CreateInstance(prefab);
                inst.SetActive(false);
                _pools[poolKey].Push(inst);
            }
        }
        
        public GameObject Get(string poolKey, Transform parent = null)
        {
            if (string.IsNullOrEmpty(poolKey))
            {
                Debug.LogError($"{LogTag} Cannot get object for null or empty pool key.");
                return null;
            }
            
            if(_prefabs.Count == 0 || !_prefabs.TryGetValue(poolKey, out var prefab))
            {
                Debug.LogError($"{LogTag} Pool with key '{poolKey}' not found!");
                return null;
            }
            
            var poolObject = _pools[poolKey].Count > 0
                ? _pools[poolKey].Pop()
                : CreateInstance(prefab);

            poolObject.transform.SetParent(parent ? parent : _root, worldPositionStays: false);
            poolObject.SetActive(true);
            
            return poolObject;
        }
        
        public void ReturnToPool(string poolKey, List<GameObject> objects)
        {
            if (!_pools.TryGetValue(poolKey, out var poolQueue))
            {
                Debug.LogWarning($"{LogTag} Pool with key '{poolKey}' not found!");
                return;
            }

            foreach (var obj in objects)
            {
                if (obj == null) continue;

                obj.transform.SetParent(_root, worldPositionStays: false);
                obj.gameObject.SetActive(false);
                poolQueue.Push(obj.gameObject);
            }
        }
        
        // ----- helpers ----
        private void EnsurePool(GameObject prefab, string poolKey, int initialSizeCapacity = 4)
        {
            if (!_prefabs.ContainsKey(poolKey))
                _prefabs[poolKey] = prefab;

            if (!_pools.ContainsKey(poolKey))
                _pools[poolKey] = new Stack<GameObject>(Mathf.Max(4, initialSizeCapacity));
        }

        private GameObject CreateInstance(GameObject prefab)
        {
            var inst = Object.Instantiate(prefab, _root);
            inst.name = $"{prefab.name} (Pooled)";
            return inst;
        }
    }
}
