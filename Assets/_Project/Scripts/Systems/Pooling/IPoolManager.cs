using System.Collections.Generic;
using _Project.Scripts.Systems.Base;
using UnityEngine;

namespace _Project.Scripts.Systems.Pooling
{
    public interface IPoolManager : IBaseManager
    {
        public void CreatePool(string poolKey, GameObject prefab, int initialSize = 5);
        public GameObject Get(string poolKey, Transform parent = null);
        public void ReturnToPool(string poolKey, List<GameObject> objects);
    }
}